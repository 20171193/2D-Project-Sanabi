using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public enum JusticeAttackType
{
    Slash,
    DashSlash,
    CircleSlash,
    CloakingSlash
}

public class Justice : MonoBehaviour, IGrabable
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

    [SerializeField]
    private CinemachineVirtualCamera justiceCamera;
    public CinemachineVirtualCamera JusticeCamera { get { return justiceCamera; } }

    [SerializeField]
    private SpriteRenderer bodyRenderer;
    public SpriteRenderer BodyRenderer { get { return bodyRenderer; } }

    [SerializeField]
    private BossRoomController bossRoomController;
    public BossRoomController BossRoomController { get { return bossRoomController; } }

    [SerializeField]
    private AudioSource parryingSource;

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
    private float groggyTime;
    public float GroggyTime { get { return groggyTime; } }

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
    private Vector3 phasePos;

    [SerializeField]
    public int nextPhaseIndex = 0;
    public int NextPhaseIndex { get { return nextPhaseIndex; } set { nextPhaseIndex = value; } }

    [SerializeField]
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

    [SerializeField]
    private int maxSlashCount;
    public int MaxSlashCount { get { return maxSlashCount; } }

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
    public int CurHp { get { return curHp; } set { curHp = value; } }

    [SerializeField]
    private float curAttackChargeTime;
    public float CurAttackChargeTime { get { return curAttackChargeTime; } }

    // 일반(슬래시)어택 카운트 
    // max count만큼의 공격을 실행하면 공격 모드를 대쉬 공격 모드로 변경

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
            switch (currentAttackType)
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
    /*******************************
     * 디버그용
     ******************************/
    [Header("Debug")]
    [SerializeField]
    private string initState;
    /******************************/
    private void Awake()
    {
        playerTr = GameObject.FindWithTag("Player").transform;
        CurrentAttackType = JusticeAttackType.Slash;

        // 데이터 로드
        if (!LoadPhaseData()) 
            Debug.Log("저스티스 페이즈 데이터 로드 실패");


        fsm = new StateMachine<Justice>(this);
        fsm.AddState("PowerOff", new PowerOff(this));
        fsm.AddState("PowerOn", new PowerOn(this));
        fsm.AddState("BeforeBattleMode", new BeforeBattleMode(this));
        fsm.AddState("BattleMode", new BattleMode(this));
        fsm.AddState("Track", new Track(this));
        fsm.AddState("Teleport", new Teleport(this));
        fsm.AddState("Charge", new Charge(this));
        fsm.AddState("Attack", new Attack(this));
        fsm.AddState("Groggy", new Groggy(this));
        fsm.AddState("Grabbed", new Grabbed(this));
        fsm.AddState("GrabbedEnd", new GrabbedEnd(this));
        fsm.AddState("PhaseChange", new PhaseChange(this));
        fsm.AddState("LastStanding", new LastStanding(this));
        // 추후
        fsm.AddState("Counter", new Counter(this));

        fsm.Init(initState);

        weaknessController.OnAllWeaknessDestroyed += OnAllWeaknessDestroyed;
    }
    private void Update()
    {
        fsm.Update();
    }
    private void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    public bool LoadPhaseData()
    {
        if (nextPhaseIndex >= phaseDatas.Count) return false;
        
        ApplyData(phaseDatas[nextPhaseIndex++]);
        return true;
    }
    private void ApplyData(JusticePhaseData data)
    {
        maxHp = data.MaxHp;
        CurHp = maxHp;
        moveSpeed = data.MoveSpeed;
        trackingTime = data.TrackingTime;
        maxSlashCount = data.SlashCount;

        slashAttackChargeTime = data.Delay.SlashAttackChargeTime;
        circleAttackChargeTime = data.Delay.CircleAttackChargeTime;
        dashSlashAttackChargeTime = data.Delay.DashSlashAttackChargeTime;
        teleportTime = data.Delay.TeleportDelayTime;
        attackDelayTime = data.Delay.AttackDelayTime;
    }

    #region Cinematic
    // 이벤트 트리거

    // 배틀모드 이전 트리거 진입 이벤트
    public void EnterPowerOn()
    {
        fsm.ChangeState("PowerOn");
    }
    public void EnterBeforeBattleMode()
    {
        transform.position = beforeBattleModePos;
        transform.rotation = Quaternion.Euler(0, 180, 0);
        fsm.ChangeState("BeforeBattleMode");
    }

    // 애니메이션 콜백
    // 배틀모드 이전 애니메이션 콜백함수
    private Coroutine cinematicRoutine;
    public void SetBattlePos()
    {
        cinematicRoutine = StartCoroutine(TranslateRoutine());
    }
    IEnumerator TranslateRoutine()
    {
        while (transform.position.y <= battleModePos.y)
        {
            transform.Translate(Vector3.up * 1.5f * Time.deltaTime);
            yield return null;
        }
        yield return null;
    }
    public void EnableObjects()
    {
        embientAnim.Play("EnterBattleMode");
        weaknessController.IsSpawnIdle = true;
    }
    public void BattleModeEnd()
    {
        if (cinematicRoutine != null)
            StopCoroutine(cinematicRoutine);

        fsm.ChangeState("Track");
    }

    // 페이즈 체인지 애니메이션 콜백
    public void OnPhaseChangeMove()
    {
        transform.position = phasePos;
    }
    public void OnPhaseChangeEnd()
    {
        fsm.ChangeState("Track");
    }

    // 보스엔딩 애니메이션 콜백

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
    private void Parrying(Vector3 hitPos)
    {
        Vector3 dir = (hitPos - transform.position).normalized;

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
        parryingOb.transform.position = transform.position + dir * 1.5f;

        parryingSource.Play();
        // vfx : 반사 스파크 
        sparkOb = agentVFXPool.ActiveVFX("ParryingSpark");
        sparkOb.transform.up = dir;
        sparkOb.transform.position = hitPos + dir * 2f;
    }

    // 일정확률 카운터
    public void Counter()
    {
        // 바로 카운터 상태로 돌입.
        // 
    }

    public void OnAllWeaknessDestroyed()
    {
        fsm.ChangeState("Groggy");
    }

    #region IGrabable override 
    public void Grabbed(Rigidbody2D ownerRigid)
    {
        fsm.ChangeState("Grabbed");
    }

    public void GrabEnd()
    {
        fsm.ChangeState("GrabbedEnd");
    }

    public bool IsMoveable()
    {
        return false;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public Vector3 GetGrabPosition()
    {
        Vector3 ret = transform.position;
        ret.y += 2.5f;
        return ret;
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 훅과 닿은 경우
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
        {
            Debug.Log("Hitted Hook");

            // 반사
            if (fsm.CurState != "Groggy")
            {
                Debug.Log("Parrying!");
                Parrying(collision.transform.position);
                collision.GetComponent<Hook>().OnHookAttackFailed?.Invoke();

                if (fsm.CurState == "BeforeBattleMode")
                    fsm.ChangeState("BattleMode");
            }
        }
    }
}
