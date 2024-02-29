using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GameScene : MonoBehaviour
{
    [SerializeField]
    private PooledObject enemyBullet;

    private void Start()
    {
        Manager.Pool.CreatePool(enemyBullet, 30, 50);
    }
}
