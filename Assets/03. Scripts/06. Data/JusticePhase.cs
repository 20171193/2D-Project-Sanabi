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

    // 기본 트래킹에서 텔레포트로 전환 시간
    [SerializeField]
    private float trackingTime;
    public float TrackingTime { get { return trackingTime; } }

    // 특수 공격이전 기본 공격 카운트
    [SerializeField]
    private float slashCount;
    public float SlashCount { get { return slashCount; } }
}
