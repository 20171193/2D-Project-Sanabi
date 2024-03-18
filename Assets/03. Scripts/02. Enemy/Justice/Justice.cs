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
    // �⺻ ���� ����
    [SerializeField]
    private float slashAttackRange;
    public float SlashAttackRange { get { return slashAttackRange; } }
    // �뽬 ���� ����
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


    // �ּ� ���� ����
    // (���� ���� Ÿ���� ���� ���� - �ּ� ���� ����)�� �÷��̾ ������ ��� ���� ���·� ��ȯ
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

    // �ּ� �̵� �ð�
    [SerializeField]
    private float trackingTime;
    public float TrackingTime { get { return trackingTime; } }

    // �ڷ���Ʈ �����̽ð�
    [SerializeField]
    private float teleportTime;
    public float TeleportTime { get { return teleportTime; } }

    [SerializeField]
    private int maxSlashCount;
    public int MaxSlashCount { get { return maxSlashCount; } }

    // ���� ���� �����̽ð� == ���� ���� ǥ�ýð�
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

    // �Ϲ�(������)���� ī��Ʈ 
    // max count��ŭ�� ������ �����ϸ� ���� ��带 �뽬 ���� ���� ����

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

            // �Ϲ� ������ ������ Ƚ���� ���� ����Ÿ�� ����
            if (curSlashCount >= maxSlashCount)
                currentAttackType = JusticeAttackType.DashSlash;
            else
                currentAttackType = JusticeAttackType.Slash;
        }
    }

    // ���� ���� Ÿ��
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

    // ���ѻ��¸ӽ�
    private StateMachine<Justice> fsm;
    public StateMachine<Justice> FSM { get { return fsm; } }
    /*******************************
     * ����׿�
     ******************************/
    [Header("Debug")]
    [SerializeField]
    private string initState;
    /******************************/
    private void Awake()
    {
        playerTr = GameObject.FindWithTag("Player").transform;
        CurrentAttackType = JusticeAttackType.Slash;

        // ������ �ε�
        if (!LoadPhaseData()) 
            Debug.Log("����Ƽ�� ������ ������ �ε� ����");


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
        // ����
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
    // �̺�Ʈ Ʈ����

    // ��Ʋ��� ���� Ʈ���� ���� �̺�Ʈ
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

    // �ִϸ��̼� �ݹ�
    // ��Ʋ��� ���� �ִϸ��̼� �ݹ��Լ�
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

    // ������ ü���� �ִϸ��̼� �ݹ�
    public void OnPhaseChangeMove()
    {
        transform.position = phasePos;
    }
    public void OnPhaseChangeEnd()
    {
        fsm.ChangeState("Track");
    }

    // �������� �ִϸ��̼� �ݹ�

    #endregion
    public void ChangeAttackType(JusticeAttackType type)
    {
        currentAttackType = type;
    }
    public void ChangeAttackType()
    {
        currentAttackType = JusticeAttackType.Slash;
    }

    // ���� �ݻ�
    private void Parrying(Vector3 hitPos)
    {
        Vector3 dir = (hitPos - transform.position).normalized;

        GameObject parryingOb = null;
        GameObject sparkOb = null;
        // vfx : �ݻ� �ܻ�
        // ��� ���
        if (dir.y >= 0)
            parryingOb = agentVFXPool.ActiveVFX("ParryingA");
        // �ϴ� ���
        else
            parryingOb = agentVFXPool.ActiveVFX("ParryingB");

        parryingOb.transform.right = -dir;
        parryingOb.transform.position = transform.position + dir * 1.5f;

        parryingSource.Play();
        // vfx : �ݻ� ����ũ 
        sparkOb = agentVFXPool.ActiveVFX("ParryingSpark");
        sparkOb.transform.up = dir;
        sparkOb.transform.position = hitPos + dir * 2f;
    }

    // ����Ȯ�� ī����
    public void Counter()
    {
        // �ٷ� ī���� ���·� ����.
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
        // �Ű� ���� ���
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
        {
            Debug.Log("Hitted Hook");

            // �ݻ�
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
