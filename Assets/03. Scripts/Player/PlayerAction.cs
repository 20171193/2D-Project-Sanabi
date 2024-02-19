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
    private LineRenderer lr;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private GameObject arm;

    [SerializeField]
    private GameObject cursorOB;

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
    private float maxMoveSpeed;
    public float MaxMoveSpeed { get { return maxMoveSpeed; } }

    [SerializeField]
    private float jumpPower;
    public float JumpPower { get { return jumpPower; } }


    // �����̵� �� �Ӱ�ġ
    [ReadOnly(true)]
    public float MoveForce_Threshold = 0.1f;

    // �����̵� �� �Ӱ�ġ
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

    [SerializeField]
    private LayerMask ropeInteractableLM;
    public LayerMask RopeInteractableLM { get { return ropeInteractableLM; } }


    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]

    [SerializeField]
    RaycastHit2D ropeHit;

    // ���� �׼� ����Ʈ�Ǿ��ִ� �������� üũ
    [SerializeField]
    private bool isJointed = false;
    public bool IsJointed { get { return isJointed; } }

    [SerializeField]
    private bool isGround;
    public bool IsGround { get { return isGround; } }


    // �¿� ������
    [SerializeField]
    private float hztBrakePower;
    public float HztBrakePower { get { return hztBrakePower; } }

    // ���� ������
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

    [SerializeField]
    private Vector3 mousePos;

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


        
        // �Է��� ���� ��� Idle -> Run ����
        // �Է��� ���� ��� Run -> RunStop 
        // �ٷ� �Է��� ���� ��� Run -> RunStop
        //  ���� ��� RunStop -> Idle 

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

    private void OnMousePos(InputValue value)
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // ���콺 Ŀ�� �̵�
        cursorOB.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
        RopeRayCast();
        Debug.Log(mousePos);
    }
    private void OnMouseClick(InputValue value)
    {
        RopeShoot();
    }

    private void RopeRayCast()
    {
        Vector2 rayDir = (mousePos - transform.position).normalized;
        ropeHit = Physics2D.Raycast(transform.position, rayDir, 100f, ropeInteractableLM);

        if (ropeHit)
        { 
            Debug.Log("hit!");
            lr.positionCount = 2;
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, ropeHit.point);
            DrawDummyRope();
        }
        else
        {
            lr.positionCount = 0;
        }
    }
    private void DrawDummyRope()
    {

    }
    private void RopeShoot()
    {
        if(ropeHit)
        {
            GameObject hitObj = ropeHit.transform.gameObject;
            DistanceJoint2D distJoint = hitObj.AddComponent<DistanceJoint2D>();
            distJoint.autoConfigureConnectedAnchor = false;
            distJoint.autoConfigureDistance = false;
            distJoint.distance = 5;
            distJoint.connectedAnchor = ropeHit.point;
            distJoint.connectedBody = rigid;
            //DrawRope();
        }
    }
    private void DrawRope()
    {

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
