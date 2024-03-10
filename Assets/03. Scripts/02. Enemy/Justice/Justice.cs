using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Justice : MonoBehaviour
{
    [Header("Specs")]
    [SerializeField]
    private float curHp;
    public float CurHP { get { return curHp; } }

    [SerializeField]
    private float moveSpeed;
    public float MoveSpeed { get { return moveSpeed; } }

    // 기본 공격 범위
    [SerializeField]
    private float slashAttackRange;
    public float SlashAttackRange { get { return slashAttackRange; } }
    // 대쉬 공격 범위
    [SerializeField]
    private float arcSlashAttackRange;
    public float ArcSlashAttackRange { get { return arcSlashAttackRange; } }


    // 유한상태머신
    private StateMachine<Justice> fsm;
    public StateMachine<Justice> FSM { get { return fsm; } }



}
