using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyTrooper : Enemy
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Transform aimPos;
    public Transform AimPos { get { return aimPos; } }

    [SerializeField]
    private LineRenderer lr;
    public LineRenderer Lr { get { return lr; } }

    [SerializeField]
    private GameObject bulletPrefab;
    public GameObject BulletPrefab { get { return bulletPrefab; } }

    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private float attackCoolTime;
    public float AttackCoolTime { get { return attackCoolTime; } }

    [SerializeField]
    private float detectingTime;
    public float DetectingTime { get { return detectingTime; } }

    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private Vector3 patrollDestination;

    [SerializeField]
    private float bulletPower;

    //[Header("Ballancing")]
    protected override void Awake()
    {
        base.Awake();

        grabbedYPos = 1.5f;

        fsm.AddState("Detect", new TrooperDetect(this));
        fsm.AddState("Grabbed", new TrooperGrabbed(this));
        fsm.AddState("Die", new TrooperDie(this));

        // regist Action
        TrooperDetect td = (TrooperDetect)fsm.StateDic["Detect"];
        td.OnShooting += Shooting;  // regist Shooting

        // test
        fsm.Init("Detect");
    }

    public void Shooting(Vector3 targetPos)
    {
        Vector3 dist = targetPos - aimPos.position;
        float zRot = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(bulletPrefab, aimPos.position + dist.normalized * 2.0f, Quaternion.Euler(0, 0, zRot));
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        rigid.AddForce(dist.normalized * bulletPower, ForceMode2D.Impulse);
    }

    public override void Grabbed()
    {
        fsm.ChangeState("Grabbed");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
            anim.Play("HookHitted");
    }
}