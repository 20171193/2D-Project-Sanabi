using Cinemachine;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public const float MoveForce_Threshold = 0.1f;
    [HideInInspector]
    public const float JumpForce_Threshold = 0.05f;

    // Linked Class
    [SerializeField]
    private PlayerFSM playerFSM;
    public PlayerFSM PrFSM { get { return playerFSM; } }

    [SerializeField]
    private PlayerMover playerMover;
    public PlayerMover PrMover { get { return playerMover; } }

    [SerializeField]
    private PlayerHooker playerHooker;
    public PlayerHooker PrHooker { get { return playerHooker; } }

    [SerializeField]
    private PlayerSkill playerSkill;
    public PlayerSkill PrSkill { get { return playerSkill; } }

    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }

    [SerializeField]
    private Animator anim;
    public Animator Anim { get { return anim; } }

    [SerializeField]
    private Camera cam;
    public Camera Cam { get { return cam; } }

    [SerializeField]
    private PlayerInput prInput;
    public PlayerInput PrInput { get { return prInput; } }

    [SerializeField]
    private CinemachineImpulseSource impulseSource;
    public CinemachineImpulseSource ImpulseSource { get { return impulseSource; } }

    [SerializeField]
    private PlayerVFXPooler playerVFXPooler;
    public PlayerVFXPooler PrVFX { get { return playerVFXPooler; } }

    [SerializeField]
    private HUD_HP hp_HUD;
    public HUD_HP HP_HUD { get { return hp_HUD; } }

    [SerializeField]
    private EventController eventController;
    public EventController EventController { get { return eventController; } }

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private int maxHP;
    public int MaxHP { get { return maxHP; }}

    [Space(3)]
    [Header("Balancing")]
    [Space(2)]
    [SerializeField]
    private int currentHp;
    public int CurrentHP { get { return currentHp; } set { currentHp = value; } }


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

    private string cutSceneAnim;
    public string CutSceneAnim { get { return cutSceneAnim; } set { cutSceneAnim = value; } }

    private void Awake()
    { 
        impulseSource = GetComponent<CinemachineImpulseSource>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        prInput = GetComponent<PlayerInput>();
        cam = Camera.main;

        // Assign linked class
        playerMover = GetComponent<PlayerMover>();
        playerFSM = GetComponent<PlayerFSM>();
        playerHooker = GetComponent<PlayerHooker>();
        playerSkill = GetComponent<PlayerSkill>();

        Respawn();
    }

    public void DoImpulse()
    {
        impulseSource.GenerateImpulse();
    }

    public void Respawn()
    {
        eventController.DisableDeathEvent(DeathType.DeadZone);
        eventController.DisableDeathEvent(DeathType.Damaged);

        GameData loadedData = Manager.Data.LoadCurrentData();

        // 로드한 데이터가 있을 경우
        if (loadedData != null) 
        {
            Manager.Camera.SetConfiner(loadedData.confiner);
            transform.position = loadedData.startPos;
        }

        currentHp = maxHP;
        hp_HUD.OnRespawn();
        eventController.FadeOut();
        StartCoroutine(Extension.DelayRoutine(0.3f, () => PrFSM.ChangeState("Respawn")));
    }

    #region CutSceneSetting
    public void OnEnterCutSceneMode(string anim = null)
    {
        cutSceneAnim = anim;
        PrFSM.FSM.ChangeState("CutSceneMode");
    }
    public void OnExitCutSceneMode()
    {
        cutSceneAnim = null;
        PrFSM.FSM.ChangeState("Idle");
    }
    #endregion

    #region TakeDamage / Restore
    private void TakeDamage()
    {
        DoImpulse();

        if (currentHp <= 1)
        {
            PrFSM.ChangeState("DamagedDie");
            return;
        }

        hp_HUD.OnDamaged(--currentHp);
        takeDamageRoutine = StartCoroutine(TakeDamageRoutine());
        // 상태전이 : ? -> Damaged
        PrFSM.ChangeState("Damaged");
        PrHooker.FiredHook?.DisConnecting();
    }
    private void RestoreHP()
    {
        if (currentHp >= maxHP) return;

        hp_HUD.OnRestore(++currentHp);
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
        PrFSM.ChangeState("Idle");
        gameObject.layer = LayerMask.NameToLayer("Player");
    }
    #endregion

    // Ground Check Raycaster
    private bool CheckGround(GroundType groundType)
    {
        RaycastHit2D hit;
        Vector2 rayDir = Vector2.zero;
        LayerMask layerMask = 0;
        float rayLength = 2f;

        switch (groundType)
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

    #region Collision / Trigger

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //fsm.ChangeState(fsm.CurState);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger enter");

        if (collision.gameObject.layer == LayerMask.NameToLayer("DeadZone"))
            PrFSM.ChangeState("DeadZoneDie");


        // 우선순위 
        // 천장 > 바닥 > 벽

        // 천장 체크 : 천장후킹
        if (PrFSM.CeilingChecker.activeSelf &&
            Manager.Layer.hookingGroundLM.Contain(collision.gameObject.layer) &&
            CheckGround(GroundType.HookingGround))
        {
            PrHooker.FiredHook?.DisConnecting();
            PrFSM.IsCeilingStick = true;
            Rigid.velocity = Vector3.zero;

            PrFSM.ChangeState("CeilingStickIdle");
            return;
        }

        // 벽 체크 : 벽타기 
        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer) && CheckGround(GroundType.Wall))
        {
            Debug.Log("Trigger wall");
            PrFSM.IsCeilingStick = false;
            PrHooker.FiredHook?.DisConnecting();

            PrFSM.IsInWall = true;
            PrFSM.ChangeState("WallSlide");
            return;
        }
        // 바닥 체크 
        if (Manager.Layer.groundLM.Contain(collision.gameObject.layer) && CheckGround(GroundType.Ground))
        {
            Debug.Log("Check Ground");
            // 마찰력을 줄이기 위한 소프트 랜딩
            Rigid.velocity = new Vector2(Rigid.velocity.x, -0.01f);
            // ground check
            OnLanding?.Invoke();

            if (PrFSM.IsJointed)
                PrHooker.FiredHook?.DisConnecting();
            if (PrFSM.IsCeilingStick)
                PrFSM.IsCeilingStick = false;

            PrFSM.IsGround = true;
            return;
        }


        if (PrFSM.IsDamageable &&
            gameObject.layer != LayerMask.NameToLayer("PlayerInvincible") &&
            Manager.Layer.damageGroundLM.Contain(collision.gameObject.layer))
        {
            // 모든 상태 탈출
            PrFSM.IsInWall = false;
            PrFSM.IsCeilingStick = false;

            Rigid.velocity = Vector3.zero;
            Rigid.AddForce(collision.gameObject.transform.right * 12f + Rigid.transform.right * -5f, ForceMode2D.Impulse);
            TakeDamage();
            return;
        }

        if (PrFSM.IsDamageable &&
            gameObject.layer != LayerMask.NameToLayer("PlayerInvincible") &&
            Manager.Layer.enemyBulletLM.Contain(collision.gameObject.layer) ||
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
            PrFSM.IsGround = false;
        }

        if (PrFSM.CeilingChecker.activeSelf &&
            Manager.Layer.hookingGroundLM.Contain(collision.gameObject.layer))
        {
            Debug.Log("Change fall");
            PrFSM.IsCeilingStick = false;
            PrFSM.ChangeState("Fall");
            return;
        }

        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer))
        {
            PrFSM.IsInWall = false;
            //PrHooker.FiredHook?.DisConnecting();

            // 벽을 오르는 상태에서 탈출했을 경우
            if (PrMover.MoveVtc > 0 && PrFSM.FSM.CurState == "WallSlide")
            {
                PrFSM.ChangeState("Jump");
                // Wall Exit to Up Position
                OnWallJump?.Invoke();
                Rigid.AddForce(transform.up * 10f, ForceMode2D.Impulse);
            }
        }
    }
    #endregion
}
