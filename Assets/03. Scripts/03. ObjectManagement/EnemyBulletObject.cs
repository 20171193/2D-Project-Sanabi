using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletObject : PooledObject
{
    [SerializeField]
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }

    [SerializeField]
    private float knockBackPower;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
            collision.rigidbody.AddForce(transform.up * knockBackPower, ForceMode2D.Impulse);

        Release();
    }
}
