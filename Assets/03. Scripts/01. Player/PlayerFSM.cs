using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    protected bool isGround;
    public bool IsGround { get { return isGround; } set { isGround = value; } }

    [SerializeField]
    protected bool isJointed = false;
    public bool IsJointed { get { return isJointed; } set { isJointed = value; } }

    [SerializeField]
    protected bool isDash = false;
    public bool IsDash { get { return isDash; } set { isDash = value; } }

    [SerializeField]
    protected bool isGrab = false;
    public bool IsGrab { get { return isGrab; } set { isGrab = value; } }

    [SerializeField]
    protected bool isHookShoot = false;
    public bool IsHookShoot { get { return isHookShoot; } set { isHookShoot = value; } }

    [SerializeField]
    protected bool isRaycastHit = false;
    public bool IsRaycastHit { get { return isRaycastHit; }set { isRaycastHit = value; } }
    #endregion


    protected override void Awake()
    {
        base.Awake();

        fsm = new StateMachine<PlayerBase>(this);
        fsm.AddState("Idle", new PlayerIdle(this));
        fsm.AddState("Run", new PlayerRun(this));
        fsm.AddState("RunStop", new PlayerRunStop(this));
        fsm.AddState("Fall", new PlayerFall(this));
        fsm.AddState("Jump", new PlayerJump(this));
        fsm.AddState("Roping", new PlayerRoping(this));
        fsm.AddState("Dash", new PlayerDash(this));
        fsm.AddState("Grab", new PlayerGrab(this));
        fsm.AddState("WallSlide", new PlayerWallSlide(this));

        fsm.AddAnyState("Jump", () =>
        {
            return !isGround && !isJointed && rigid.velocity.y > JumpForce_Threshold;
        });
        fsm.AddTransition("Jump", "Fall", 0f, () =>
        {
            return rigid.velocity.y < -JumpForce_Threshold;
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
}
