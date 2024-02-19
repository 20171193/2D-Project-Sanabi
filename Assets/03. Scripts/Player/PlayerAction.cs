using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
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

    [Space(3)]
    [Header("Specs")]
    [Space(2)]

    [SerializeField]
    private float movePower;
    public float MovePower { get { return movePower; } }

    [SerializeField]
    private float maxMoveSpeed;
    public float MaxMoveSpeed { get { return maxMoveSpeed; } }


    [SerializeField]
    private float jumpPower;
    public float JumpPower { get { return jumpPower; } }


    // 수평이동 힘 임계치
    [ReadOnly(true)]
    public float MoveForce_Threshold = 0.1f;

    // 수직이동 힘 임계치
    [ReadOnly(true)]
    public float JumpForce_Threshold = 0.05f;


    [Space(3)]
    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    private StateMachine<PlayerAction> fsm;
    public StateMachine<PlayerAction> FSM { get { return fsm; } }

    [Space(3)]
    [Header("Layer")]
    [Space(2)]
    [SerializeField]
    private LayerMask groundLM;
    public LayerMask GroundLM { get { return groundLM; } }


    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    // 로프 액션 조인트되어있는 상태인지 체크
    [SerializeField]
    private bool isJointed = false;
    public bool IsJointed { get { return isJointed; } }

    [SerializeField]
    private bool isGround;
    public bool IsGround { get { return isGround; } }


    // 좌우 정지력
    [SerializeField]
    private float hztBrakePower;
    public float HztBrakePower { get { return hztBrakePower; } }

    // 상하 정지력
    [SerializeField]
    private float vtcBrakePower;
    public float VtcBrakePower { get { return vtcBrakePower; } }

    [SerializeField]
    private float moveHzt;
    public float MoveHzt { get { return moveHzt; } }

    [SerializeField]
    private float moveVtc;
    public float MoveVtc { get { return moveVtc; } }

    [SerializeField]
    private float inputJumpPower;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        fsm = new StateMachine<PlayerAction>(this);

        fsm.AddState("Idle", new PlayerIdle(this));
        fsm.AddState("Run", new PlayerRun(this));
        fsm.AddState("RunStop", new PlayerRunStop(this));
        fsm.AddState("Fall", new PlayerFall(this));
        fsm.AddState("Jump", new PlayerJump(this));

        fsm.AddAnyState("Jump", () =>
        {
            return !isGround && !isJointed && rigid.velocity.y > JumpForce_Threshold;
        });

        fsm.AddAnyState("Fall", () =>
        {
            return !isGround && !isJointed && rigid.velocity.y < -JumpForce_Threshold;
        });
        fsm.AddTransition("Fall", "Idle",0f, () =>
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
    private void OnMove(InputValue value)
    {
        moveHzt = value.Get<Vector2>().x;
        moveVtc = value.Get<Vector2>().y;
    }

    private void OnJump(InputValue value)
    {
        if(isGround)
            Jump();
    }
    private void Jump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y + jumpPower);
    }
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
