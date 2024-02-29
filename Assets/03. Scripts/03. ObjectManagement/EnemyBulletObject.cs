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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Vector3 forceDir = new Vector3(
                rigid.velocity.x > 0 ? 1f : -1f,
                0.5f,
                0);
            collision.GetComponent<Rigidbody2D>().AddForce(forceDir * knockBackPower, ForceMode2D.Impulse);
        }

        Release();
    }
}
