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

    // �⺻ ���� ����
    [SerializeField]
    private float slashAttackRange;
    public float SlashAttackRange { get { return slashAttackRange; } }
    // �뽬 ���� ����
    [SerializeField]
    private float arcSlashAttackRange;
    public float ArcSlashAttackRange { get { return arcSlashAttackRange; } }


    // ���ѻ��¸ӽ�
    private StateMachine<Justice> fsm;
    public StateMachine<Justice> FSM { get { return fsm; } }



}
