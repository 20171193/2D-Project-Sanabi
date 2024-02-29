using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chapter Data", menuName = "Scriptable Object/Chapter Data", order = int.MaxValue)]
public class ChapterData : ScriptableObject
{
    [Header("Tropper Spawn Count")]
    [SerializeField]
    private int tropperSpawnCount;
    public int TropperSpawnCount { get { return tropperSpawnCount; } }

    [Header("Turret Spawn Count")]
    [SerializeField]
    private int turretSpawnCount;
    public int TurretSpawnCount { get { return turretSpawnCount;} }
}
