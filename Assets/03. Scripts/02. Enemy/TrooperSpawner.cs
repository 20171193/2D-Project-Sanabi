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
    protected Transform spawnPos;

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

    public override void EnableSpawner()
    {
        anim.SetBool("IsEnable", true);
        anim.SetTrigger("OnSpawn");
    }
    public override void DestroySpawner()
    {
        base.DestroySpawner();
        Debug.Log("Destroy Trooper Spawner");
    }

    public override void Spawn()
    {
        Debug.Log("Spawn!");
        spawnCount--;
        spawnedTrooper = Manager.Pool.GetPool(trooperPrefab, spawnPos.position, spawnPos.rotation) as Trooper;
        spawnedTrooper.OnDie += OnTrooperDied;
    }

    public void OnTrooperDied()
    {
        spawnedTrooper.OnDie -= OnTrooperDied;

        if (spawnCount > 0)
            anim.SetTrigger("OnSpawn");
        else
            anim.SetBool("IsEnable", false);
    }

    // Animation Bind
    public void OnAnimationSpawn()
    {
        Debug.Log("Spawn Bind!");
        Spawn();
    }
    public void OnAnimationDisable()
    {
        Debug.Log("Trooper Spawner Destroy");
        DestroySpawner();
    }
}
