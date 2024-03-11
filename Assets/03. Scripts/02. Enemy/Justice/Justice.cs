using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JusticeAttackType
{ 
    Slash,
    DashSlash,
    CircleSlash
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

    //[SerializeField]
    //private Dictionary<string, JusticeVFX>;

    [Header("Specs")]
    // �⺻ ���� ����
    [SerializeField]
    private float slashAttackRange;
    public float SlashAttackRange { get { return slashAttackRange; } }
    // �뽬 ���� ����
    [SerializeField]
    private float arcSlashAttackRange;
    public float ArcSlashAttackRange { get { return arcSlashAttackRange; } }

    // �ּ� ���� ����
    // (���� ���� Ÿ���� ���� ���� - �ּ� ���� ����)�� �÷��̾ ������ ��� ���� ���·� ��ȯ
    [SerializeField]
    public const float Attack_Threshold = 1.5f;


    [Header("Phase Data")]
    [Space(2)]
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

    // ���� ���� �����̽ð� == ���� ���� ǥ�ýð�
    [SerializeField]
    private float attackChargeTime;
    public float AttackChargeTime { get { return attackChargeTime; } }

    [Space(3)]
    [Header("Balancing")]
    [Space(2)]
    [SerializeField]
    private Transform playerTr;
    public Transform PlayerTr { get { return playerTr; } }

    [SerializeField]
    private float curHp;
    public float CurHp { get { return curHp; } }

    // ���� ���� Ÿ��
    public JusticeAttackType currentAttackType;

    // ���ѻ��¸ӽ�
    private StateMachine<Justice> fsm;
    public StateMachine<Justice> FSM { get { return fsm; } }

    private void Awake()
    {
        playerTr = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        fsm.Update();
    }
    private void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    public void ChangeAttackType(JusticeAttackType type)
    {
        currentAttackType = type;
    }
    public void ChangeAttackType()
    {
        currentAttackType = JusticeAttackType.Slash;
    }
}
