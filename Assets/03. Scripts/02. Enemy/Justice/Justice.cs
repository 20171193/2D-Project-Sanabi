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
    // 기본 공격 범위
    [SerializeField]
    private float slashAttackRange;
    public float SlashAttackRange { get { return slashAttackRange; } }
    // 대쉬 공격 범위
    [SerializeField]
    private float arcSlashAttackRange;
    public float ArcSlashAttackRange { get { return arcSlashAttackRange; } }

    // 최소 공격 범위
    // (현재 공격 타입의 공격 범위 - 최소 공격 범위)에 플레이어가 존재할 경우 차지 상태로 전환
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

    // 현재 공격 타입
    public JusticeAttackType currentAttackType;

    // 유한상태머신
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
