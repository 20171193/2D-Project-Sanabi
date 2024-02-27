using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings.SplashScreen;

public class PlayerAction : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }

    [SerializeField]
    private Animator anim;
    public Animator Anim { get { return anim; } }

    [SerializeField]
    private Camera mainCamera;

    // + mouse controller
    [SerializeField]
    private GameObject cursorOb;

    // + hooker
    [SerializeField]
    private HookAim hookAim;

    [SerializeField]
    private Hook hookPrefab;

    #region Specs
    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    #region Normal Movement
    [SerializeField]
    private float movePower;
    public float MovePower { get { return movePower; } }

    [SerializeField]
    private float maxMoveSpeed;
    public float MaxMoveSpeed { get { return maxMoveSpeed; } }

    [SerializeField]
    private float jumpPower;
    public float JumpPower { get { return jumpPower; } }

    [SerializeField]
    private float flyMovePower;
    public float FlyMovePower { get { return flyMovePower; } }
    #endregion

    #region Rope Movement
    [SerializeField]
    private float ropeMovePower;
    public float RopeMovePower { get { return ropeMovePower; } }

    [SerializeField]
    private float ropeAccelerationPower; 
    public float RopeAccelerationPower { get { return ropeAccelerationPower; } }
    #endregion

    [SerializeField]
    private float hookShootPower;
    public float HookShootPower { get { return hookShootPower; } }

    [SerializeField]
    private float hookShootCoolTime;
    public float HookShootCoolTime { get { return hookShootCoolTime; } }

    #endregion

    [ReadOnly(true)]
    public float MoveForce_Threshold = 0.1f;

    [ReadOnly(true)]
    public float JumpForce_Threshold = 0.05f;

    [Space(3)]
    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    private StateMachine<PlayerAction> fsm; // Player finite state machine
    public StateMachine<PlayerAction> FSM { get { return fsm; } }

    #region Ballancing
    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private bool isGround; 
    public bool IsGround { get { return isGround; } }

    [SerializeField]
    private float hztBrakePower;    // horizontal movement brake force
    public float HztBrakePower { get { return hztBrakePower; } }

    [SerializeField]
    private float vtcBrakePower;    // vertical movement brake force
    public float VtcBrakePower { get { return vtcBrakePower; } }

    [SerializeField]
    private float moveHzt;  // Keyboard input - 'A', 'D' *Ground Movement
    public float MoveHzt { get { return moveHzt; } }

    [SerializeField]
    private float moveVtc;  // Keyboard input - 'W', 'S' *Wall Movement 
    public float MoveVtc { get { return moveVtc; } }

    [SerializeField]
    private float inputJumpPower;

    [SerializeField]
    private bool isJointed = false;
    public bool IsJointed { get { return isJointed; } set { isJointed = value; } }

    [SerializeField]
    private bool isHookShoot = false;
    public bool IsHookShoot { get { return isHookShoot; } }

    [SerializeField]
    private bool isDash = false;
    public bool IsDash { get { return isDash; } }

    [SerializeField]
    private bool isGrab = false;
    public bool IsGrab { get { return isGrab; } }


    [SerializeField]
    private Hook firedHook;
    public Hook FiredHook { get { return firedHook; } }

    [SerializeField]
    private bool isRaycastHit = false;

    [SerializeField]
    private RaycastHit2D hookHitInfo;

    [SerializeField]
    private float ropeLength;  // Raycast distance
    public float RopeLength { get { return ropeLength; } }

    [SerializeField]
    private Vector3 mousePos;

    [SerializeField]
    private Enemy grabEnemy;
    public Enemy GrabEnemy { get { return grabEnemy; } }

    [SerializeField]
    private Animation test;
    #endregion

    private void Awake()
    {
        mainCamera = Camera.main;
        rigid = GetComponent<Rigidbody2D>();
        fsm = new StateMachine<PlayerAction>(this);

        fsm.AddState("Idle", new PlayerIdle(this));
        fsm.AddState("Run", new PlayerRun(this));
        fsm.AddState("RunStop", new PlayerRunStop(this));
        fsm.AddState("Fall", new PlayerFall(this));
        fsm.AddState("Jump", new PlayerJump(this));
        fsm.AddState("Roping", new PlayerRoping(this));
        fsm.AddState("Dash", new PlayerDash(this));
        fsm.AddState("Grab", new PlayerGrab(this));

        fsm.AddAnyState("Jump", () =>
        {
            return !isGround && !isJointed && rigid.velocity.y > JumpForce_Threshold;
        });
        fsm.AddAnyState("Fall", () =>
        {
            return !isGround && !isJointed && rigid.velocity.y < -JumpForce_Threshold;
        });
        fsm.AddTransition("Fall", "Idle", 0f, () =>
        {
            return isGround;
        });

        fsm.AddTransition("Idle", "Run", 0f, () =>
        {
            // is input Keyboard "A"key or "D" key
            return Mathf.Abs(moveHzt) > MoveForce_Threshold;
        });
        fsm.AddTransition("Run", "RunStop", 0f, () =>
        {
            // isn't input Keyboard "A"key or "D" key
            return Mathf.Abs(moveHzt) == 0;
        });
        fsm.AddTransition("RunStop", "Run", 0f, () =>
        {
            // input swap "A"key -> "D"key or "D"key -> "A"key
            return Mathf.Abs(moveHzt) > MoveForce_Threshold;
        });
        fsm.AddTransition("RunStop", "Idle", 0.2f, () =>
        {
            return Mathf.Abs(moveHzt) <= MoveForce_Threshold;
        });

        fsm.Init("Idle");
    }

    private void Update()
    {
        fsm.Update();
    }

    private void FixedUpdate()
    {
        fsm.FixedUpdate();
    }
    private void LateUpdate()
    {
        fsm.LateUpdate();
    }

    #region Input Action
    #region Normal Movement
    // Keyboard Acition
    // move horizontally with keyboard "a" key and "d" key
    // jump with keyboard "space" key
    // +rope jump 
    private void OnMove(InputValue value)
    {
        moveHzt = value.Get<Vector2>().x;
        moveVtc = value.Get<Vector2>().y;
    }
    private void OnJump(InputValue value)
    {
        if (isJointed || isDash || isGrab) return;

        if (isGround)
            Jump();
    }

    // normal jumpping
    private void Jump()
    {
        anim.Play("Jump");
        rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y + jumpPower);
    }

    #endregion

    #region Mouse / Rope Action
    // Raycast to mouse position
    private void OnMousePos(InputValue value)
    {
        // cursorPos is mousePos
        // +Linerendering
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // move cursor
        cursorOb.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        HookAimSet();

        if (!isJointed && !isGrab && !isDash)
            RopeRayCast();
        else
            hookAim.LineOff();
    }
    // if Raycast hit is not null, linerendering to hit.point
    private void RopeRayCast()
    {
        Vector2 rayDir = (mousePos-transform.position).normalized;
        hookHitInfo = Physics2D.Raycast(transform.position, rayDir, ropeLength, Manager.Layer.hookInteractableLM);

        if (hookHitInfo)
        {
            isRaycastHit = true;

            // hit is Enemy
            if (Manager.Layer.enemyLM.Contain(hookHitInfo.collider.gameObject.layer))
                hookAim.LineOn(LineRenderType.Enemy, hookHitInfo.point);
            // hit is Ground
            else
                hookAim.LineOn(LineRenderType.Ground, hookHitInfo.point);
        }
        else
        {
            isRaycastHit = false;
            hookAim.LineOff();
        }

    }
    // hookshot to mouse position
    private void OnMouseClick(InputValue value)
    {
        if (value.isPressed)
        {
            if (!IsJointed && isRaycastHit)
                HookShoot();
        }
        else
        {
            if (isJointed)
            {
                isJointed = false;
                firedHook?.DisConnecting();
                RopeJump();
            }
        }
    }

    private void RopeJump()
    {
        rigid.AddForce(hookAim.transform.up * 15f, ForceMode2D.Impulse);
        anim.Play("RopeJump");
    }

    #endregion

    #region Skills
    private void OnRopeForce(InputValue value)
    {
        if (isJointed)
            RopeForce();
    }
    private void RopeForce()
    {
        // 강한 반동 적용
        // 잔상 등 이펙트 추가
        Vector2 forceDir = transform.rotation.y == 0 ? Vector2.right : Vector2.left;
        rigid.AddForce(ropeAccelerationPower * forceDir, ForceMode2D.Impulse);
    }
    #endregion
    #endregion

    #region Hooking
    private void HookAimSet()
    {
        Vector3 dist = mousePos - transform.position;
        float zRot = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;

        hookAim.transform.rotation = Quaternion.Euler(0, 0, zRot - 90f);
        hookAim.transform.position = transform.position + hookAim.transform.up*1.7f;
    }

    // if hook collide with enemy, Invoke OnGrabbedEnemy
    // else if hook collide with ground, Invoke OnGrabbedGround
    private void HookShoot()
    {
        isHookShoot = true;
        hookAim.LineOff();
        anim.Play("RopeShot");

        firedHook = Instantiate(hookPrefab, hookAim.transform.position, hookAim.transform.rotation);
        FiredHookInitialSetting(firedHook);

        // Hook shoot
        firedHook.Rigid?.AddForce(hookAim.transform.position.GetDirectionTo2DTarget(hookHitInfo.point) * hookShootPower, ForceMode2D.Impulse);
    }
    private void FiredHookInitialSetting(Hook hook)
    {
        // assign player rigidbody2D for DistanceJoint2D
        hook.OwnerRigid = rigid;

        // hook action setting
        hook.OnDestroyHook += HookReloading;
        hook.OnHookHitEnemy += HookHitEnemy;
        hook.OnHookHitGround += HookHitGround;

        // Convex Collision Detection setting
        // Time = Distance / Velocity
        hook.ccdRoutine = hook.StartCoroutine(hook.CCD(ropeLength / hookShootPower, new Vector3(hookHitInfo.point.x, hookHitInfo.point.y, 0)));

        // destroy by no collision
        hook.destroyTime = hookShootCoolTime;
    }
    #endregion
    #region Hooking Action
    private void HookReloading()
    {
        isHookShoot = false;
    }
    private void HookHitGround()
    {
        StopCoroutine(firedHook.ccdRoutine);

        isJointed = true;
        fsm.ChangeState("Roping");
    }
    private void HookHitEnemy(GameObject enemy)
    {
        StopCoroutine(firedHook.ccdRoutine);

        Dash(enemy);
    }

    private void Dash(GameObject target)
    {
        isDash = true;
        rigid.velocity = Vector3.zero;
        rigid.gravityScale = 0;

        fsm.ChangeState("Dash");
       
        rigid.AddForce((target.transform.position - transform.position).normalized * 50f, ForceMode2D.Impulse);
        
        // add Player CCD
        // if player and enemy not collide,
        // Dash -> Idle 
    }
    private void Grab(GameObject target)
    {
        // Check Enemy
        grabEnemy = target.GetComponent<Enemy>();
        if (grabEnemy == null)
        {
            Debug.Log("Grabbed Object is not Enemy");
            fsm.ChangeState("Idle");
            return;
        }

        isGrab = true;

        rigid.velocity = Vector3.zero;
        Vector3 enemyPos = grabEnemy.transform.position;
        transform.position = new Vector3(enemyPos.x, enemyPos.y + GrabEnemy.GrabbedYPos, 0);

        fsm.ChangeState("Grab");
        grabEnemy.Grabbed();
    }
    #endregion

    #region Collision Callback
    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(Manager.Layer.groundLM.Contain(collision.gameObject.layer))
        {
            // ground check
            isGround = true;
        }

        // Dash Enemy
        if (isDash && Manager.Layer.enemyLM.Contain(collision.gameObject.layer))
        {
            isDash = false;
            Grab(collision.gameObject);
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Manager.Layer.groundLM.Contain(collision.gameObject.layer))
        {
            // ground check
            isGround = false;
        }
    }
    #endregion
}
