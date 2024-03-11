using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Justice_Phase", menuName = "Scriptable Object/Justice_Phase", order = int.MaxValue)]
public class JusticePhaseData : ScriptableObject
{
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private float moveSpeed;
    public float MoveSpeed {  get { return moveSpeed; } }

    [SerializeField]
    private int maxHp;
    public int MaxHp { get { return maxHp; } }

    // �⺻ Ʈ��ŷ���� �ڷ���Ʈ�� ��ȯ �ð�
    [SerializeField]
    private float trackingTime;
    public float TrackingTime { get { return trackingTime; } }

    // Ư�� �������� �⺻ ���� ī��Ʈ
    [SerializeField]
    private float slashCount;
    public float SlashCount { get { return slashCount; } }
}
