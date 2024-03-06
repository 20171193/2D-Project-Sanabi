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
    protected Animator lrAnim;
    public Animator LrAnim { get { return lrAnim; } }

    [SerializeField]
    protected Animator shootVFXAnim;
    public Animator ShootVFXAnim { get { return shootVFXAnim; } }

    [SerializeField]
    protected EnemyBulletObject bulletPrefab;
    public EnemyBulletObject BulletPrefab { get { return bulletPrefab; } }

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

    // not use function
    public abstract void Detecting(out Vector3 targetPos);
    public abstract void Shooting();

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