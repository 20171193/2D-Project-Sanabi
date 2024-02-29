using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Turret : EnemyShooter
{
    private BoxCollider2D boxCol;
    public BoxCollider2D BoxCol { get { return boxCol; } }

    protected override void Awake()
    {
        base.Awake();

        boxCol = GetComponent<BoxCollider2D>();

        fsm.AddState("PopUp", new TurretPopUp(this));
        fsm.AddState("Detect", new TurretDetect(this));
        fsm.AddState("Grabbed", new TurretGrabbed(this));
        fsm.AddState("Die", new TurretDie(this));

        fsm.Init("PopUp");
    }

    public override void Detecting(out Vector3 targetPos)
    {
        lr.positionCount = 2;
        targetPos = playerTr.transform.position;

        // Aim Rotation
        Vector3 dir = (targetPos - aimPos.position).normalized;
        aimPos.up = dir;
    }

    public override void Aiming(in Vector3 targetPos)
    {
        base.Aiming(targetPos);
    }

    public override void Shooting()
    {
        anim.Play("Shooting");
        lr.positionCount = 0;
        EnemyBulletObject bullet = Manager.Pool.GetPool(bulletPrefab, muzzlePos.position, aimPos.rotation) as EnemyBulletObject;
        bullet.transform.up = aimPos.up;
        bullet.Rigid.AddForce(aimPos.up * bulletPower, ForceMode2D.Impulse);
    }

    public override void Died()
    {
        Destroy(gameObject, 3f);
        fsm.ChangeState("Die");
    }
    public override void Grabbed(out float holdingYpoint)
    {
        holdingYpoint = grabbedYPos;
        fsm.ChangeState("Grabbed");
    }
}
