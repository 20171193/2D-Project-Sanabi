using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
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

    [Space(3)]
    [Header("Specs")]
    [Space(2)]

    [SerializeField]
    private float movePower;
    public float MovePower { get { return movePower; } }

    [SerializeField]
    private float flyMovePower;
    public float FlyMovePower { get { return flyMovePower; } }

    [SerializeField]
    private float ropeMovePower;
    public float RopeMovePower { get { return ropeMovePower; } }

    [SerializeField]
    private float ropeAccelerationPower; 
    public float RopeAccelerationPower { get { return ropeAccelerationPower; } }

    [SerializeField]
    private float maxMoveSpeed;
    public float MaxMoveSpeed { get { return maxMoveSpeed; } }

    [SerializeField]
    private float jumpPower;
    public float JumpPower { get { return jumpPower; } }


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

    [Space(3)]
    [Header("Layer")]
    [Space(2)]
    [SerializeField]
    private LayerMask groundLM; // ground check layermask
    public LayerMask GroundLM { get { return groundLM; } }

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private bool isJointed = false; 
    public bool IsJointed { get { return isJointed; } set { isJointed = value; } }

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
    private float moveHzt;  // Keyboard input - 'A', 'D'
    public float MoveHzt { get { return moveHzt; } }

    [SerializeField]
    private float moveVtc;  // Keyboard input - 'W', 'S'
    public float MoveVtc { get { return moveVtc; } }

    [SerializeField]
    private float inputJumpPower;   

    [SerializeField]
    private GameObject jointedOB;

    private void Awake()
    {
        mainCamera = Camera.main;
        lr = GetComponent<LineRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        fsm = new StateMachine<PlayerAction>(this);

        fsm.AddState("Idle", new PlayerIdle(this));
        fsm.AddState("Run", new PlayerRun(this));
        fsm.AddState("RunStop", new PlayerRunStop(this));
        fsm.AddState("Fall", new PlayerFall(this));
        fsm.AddState("Jump", new PlayerJump(this));
        fsm.AddState("Rope", new PlayerRope(this));

        fsm.AddAnyState("Jump", () =>
        {
            return !isGround && !isJointed && rigid.velocity.y > JumpForce_Threshold;
        });
        fsm.AddAnyState("Fall", () =>
        {
            return !isGround && !isJointed && rigid.velocity.y < -JumpForce_Threshold;
        });
        fsm.AddAnyState("Rope", () =>
        {
            return isJointed;
        });

        fsm.AddTransition("Fall", "Idle", 0f, () =>
        {
            return isGround;
        });
        // 입력을 받은 경우 Idle -> Run 상태
        // 입력이 없는 경우 Run -> RunStop 
        // 바로 입력을 받은 경우 Run -> RunStop
        //  멈춘 경우 RunStop -> Idle 
        fsm.AddTransition("Idle", "Run", 0f, () =>
        {
            return Mathf.Abs(moveHzt) > MoveForce_Threshold;
        });
        fsm.AddTransition("Run", "RunStop", 0f, () =>
        {
            return Mathf.Abs(moveHzt) == 0;
        });
        fsm.AddTransition("RunStop", "Run", 0f, () =>
        {
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
        fsm.FixedUpdate();
    }

    #region Input Action

    #region Normal Movement
    private void OnMove(InputValue value)
    {
        moveHzt = value.Get<Vector2>().x;
        moveVtc = value.Get<Vector2>().y;
    }
    private void OnJump(InputValue value)
    {
        if(isGround || isJointed)
            Jump();
    }
    private void Jump()
    {
        if (isJointed)
        {
            Destroy(jointedOB);
            isJointed = false;
        }

        rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y + jumpPower);
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
        Debug.Log("RopeForce! : " + rigid.transform.forward);
        Vector2 forceDir = transform.rotation.y == 0 ? Vector2.right : Vector2.left;
        rigid.AddForce(ropeAccelerationPower * forceDir, ForceMode2D.Impulse);
    }
    #endregion

    #endregion

    #region Collision Callback
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.layer);

        if(groundLM.Contain(collision.gameObject.layer))
        {
            // ground check
            isGround = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (groundLM.Contain(collision.gameObject.layer))
        {
            // ground check
            isGround = false;
        }
    }
    #endregion
}
