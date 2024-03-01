using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrooperSpawner : Spawner
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Trooper trooperPrefab;

    [SerializeField]
    private Animator anim;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private int spawnCount;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    private Trooper spawnedTrooper;

    public void OnEnable()
    {
        anim.Play("Spawn");
    }

    protected override void Spawn()
    {
        spawnCount--;
        spawnedTrooper = Manager.Pool.GetPool(trooperPrefab, spawnPos.position, spawnPos.rotation) as Trooper;
        spawnedTrooper.OnDie += OnTrooperDied;
    }

    public void OnTrooperDied()
    {
        spawnedTrooper.OnDie -= OnTrooperDied;

        if (spawnCount > 0)
            anim.Play("Spawn");
        else
            Destroy(gameObject);
    }

    // Animation Bind
    public void OnAnimationSpawn()
    {
        Spawn();
        anim.Play("SpawnEnd");
    }

    private void OnDisable()
    {
    }
}
