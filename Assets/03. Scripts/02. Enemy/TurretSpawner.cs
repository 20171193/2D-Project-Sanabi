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
    private int enabledSpawnCount;    // �ѹ��� ������ �ͷ� ����
    [SerializeField]
    private int maxSpawnCount;      // �ִ� �ͷ� ����
    [SerializeField]
    private float spawnDelay;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private int spawnedCount;       // ������ �ͷ� ����
    [SerializeField]
    private int destroyedCount;     // �ı��� �ͷ� ����

    private Coroutine spawnDelayRoutine;

    private void Awake()
    {
        // ����׿� ������ ����
        foreach(Transform tr in spawnTr)
            tr.GetComponent<SpriteRenderer>().enabled = false;
    }

    public override void EnableSpawner()
    {
        int cnt = enabledSpawnCount <= maxSpawnCount ? enabledSpawnCount : maxSpawnCount;

        for (int i =0; i< cnt; i++)
            Spawn();
        
    }
    public override void DestroySpawner()
    {
        base.DestroySpawner();
        Debug.Log("Destroy Turret Spawner");
    }

    public override void Spawn()
    {
        if (maxSpawnCount <= 0) return;

        // ���� ��ġ �ε��� ����
        int idx = GetRandIndex();
        if (idx == -1) return;

        Turret spawned = Manager.Pool.GetPool(turretPrefab, spawnTr[idx].position, spawnTr[idx].rotation) as Turret;
        spawned.transform.parent = spawnTr[idx];    // �������� ���� ��ġ�� transform�� �θ�� �Ҵ�
                                                    // ���� ��ġ�� ���� �� ��� : �ڽ��� ���� transform�� ����ִ� ��ġ�� Ȯ��
        spawnedCount++;
        maxSpawnCount--;
        spawned.OnTurretDie += OnTurretDied;
    }
    public void OnTurretDied(Turret turret)
    {
        destroyedCount++;
        turret.OnTurretDie -= OnTurretDied;

        if (spawnedCount == destroyedCount && maxSpawnCount < 1)
            DestroySpawner();
        else 
            spawnDelayRoutine = StartCoroutine(SpawnDelayRoutine());
    }
    private int GetRandIndex()
    {
        List<int> indexList = new List<int>();
        
        // �ĺ� �ε��� ����
        for(int i =0; i< spawnTr.Length; i++)
        {
            if (spawnTr[i].childCount == 0)
                indexList.Add(i);
        }

        // �ĺ� �ε����� �������� ���� ��� -1 ����
        if (indexList.Count < 1) return -1;

        // ���� �ε��� ����
        return Random.Range(0, indexList.Count);
    }
    IEnumerator SpawnDelayRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);
        Spawn();
    }
}
