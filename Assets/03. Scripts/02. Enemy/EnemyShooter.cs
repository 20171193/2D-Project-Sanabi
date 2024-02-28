using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class EnemyShooter : Enemy
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    protected Transform aimPos;
    public Transform AimPos { get { return aimPos; } }
    [SerializeField]
    protected Transform muzzlePos;
    public Transform MuzzlePos { get { return muzzlePos; } }


    [SerializeField]
    protected LineRenderer lr;
    public LineRenderer Lr { get { return lr; } }

    [SerializeField]
    protected GameObject bulletPrefab;
    public GameObject BulletPrefab { get { return bulletPrefab; } }

    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    protected float attackCoolTime;
    public float AttackCoolTime { get { return attackCoolTime; } }

    [SerializeField]
    protected float attackDelay;
    public float AttackDelay { get { return attackDelay; } }

    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    protected float bulletPower;
    public float BulletPower { get { return bulletPower; } }

    //[Header("Ballancing")]
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
            anim.Play("HookHitted");
    }
}