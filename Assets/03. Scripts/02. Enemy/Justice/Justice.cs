using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JusticeAttackType
{ 
    Slash,
    DashSlash,
    CircleSlash,
    CloakingSlash
}

public class Justice : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }

    [SerializeField]
    private Animator anim;
    public Animator Anim { get { return anim; } }

    [SerializeField]
    private Animator embientAnim;
    public Animator EmbientAnim { get { return embientAnim; } }

    [SerializeField]
    private CircleCollider2D circleCol;
    public CircleCollider2D CircleCol { get { return circleCol; } }

    [SerializeField]
    private WeaknessController weaknessController;
    public WeaknessController WeaknessController { get { return weaknessController; } }

    [SerializeField]
    private JusticeVFXPooler agentVFXPool;
    public JusticeVFXPooler AgentVFXPool { get { return agentVFXPool; } }
    [SerializeField]
    private JusticeVFXPooler chargeVFXPool;
    public JusticeVFXPooler ChargeVFXPool { get { return chargeVFXPool; } }

    //[SerializeField]
    //private Dictionary<string, JusticeVFX>;

    [Header("Specs")]
    // 기본 공격 범위
    [SerializeField]
    private float slashAttackRange;
    public float SlashAttackRange { get { return slashAttackRange; } }
    // 대쉬 공격 범위
    [SerializeField]
    private float dashSlashAttackRange;
    public float DashSlashAttackRange { get { return dashSlashAttackRange; } }

    [SerializeField]
    private Vector3 beforeBattleModePos;
    [SerializeField]
    private Vector3 battleModePos;


    // 최소 공격 범위
    // (현재 공격 타입의 공격 범위 - 최소 공격 범위)에 플레이어가 존재할 경우 차지 상태로 전환
    [SerializeField]
    public const float Attack_Threshold = 3f;


    [Header("Phase Data")]
    [Space(2)]
    [SerializeField]
    private Vector3 phase1Pos;
    
    private List<JusticePhaseData> phaseDatas;
    public List<JusticePhaseData> PhaseDatas { get { return phaseDatas; } }

    [Space(3)]
    [Header("Load Data")]
    [Space(2)]
    [SerializeField]
    private int maxHp;
    public int MaxHp { get { return maxHp; } }

    [SerializeField]
    private float moveSpeed;
    public float MoveSpeed { get { return moveSpeed; } }

    // 최소 이동 시간
    [SerializeField]
    private float trackingTime;
    public float TrackingTime { get { return trackingTime; } }

    // 텔레포트 딜레이시간
    [SerializeField]
    private float teleportTime;
    public float TeleportTime { get { return teleportTime; } }

    // 공격 이전 딜레이시간 == 차지 범위 표시시간
    [SerializeField]
    private float slashAttackChargeTime;
    [SerializeField]
    private float circleAttackChargeTime;
    [SerializeField]
    private float dashSlashAttackChargeTime;

    [SerializeField]
    private float attackDelayTime;
    public float AttackDelayTime { get { return attackDelayTime; } }

    [Space(3)]
    [Header("Balancing")]
    [Space(2)]
    [SerializeField]
    private Vector3 attackDir;
    public Vector3 AttackDir { get { return attackDir; } set { attackDir = value; } }

    [SerializeField]
    private Transform playerTr;
    public Transform PlayerTr { get { return playerTr; } }

    [SerializeField]
    private int curHp;
    public int CurHp { get { return curHp; } }

    [SerializeField]
    private float curAttackChargeTime;
    public float CurAttackChargeTime { get { return curAttackChargeTime; } }

    // 일반(슬래시)어택 카운트 
    // max count만큼의 공격을 실행하면 공격 모드를 대쉬 공격 모드로 변경
    [SerializeField]
    private int maxSlashCount;
    public int MaxSlashCount { get { return maxSlashCount; } }

    [SerializeField]
    private int curSlashCount;
    public int CurSlashCount 
    { 
        get 
        { 
            return curSlashCount; 
        } 
        set 
        {
            curSlashCount = value;

            // 일반 공격을 실행한 횟수에 따라 공격타입 변경
            if (curSlashCount >= maxSlashCount)
                currentAttackType = JusticeAttackType.DashSlash;
            else
                currentAttackType = JusticeAttackType.Slash;
        } 
    }

    // 현재 공격 타입
    private JusticeAttackType currentAttackType;
    public JusticeAttackType CurrentAttackType 
    { 
        get 
        {
            return currentAttackType; 
        } 
        set
        {
            currentAttackType = value;
            switch(currentAttackType)
            {
                case JusticeAttackType.Slash:
                    curAttackChargeTime = slashAttackChargeTime;
                    break;
                case JusticeAttackType.DashSlash:
                    curAttackChargeTime = dashSlashAttackChargeTime;
                    break;
                case JusticeAttackType.CircleSlash:
                    curAttackChargeTime = circleAttackChargeTime;
                    break;
                case JusticeAttackType.CloakingSlash:
                    curAttackChargeTime = circleAttackChargeTime;
                    break;
                default:
                    break;
            }
        }
    }

    // 유한상태머신
    private StateMachine<Justice> fsm;
    public StateMachine<Justice> FSM { get { return fsm; } }

    private void Awake()
    {
        playerTr = GameObject.FindWithTag("Player").transform;
        CurrentAttackType = JusticeAttackType.Slash;

        fsm = new StateMachine<Justice>(this);
        fsm.AddState("PowerOff", new PowerOff());
        fsm.AddState("PowerOn", new PowerOn(this));
        fsm.AddState("BeforeBattleMode", new BeforeBattleMode(this));
        fsm.AddState("BattleMode", new BattleMode(this));
        fsm.AddState("Track", new Track(this));
        fsm.AddState("Teleport", new Teleport(this));
        fsm.AddState("Charge", new Charge(this));
        fsm.AddState("Attack", new Attack(this));
        fsm.AddState("Groggy", new Groggy(this));

        // 추후
        fsm.AddState("Counter", new Counter(this));

        fsm.Init("PowerOff");
    }
    private void Update()
    {
        fsm.Update();
    }
    private void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    #region Cinematic
    // 이벤트 트리거
    public void EnterBeforeBattleMode()
    {
        fsm.ChangeState("PowerOn");

        StartCoroutine(Extension.DelayRoutine(5f, () => fsm.ChangeState("BeforeBattleMode")));
    }

    // 시네마틱 용 애니메이션
    public void SetBeforeBattlePos()
    {
        transform.position = beforeBattleModePos;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    public void SetBattlePos()
    {
        transform.position = battleModePos;
    }
    public void EnableObjects()
    {
        embientAnim.Play("EnterBattleMode");
        weaknessController.IsSpawnIdle = true;
    }
    #endregion

    public void ChangeAttackType(JusticeAttackType type)
    {
        currentAttackType = type;
    }
    public void ChangeAttackType()
    {
        currentAttackType = JusticeAttackType.Slash;
    }

    // 공격 반사
    private void Parrying(Collider2D collision)
    {
        collision.GetComponent<Hook>().DisConnecting();

        Vector3 dir = (collision.transform.position - transform.position).normalized;

        GameObject parryingOb = null;
        GameObject sparkOb = null;

        // vfx : 반사 잔상
        // 상단 방어
        if (dir.y >= 0)
            parryingOb = agentVFXPool.ActiveVFX("ParryingA");
        // 하단 방어
        else
            parryingOb = agentVFXPool.ActiveVFX("ParryingB");

        parryingOb.transform.right = -dir;
        parryingOb.transform.position += dir * 1.5f;
        
        // vfx : 반사 스파크 
        sparkOb = agentVFXPool.ActiveVFX("ParryingSpark");
        sparkOb.transform.up = dir;
        sparkOb.transform.position = collision.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 훅과 닿은 경우
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
        {
            // 그랩
            if (fsm.CurState == "Groggy")
            {

            }
            // 반격
            else
            {
                Parrying(collision);

                if (fsm.CurState == "BeforeBattleMode")
                    fsm.ChangeState("BattleMode");
            }
        }
    }
}
