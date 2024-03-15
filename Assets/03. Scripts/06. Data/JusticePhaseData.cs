using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Justice_Phase", menuName = "Scriptable Object/Justice_Phase", order = int.MaxValue)]
public class JusticePhaseData : ScriptableObject
{
    [Serializable]
    public struct DelayTime
    {
        [SerializeField]
        private float slashAttackChargeTime;
        public float SlashAttackChargeTime { get { return slashAttackChargeTime; } }

        [SerializeField]
        private float circleAttackChargeTime;
        public float CircleAttackChargeTime { get { return circleAttackChargeTime; } }

        [SerializeField]
        private float dashSlashAttackChargeTime;
        public float DashSlashAttackChargeTime { get { return dashSlashAttackChargeTime; } }

        [SerializeField]
        private float teleportDelayTime;
        public float TeleportDelayTime { get { return teleportDelayTime; } }

        [SerializeField]
        private float attackDelayTime;
        public float AttackDelayTime { get { return attackDelayTime; } }

        public DelayTime(float slash, float circle, float dash, float teleport, float attack)
        {
            slashAttackChargeTime = slash;
            circleAttackChargeTime = circle;
            dashSlashAttackChargeTime = dash;
            teleportDelayTime = teleport;
            attackDelayTime = attack;
        }
    }

    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private int maxHp;
    public int MaxHp { get { return maxHp; } }

    [SerializeField]
    private float moveSpeed;
    public float MoveSpeed { get { return moveSpeed; } }

    // �⺻ Ʈ��ŷ���� �ڷ���Ʈ�� ��ȯ �ð�
    [SerializeField]
    private float trackingTime;
    public float TrackingTime { get { return trackingTime; } }

    // Ư�� �������� �⺻ ���� ī��Ʈ
    [SerializeField]
    private int slashCount;
    public int SlashCount { get { return slashCount; } }

    [SerializeField]
    private DelayTime delay;
    public DelayTime Delay {get { return delay; }}
}
