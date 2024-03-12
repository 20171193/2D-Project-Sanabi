using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pillar : MonoBehaviour
{
    [SerializeField]
    public float spawnYpos;
    [SerializeField]
    public Transform[] spawnTr;

    public GameObject spawnedRailCar;

    public Vector3 GetSpawnPos(int index)
    {
        return new Vector3(spawnTr[index].position.x, spawnTr[index].position.y + spawnYpos, spawnTr[index].position.z);
    }
}
