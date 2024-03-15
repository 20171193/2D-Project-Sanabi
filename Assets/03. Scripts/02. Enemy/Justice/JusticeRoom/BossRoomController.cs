using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [Space(3)]
    [Header("BackGround Move")]
    [Space(2)]
    [SerializeField]
    private Transform[] movingGround1;
    [SerializeField]
    private Transform[] movingGround2;

    [Header("Specs")]
    [SerializeField]
    private float ground1MoveSpeed;
    [SerializeField]
    private float ground2MoveSpeed;


    [Space(3)]
    [Header("Spawn Pillar Move")]
    [Space(2)]
    [SerializeField]
    private Pillar[] movingLeftPillar;
    [SerializeField]
    private Pillar[] movingMiddlePillar;
    [SerializeField]
    private Pillar[] movingRightPillar;

    [Header("Specs")]
    [SerializeField]
    private float leftPillarSpeed;
    [SerializeField]
    private float middlePillarSpeed;
    [SerializeField]
    private float rightPillarSpeed;

    [Space(3)]
    [Header("RailCars")]
    [Space(2)]
    [SerializeField]
    private List<GameObject> railCarPrefabs;

    private GameObject dummyPooler = null;
    private Stack<GameObject> railCarPool;

    private const int GroundHeight = 55;

    [SerializeField]
    private bool isRunning = false;
    public bool IsRunning { get { return isRunning; } }

    [SerializeField]
    private bool isSpawn = false;
    public bool IsSpawn { get { return isSpawn; } set { isSpawn = value; } }

    private void Awake()
    {
        railCarPool = new Stack<GameObject>();


        if (railCarPrefabs.Count < 1) return;

        dummyPooler = new GameObject();
        dummyPooler.name = "Pool_RailCar";

        // 활성화 : 3개
        // 비활성화 : 3개
        // 비활성화->활성화 시 겹치는 경우를 고려해서 각 레일카를 6개씩 풀링
        for (int i = 0; i < 20; i++)
        {
            foreach(GameObject rc in railCarPrefabs)
            {
                GameObject inst = Instantiate(rc);
                inst.GetComponent<RailCar>().Pooler = this;
                inst.transform.parent = dummyPooler.transform;
                inst.SetActive(false);
                railCarPool.Push(inst);
            }
        }

    }

    private void Update()
    {
        if (isRunning)
            MoveGround();
    }

    private void MoveGround()
    {
        // Moving Ground
        foreach(Transform ground in movingGround1)
        {
            ground.Translate(Vector3.up * ground1MoveSpeed * Time.deltaTime);

            if (ground.position.y > GroundHeight)
                ground.position = new Vector3(ground.position.x, -ground.position.y*2, ground.position.z);
        }

        foreach (Transform ground in movingGround2)
        {
            ground.Translate(Vector3.up * ground2MoveSpeed * Time.deltaTime);

            if (ground.position.y > GroundHeight)
                ground.position = new Vector3(ground.position.x, -ground.position.y * 2, ground.position.z);
        }

        // Moving Pillar
        foreach (Pillar pillar in movingLeftPillar)
        {
            pillar.transform.Translate(Vector3.up * leftPillarSpeed * Time.deltaTime);


            if (pillar.transform.position.y > GroundHeight - 2f)
            {
                pillar.transform.position = new Vector3(pillar.transform.position.x, -pillar.transform.position.y * 2, pillar.transform.position.z);
                
                if(isSpawn)
                    PillarReset(pillar);
            }
        }

        foreach (Pillar pillar in movingMiddlePillar)
        {
            pillar.transform.Translate(Vector3.up * middlePillarSpeed * Time.deltaTime);


            if (pillar.transform.position.y > GroundHeight - 2f)
            {
                pillar.transform.position = new Vector3(pillar.transform.position.x, -pillar.transform.position.y * 2, pillar.transform.position.z);
                
                if (isSpawn)
                    PillarReset(pillar);
            }
        }

        foreach (Pillar pillar in movingRightPillar)
        {
            pillar.transform.Translate(Vector3.up * rightPillarSpeed * Time.deltaTime);


            if (pillar.transform.position.y > GroundHeight - 2f)
            {
                pillar.transform.position = new Vector3(pillar.transform.position.x, -pillar.transform.position.y * 2, pillar.transform.position.z);
                
                if (isSpawn)
                    PillarReset(pillar);
            }
        }
    }

    // 풀링 방식
    private GameObject GetRailCar()
    {
        if (railCarPool.Count > 0)
        {
            GameObject inst = railCarPool.Pop();
            inst.transform.parent = null;
            inst.SetActive(true);
            return inst;
        }
        // 그 외 경우는 발생할 수 없음
        // 디버그로 처리
        else
        {
            Debug.Log("Rail Car Pool is Empty");
            return null;
        }
    }
    public void ReturnRailCar(GameObject rc)
    {
        if (rc.activeSelf)
            rc.SetActive(false);

        rc.transform.parent = dummyPooler.transform;
        railCarPool.Push(rc);
    }

    private void PillarReset(Pillar pillar)
    {
        // 기존에 활성화 되었던 레일카 리셋
        if (pillar.spawnedRailCar != null)
        {
            ReturnRailCar(pillar.spawnedRailCar);
            pillar.spawnedRailCar = null;
        }

        GameObject rc = GetRailCar();
        pillar.spawnedRailCar = rc;
        rc.transform.position = pillar.GetSpawnPos(2);
        rc.transform.parent = pillar.transform;

        GameObject rc2 = GetRailCar();
        pillar.spawnedRailCar = rc2;
        rc2.transform.position = pillar.GetSpawnPos(5);
        rc2.transform.parent = pillar.transform;
    }
}

