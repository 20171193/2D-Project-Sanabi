using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSpawner : Spawner
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Turret turretPrefab;

    [SerializeField]
    private Transform[] spawnTr;       

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private int spawnBatchCount;    // 한번에 생성할 몬스터의 개수
    [SerializeField]
    private int maxSpawnCount;      // 최대 몬스터 개수
    [SerializeField]
    private float spawnDelay;

    private Coroutine spawnDelayRoutine;

    private void Awake()
    {
        // 디버그용 렌더러 제거
        foreach(Transform tr in spawnTr)
            tr.GetComponent<SpriteRenderer>().enabled = false;
    }

    public override void EnableSpawner()
    {
        for(int i =0; i<spawnBatchCount; i++)
        {
            Spawn();
        }
    }
    public override void DestroySpawner()
    {
        base.DestroySpawner();
    }

    public override void Spawn()
    {
        if (maxSpawnCount <= 0) return;

        // 랜덤 위치 인덱스 추출
        int idx = GetRandIndex();
        if (idx == -1) return;

        Turret spawned = Manager.Pool.GetPool(turretPrefab, spawnTr[idx].position, spawnTr[idx].rotation) as Turret;
        spawned.transform.parent = spawnTr[idx];    // 랜덤으로 뽑은 위치의 transform을 부모로 할당
                                                    // 랜덤 위치를 뽑을 때 사용 : 자식이 없는 transform을 비어있는 위치로 확인
        maxSpawnCount--;
        spawned.OnTurretDie += OnTurretDied;
    }
    public void OnTurretDied(Turret turret)
    {
        if (maxSpawnCount <= 0) return;

        turret.OnTurretDie -= OnTurretDied;
        spawnDelayRoutine = StartCoroutine(SpawnDelayRoutine());
    }
    private int GetRandIndex()
    {
        List<int> indexList = new List<int>();
        
        // 후보 인덱스 추출
        for(int i =0; i< spawnTr.Length; i++)
        {
            if (spawnTr[i].childCount == 0)
                indexList.Add(i);
        }

        // 후보 인덱스가 존재하지 않을 경우 -1 리턴
        if (indexList.Count < 1) return -1;

        // 랜덤 인덱스 리턴
        return Random.Range(0, indexList.Count);
    }


    IEnumerator SpawnDelayRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);
        Spawn();
    }
}
