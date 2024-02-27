using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    [HideInInspector]
    public const float MoveForce_Threshold = 0.1f;
    [HideInInspector]
    public const float JumpForce_Threshold = 0.05f;

    [Header("Components")]
    [Space(2)]
    protected Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }

    protected Animator anim;
    public Animator Anim { get { return anim; } }

    protected Camera cam;
    protected PlayerMover mover;
    protected PlayerHooker hooker;
    protected PlayerSkill skill;

    [Space(3)]
    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    protected StateMachine<PlayerBase> fsm; // Player finite state machine
    public StateMachine<PlayerBase> FSM { get { return fsm; } }


    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    protected Enemy grabEnemy;
    public Enemy GrabEnemy { get { return grabEnemy; } }

    [SerializeField]
    protected Hook firedHook;
    public Hook FiredHook { get { return firedHook; } }


    [SerializeField]
    protected bool isGround;
    public bool IsGround { get { return isGround; } }

    [SerializeField]
    protected bool isJointed = false;
    public bool IsJointed { get { return isJointed; } set { isJointed = value; } }

    [SerializeField]
    protected bool isDash = false;
    public bool IsDash { get { return isDash; } }

    [SerializeField]
    protected bool isGrab = false;
    public bool IsGrab { get { return isGrab; } }

    [SerializeField]
    protected bool isHookShoot = false;
    public bool IsHookShoot { get { return isHookShoot; } }

    [SerializeField]
    protected bool isRaycastHit = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();    
        anim = GetComponent<Animator>();

        // childeren
        mover = GetComponent<PlayerMover>();
        hooker = GetComponent<PlayerHooker>();
        skill = GetComponent<PlayerSkill>();

        cam = Camera.main;

        fsm = new StateMachine<PlayerBase>(this);
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
}
