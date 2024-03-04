using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Turret : EnemyShooter, IGrabable
{
    private BoxCollider2D boxCol;
    public BoxCollider2D BoxCol { get { return boxCol; } }

    protected override void Awake()
    {
        base.Awake();

        boxCol = GetComponent<BoxCollider2D>();

        fsm.AddState("PopUp", new TurretPopUp(this));

        TurretDetect detect = new TurretDetect(this);
        detect.OnEnableDetect += () => detect.detectRoutine = StartCoroutine(detect.DetectRoutine());
        detect.OnDisableDetect += () => StopCoroutine(detect.detectRoutine);

        fsm.AddState("Detect", detect);
        fsm.AddState("Grabbed", new TurretGrabbed(this));
        fsm.AddState("Die", new TurretDie(this));

        fsm.Init("PopUp");
        initState = "PopUp";
    }

    public override void Detecting(out Vector3 targetPos)
    {
        targetPos = playerTr.transform.position;

        lrAnim.Play("DetectAim");

        lr.positionCount = 2;
        lr.SetPosition(0, muzzlePos.position);
        lr.SetPosition(1, (targetPos - muzzlePos.position).normalized * 100f);

        // Agent Rotation
        if (transform.position.x > targetPos.x)
            transform.rotation = Quaternion.Euler(0, -180f, 0);
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);

        // Aim Rotation
        Vector3 dir = (targetPos - aimPos.position).normalized;
        aimPos.up = dir;
    }
    public override void Shooting()
    {
        anim.Play("Attack");
        lr.positionCount = 0;

        EnemyBulletObject bullet = Manager.Pool.GetPool(bulletPrefab, muzzlePos.position, aimPos.rotation) as EnemyBulletObject;
        bullet.transform.up = aimPos.up;
        bullet.Rigid.AddForce(aimPos.up * bulletPower, ForceMode2D.Impulse);
    }

    public void Grabbed()
    {
        markerAnim.SetBool("IsEnable", false);

        lr.positionCount = 0;
        fsm.ChangeState("Grabbed");
    }
    public void GrabEnd()
    {
        Died();
    }
    public GameObject GetObject() { return this.gameObject; }
    public Vector3 GetGrabPosition() { return new Vector2(this.transform.position.x, this.transform.position.y + grabbedYPos); }
}
