using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GameScene : MonoBehaviour
{
    [SerializeField]
    private PooledObject eneTrooperPrefab;
    [SerializeField]
    private PooledObject eneTurretPrefab;
    [SerializeField]
    private PooledObject enemyBullet;

    [SerializeField]
    private GameChapter curChapter;
    public GameChapter CurChapter { get { return curChapter; }  set { curChapter = value; } }

    private void Start()
    {
        Manager.Pool.CreatePool(enemyBullet, 15, 30);
        Manager.Pool.CreatePool(eneTrooperPrefab, 5, 10);
        //Manager.Pool.CreatePool(eneTurretPrefab, 5, 10);
    }
}
