using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;

public enum GroundType
{
    Ground,
    Wall,
    HookingGround
}

public class PlayerFSM : PlayerBase
{
    [SerializeField]
    protected GameObject ceilingChecker;
    public GameObject CeilingChecker { get { return ceilingChecker; } }

    [Space(3)]
    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    private StateMachine<Player> fsm; // Player finite state machine
    public StateMachine<Player> FSM { get { return fsm; } }

    #region check extra State
    [SerializeField]
    private bool isGround;
    public bool IsGround { get { return isGround; } set { isGround = value; } }

    [SerializeField]
    private bool isDamageable = true;
    public bool IsDamageable { get { return isDamageable; } set { isDamageable = value; } }

    [SerializeField]
    private int groundCount;

    [SerializeField]
    private bool isInWall;
    public bool IsInWall { get { return isInWall; } set { isInWall = value; } }

    [SerializeField]
    private bool isJointed = false;
    public bool IsJointed { get { return isJointed; } set { isJointed = value; } }

    [SerializeField]
    private bool isEnableGrabMove = false;
    public bool IsEnableGrabMove { get { return isEnableGrabMove; } set { isEnableGrabMove = value; } }

    [SerializeField]
    private bool isCeilingStick = false;
    public bool IsCeilingStick
    {
        get { return isCeilingStick; }
        set
        {
            isCeilingStick = value;
            CeilingChecker.SetActive(value);
            if (isCeilingStick)
                Player.Rigid.gravityScale = 0f;
            else
                Player.Rigid.gravityScale = 1f;
        }
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();
        FSMInitialSetting();
    }

    private void FSMInitialSetting()
    {
        fsm = new StateMachine<Player>(Player);
        fsm.AddState("Idle", new PlayerIdle(Player));
        fsm.AddState("Damaged", new PlayerDamaged(Player));
        fsm.AddState("Run", new PlayerRun(Player));
        fsm.AddState("RunStop", new PlayerRunStop(Player));
        fsm.AddState("HookShoot", new PlayerHookShoot(Player));
        fsm.AddState("Fall", new PlayerFall(Player));
        fsm.AddState("Jump", new PlayerJump(Player));
        fsm.AddState("HookingJump", new PlayerHookingJump(Player));
        fsm.AddState("Roping", new PlayerRoping(Player));
        fsm.AddState("Dash", new PlayerDash(Player));
        fsm.AddState("Grab", new PlayerGrab(Player));
        fsm.AddState("WallSlide", new PlayerWallSlide(Player));

        fsm.AddState("CeilingStickStart", new PlayerCeilingStickStart(Player));
        fsm.AddState("CeilingStickIdle", new PlayerCeilingStickIdle(Player));
        fsm.AddState("CeilingStickMove", new PlayerCeilingStickMove(Player));

        fsm.AddState("Respawn", new PlayerRespawn(Player));
        fsm.AddState("DeadZoneDie", new PlayerDeadZoneDie(Player));
        fsm.AddState("DamagedDie", new PlayerDamagedDie(Player));
        fsm.AddState("CutSceneMode", new PlayerCutSceneMode(Player));

        fsm.AddTransition("Roping", "Idle", 0f, () =>
        {
            return IsGround && !isJointed;
        });
        fsm.AddTransition("HookingJump", "Idle", 0f, () =>
        {
            return IsGround;
        });

        #region fall transition
        fsm.AddTransition("Jump", "Fall", 0f, () =>
        {
            return Player.Rigid.velocity.y < -Player.JumpForce_Threshold;
        });

        fsm.AddAnyState("Fall", () =>
        {
            return !isGround &&
            fsm.CurState != "CeilingStickStart" &&
            fsm.CurState != "CeilingStickStart" &&
            fsm.CurState != "HookShoot" &&
            fsm.CurState != "Roping" &&
            fsm.CurState != "WallSlide" &&
            fsm.CurState != "Grab" &&
            fsm.CurState != "CutSceneMode" &&
            fsm.CurState != "Respawn" && 
            fsm.CurState != "DamagedDie" &&
            fsm.CurState != "DeadZoneDie" &&
            Player.Rigid.velocity.y < -Player.JumpForce_Threshold;
        });
        fsm.AddTransition("Fall", "Idle", 0f, () =>
        {
            return isGround && Player.PrMover.MoveHzt == 0;
        });

        fsm.AddTransition("Fall", "Run", 0f, () =>
        {
            return isGround && Player.PrMover.MoveHzt != 0;
        });
        #endregion

        fsm.AddTransition("WallSlide", "Idle", 0f, () =>
        {
            return isGround || !isInWall;
        });

        fsm.AddTransition("Idle", "Run", 0f, () =>
        {
            // is input Keyboard "A"key or "D" key
            return Mathf.Abs(Player.PrMover.MoveHzt) > Player.MoveForce_Threshold;
        });
        fsm.AddTransition("Run", "RunStop", 0f, () =>
        {
            // isn't input Keyboard "A"key or "D" key
            return Mathf.Abs(Player.PrMover.MoveHzt) == 0;
        });
        fsm.AddTransition("RunStop", "Run", 0f, () =>
        {
            // input swap "A"key -> "D"key or "D"key -> "A"key
            return Mathf.Abs(Player.PrMover.MoveHzt) > Player.MoveForce_Threshold;
        });
        fsm.AddTransition("RunStop", "Idle", 0.2f, () =>
        {
            return Mathf.Abs(Player.PrMover.MoveHzt) <= Player.MoveForce_Threshold;
        });

        fsm.Init("Idle");
    }

    public void ChangeState(string name)
    {
        fsm.ChangeState(name);
    }

    [SerializeField]
    private string currentState;

    private void Update()
    {
        currentState = fsm.CurState;
        fsm.Update();

        Debug.DrawLine(transform.position, transform.position + Vector3.up * 1.5f, Color.red);
        Debug.DrawLine(transform.position, transform.position + Vector3.down * 1.5f, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.right * 1.5f, Color.red);
    }
    private void FixedUpdate()
    {
        fsm.FixedUpdate();
    }
    private void LateUpdate()
    {
        fsm.LateUpdate();
    }

    public bool IsHookable()
    {
        // ÈÅ ¼¦ÀÌ °¡´ÉÇÑ »óÅÂ
        // : Idle, Run, RunStop, Jump, Fall
        string myState = FSM.CurState;

        return
            myState == "Idle" ||
            myState == "Run" ||
            myState == "RunStop" ||
            myState == "Jump" ||
            myState == "Fall";
    }
}
