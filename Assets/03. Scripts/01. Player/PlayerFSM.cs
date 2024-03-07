using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;

public class PlayerFSM : PlayerBase
{
    [Space(3)]
    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    private StateMachine<PlayerBase> fsm; // Player finite state machine
    public StateMachine<PlayerBase> FSM { get { return fsm; } }

    #region check extra State
    [SerializeField]
    private bool isGround;
    public bool IsGround { get { return isGround; } set { isGround = value; } }

    [SerializeField]
    private bool isInWall;
    public bool IsInWall { get { return isInWall; } set { isInWall = value; } }

    [SerializeField]
    private bool isJointed = false;
    public bool IsJointed { get { return isJointed; } set { isJointed = value; } }

    [SerializeField]
    private bool isDash = false;
    public bool IsDash { get { return isDash; } set { isDash = value; } }

    [SerializeField]
    private bool isGrab = false;
    public bool IsGrab { get { return isGrab; } set { isGrab = value; } }

    [SerializeField]
    private bool isEnableGrabMove = false;
    public bool IsEnableGrabMove { get { return isEnableGrabMove; } set { isEnableGrabMove = value; } }

    [SerializeField]
    private bool isHookShoot = false;
    public bool IsHookShoot { get { return isHookShoot; } set { isHookShoot = value; } }

    [SerializeField]
    private bool isRaycastHit = false;
    public bool IsRaycastHit { get { return isRaycastHit; }set { isRaycastHit = value; } }

    [SerializeField]
    private bool beDamaged = false;
    public bool BeDamaged { get { return beDamaged; } set { beDamaged = value; } }
    #endregion

    [Space(3)]
    [Header("Player Action Events")]
    [Space(2)]
    public UnityEvent OnRun;            // invoke by state
    public UnityEvent OnJump;           // invoke by PlayerMover (Jump())
    public UnityEvent OnWallJump;       // invoke by PlayerMover (WallJump())
    public UnityEvent OnDash;           // invoke by state
    public UnityEvent OnGrabEnd;        // invoke by state
    public UnityEvent OnClimb;          // invoke by state
    public UnityEvent OnWallSliding;    // invoke by state
    public UnityEvent OnHookShoot;      // invoke by PlayerHooker (HookShoot)
    public UnityEvent OnTakeDamage;     // invoke by state
    public UnityEvent OnLanding;        // invoke by PlayerMover (TriggerEnter2D)

    protected override void Awake()
    {
        base.Awake();

        fsm = new StateMachine<PlayerBase>(this);
        fsm.AddState("Idle", new PlayerIdle(this));
        fsm.AddState("Damaged", new PlayerDamaged(this));
        fsm.AddState("Run", new PlayerRun(this));
        fsm.AddState("RunStop", new PlayerRunStop(this));
        fsm.AddState("Fall", new PlayerFall(this));
        fsm.AddState("Jump", new PlayerJump(this));
        fsm.AddState("Roping", new PlayerRoping(this));
        fsm.AddState("Dash", new PlayerDash(this));
        fsm.AddState("Grab", new PlayerGrab(this));
        fsm.AddState("WallSlide", new PlayerWallSlide(this));

        fsm.AddAnyState("Damaged", () =>
        {
            return beDamaged;
        });
        fsm.AddTransition("Damaged", "Idle", 0f, () =>
        {
            return !beDamaged;
        });
        fsm.AddAnyState("Roping", () =>
        {
            return !beDamaged
                    && isJointed;
        });
        fsm.AddAnyState("WallSlide", () =>
        {
            return !beDamaged && !isDash && !isGrab 
                    && isInWall;
        });

        fsm.AddAnyState("Jump", () =>
        {
            return !isGrab && !beDamaged && !isInWall && !isGround && !isJointed 
                    && rigid.velocity.y > JumpForce_Threshold;
        });

        fsm.AddTransition("Jump", "Fall", 0f, () =>
        {
            return rigid.velocity.y < -JumpForce_Threshold;
        });

        fsm.AddAnyState("Fall", () =>
        {
            return !isGrab && !beDamaged && !isInWall && !isGround && !isJointed 
                    && rigid.velocity.y < -JumpForce_Threshold;
        });

        fsm.AddTransition("WallSlide", "Idle", 0f, () =>
        {
            return isGround || !isInWall;
        });

        fsm.AddTransition("Fall", "Idle", 0f, () =>
        {
            return isGround;
        });
        fsm.AddTransition("Idle", "Run", 0f, () =>
        {
            // is input Keyboard "A"key or "D" key
            return Mathf.Abs(playerMover.MoveHzt) > MoveForce_Threshold;
        });
        fsm.AddTransition("Run", "RunStop", 0f, () =>
        {
            // isn't input Keyboard "A"key or "D" key
            return Mathf.Abs(playerMover.MoveHzt) == 0;
        });
        fsm.AddTransition("RunStop", "Run", 0f, () =>
        {
            // input swap "A"key -> "D"key or "D"key -> "A"key
            return Mathf.Abs(playerMover.MoveHzt) > MoveForce_Threshold;
        });
        fsm.AddTransition("RunStop", "Idle", 0.2f, () =>
        {
            return Mathf.Abs(playerMover.MoveHzt) <= MoveForce_Threshold;
        });

        fsm.Init("Idle");
    }

    public void ChangeState(string name)
    {
        fsm.ChangeState(name);
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

    private void TakeDamage()
    {
        DoImpulse();

        beDamaged = true;
        PrHooker.FiredHook?.DisConnecting();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Manager.Layer.enemyBulletLM.Contain(collision.gameObject.layer))
        {
            TakeDamage();
            return;
        }

        if (Manager.Layer.damageGroundLM.Contain(collision.gameObject.layer))
        {
            rigid.velocity = Vector3.zero;
            rigid.AddForce(collision.gameObject.transform.right * 12f + rigid.transform.right * -5f, ForceMode2D.Impulse);
            TakeDamage();
        }

        if (Manager.Layer.groundLM.Contain(collision.gameObject.layer))
        {
            // ground check
            if (isInWall)
                isInWall = false;
            else
                OnLanding?.Invoke();

            isGround = true;
            return;
        }

        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer))
        {
            if (isGround) return;

            isInWall = true;
            fsm.ChangeState("WallSlide");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Manager.Layer.groundLM.Contain(collision.gameObject.layer))
        {
            // ground check
            isGround = false;
        }

        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer))
        {
            isInWall = false;
            PrHooker.FiredHook?.DisConnecting();

            if (PrMover.MoveVtc > 0)
            {
                anim.Play("Jump");

                // Wall Exit to Up Position
                OnWallJump?.Invoke();
                rigid.AddForce(transform.up * 8f + transform.right * 3f, ForceMode2D.Impulse);
            }
        }
    }
}
