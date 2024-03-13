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
    private StateMachine<PlayerBase> fsm; // Player finite state machine
    public StateMachine<PlayerBase> FSM { get { return fsm; } }

    #region check extra State
    [SerializeField]
    private bool isGround;
    public bool IsGround { get { return isGround; } set { isGround = value; } }

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
    public bool IsCeilingStick { 
        get { return isCeilingStick; }
        set 
        {
            isCeilingStick = value;
            CeilingChecker.SetActive(value);
            if (isCeilingStick)
                rigid.gravityScale = 0f;
            else
                rigid.gravityScale = 1f;
        } 
    }

    #endregion

    [Space(3)]
    [Header("Player Action Events")]
    [Space(2)]
    public UnityEvent OnRun;            // invoke by state
    public UnityEvent OnJump;           // invoke by PlayerMover (Jump())
    public UnityEvent OnWallJump;       // invoke by PlayerMover (WallJump())
    public UnityEvent OnHitJump;        // invoke by PlayerMover (HitJump())
    public UnityEvent OnDash;           // invoke by state
    public UnityEvent OnRopeForceStart; // invoke by PlayerSkill (RopeForce())
    public UnityEvent OnRopeForceEnd;   // invoke by PlayerHooker (OnHookDestroyed())
    public UnityEvent OnGrabEnd;        // invoke by state
    public UnityEvent OnClimb;          // invoke by state
    public UnityEvent OnWallSliding;    // invoke by state
    public UnityEvent OnHookShoot;      // invoke by PlayerHooker (HookShoot)
    public UnityEvent OnTakeDamage;     // invoke by state
    public UnityEvent OnLanding;        // invoke by PlayerMover (TriggerEnter2D)

    private Coroutine takeDamageRoutine;
    public Coroutine TakeDamageCoroutine { get { return takeDamageRoutine; } }
    protected override void Awake()
    {
        base.Awake();

        fsm = new StateMachine<PlayerBase>(this);
        fsm.AddState("Idle", new PlayerIdle(this));
        fsm.AddState("Damaged", new PlayerDamaged(this));
        fsm.AddState("Run", new PlayerRun(this));
        fsm.AddState("RunStop", new PlayerRunStop(this));
        fsm.AddState("HookShoot", new PlayerHookShoot(this));
        fsm.AddState("Fall", new PlayerFall(this));
        fsm.AddState("Jump", new PlayerJump(this));
        fsm.AddState("HookingJump", new PlayerHookingJump(this));
        fsm.AddState("Roping", new PlayerRoping(this));
        fsm.AddState("Dash", new PlayerDash(this));
        fsm.AddState("Grab", new PlayerGrab(this));
        fsm.AddState("WallSlide", new PlayerWallSlide(this));

        fsm.AddState("CeilingStickStart", new PlayerCeilingStickStart(this));
        fsm.AddState("CeilingStickIdle", new PlayerCeilingStickIdle(this));
        fsm.AddState("CeilingStickMove", new PlayerCeilingStickMove(this));

        fsm.AddTransition("Jump", "Fall", 0f, () =>
        {
            return rigid.velocity.y < -JumpForce_Threshold;
        });
        fsm.AddTransition("HookingJump", "Fall", 0f, () =>
        {
            return rigid.velocity.y < -JumpForce_Threshold;
        });

        fsm.AddAnyState("Fall", () =>
        {
            return  !isGround && fsm.CurState != "Roping" && fsm.CurState != "WallSlide" && fsm.CurState != "Grab"
                    && rigid.velocity.y < -JumpForce_Threshold;
        });

        fsm.AddTransition("WallSlide", "Idle", 0f, () =>
        {
            return isGround || !isInWall;
        });

        fsm.AddTransition("Fall", "Idle", 0f, () =>
        {
            return isGround && PrMover.MoveHzt == 0;
        });

        fsm.AddTransition("Fall", "Run", 0f, () =>
        {
            return isGround && PrMover.MoveHzt != 0;
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
        // 훅 샷이 가능한 상태
        // : Idle, Run, RunStop, Jump, Fall
        string myState = FSM.CurState;

        return 
            myState == "Idle" ||
            myState == "Run" ||
            myState == "RunStop" ||
            myState == "Jump" ||
            myState == "Fall";
    }

    private void TakeDamage()
    {
        DoImpulse();

        takeDamageRoutine = StartCoroutine(TakeDamageRoutine());

        // 상태전이 : ? -> Damaged
        fsm.ChangeState("Damaged");
        PrHooker.FiredHook?.DisConnecting();
    }
    
    // 히트 점프로 데미지 루틴을 빠져나간 경우
    // 글리치 이펙트 초기화, 무적상태 초기화
    public void InitDamageRoutine()
    {
        if (takeDamageRoutine != null)
            StopCoroutine(takeDamageRoutine);
        // 글리치 이펙트 비활성화
        Camera.main.GetComponent<GlitchEffect>().enabled = false;
        // 레이어 변경
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    IEnumerator TakeDamageRoutine()
    {
        // 무적상태로 변경
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        // 카메라 글리치 이펙트
        Camera.main.GetComponent<GlitchEffect>().enabled = true;
        yield return new WaitForSeconds(0.3f);
        Camera.main.GetComponent<GlitchEffect>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        fsm.ChangeState("Idle");
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private bool CheckGround(GroundType groundType)
    {
        RaycastHit2D hit;
        Vector2 rayDir = Vector2.zero;
        LayerMask layerMask = 0;
        float rayLength = 1.5f;

        switch(groundType)
        {
            case GroundType.Ground:
                layerMask = Manager.Layer.groundLM;
                rayDir = Vector2.down;
                break;
            case GroundType.HookingGround:
                layerMask = Manager.Layer.hookingGroundLM;
                rayDir = Vector2.up;
                break;
            case GroundType.Wall:
                layerMask = Manager.Layer.wallLM;
                rayDir = transform.right;
                break;
            default:
                return false;
        }

        hit = Physics2D.Raycast(transform.position, rayDir, rayLength, layerMask);
        if (hit)
            Debug.Log($"Type Change {groundType}");
        return hit;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        fsm.ChangeState(fsm.CurState);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger enter");

        // 우선순위 
        // 천장 > 바닥 > 벽

        // 천장 체크 : 천장후킹
        if (CeilingChecker.activeSelf &&
            Manager.Layer.hookingGroundLM.Contain(collision.gameObject.layer) &&
            CheckGround(GroundType.HookingGround))
        {
            PrHooker.FiredHook?.DisConnecting();
            IsCeilingStick = true;
            rigid.velocity = Vector3.zero;

            fsm.ChangeState("CeilingStickIdle");
            return;
        }

        // 벽 체크 : 벽타기 
        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer) && CheckGround(GroundType.Wall))
        {
            Debug.Log("Trigger wall");
            PrHooker.FiredHook?.DisConnecting();

            isInWall = true;
            fsm.ChangeState("WallSlide");
            return;
        }
        // 바닥 체크 
        if (Manager.Layer.groundLM.Contain(collision.gameObject.layer) && CheckGround(GroundType.Ground))
        {
            Debug.Log("Check Ground");
            // 마찰력을 줄이기 위한 소프트 랜딩
            rigid.velocity = new Vector2(rigid.velocity.x, -0.01f);
            // ground check
            OnLanding?.Invoke();

            isGround = true;
            return;
        }


        if (Manager.Layer.damageGroundLM.Contain(collision.gameObject.layer))
        {
            // 모든 상태 탈출
            isInWall = false;
            IsCeilingStick = false;

            rigid.velocity = Vector3.zero;
            rigid.AddForce(collision.gameObject.transform.right * 12f + rigid.transform.right * -5f, ForceMode2D.Impulse);
            TakeDamage();
            return;
        }

        if (Manager.Layer.enemyBulletLM.Contain(collision.gameObject.layer) ||
            (Manager.Layer.bossAttackLM.Contain(collision.gameObject.layer)))
        {
            TakeDamage();
            return;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trigger exit");

        if (Manager.Layer.groundLM.Contain(collision.gameObject.layer))
        {
             isGround = false;
        }

        if (CeilingChecker.activeSelf &&
            Manager.Layer.hookingGroundLM.Contain(collision.gameObject.layer))
        {
            IsCeilingStick = false;
            fsm.ChangeState("Fall");
            return;
        }

        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer))
        {
            isInWall = false;
            //PrHooker.FiredHook?.DisConnecting();

            // 벽을 오르는 상태에서 탈출했을 경우
            if (PrMover.MoveVtc > 0 && fsm.CurState == "WallSlide")
            {
                fsm.ChangeState("Jump");
                // Wall Exit to Up Position
                OnWallJump?.Invoke();
                rigid.AddForce(transform.up * 10f, ForceMode2D.Impulse);
            }
        }
    }
}
