using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    protected Transform spawnPos;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    protected float spawnTime;

    protected abstract void Spawn();
}
