using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletObject : PooledObject
{
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Release();
    }
}
