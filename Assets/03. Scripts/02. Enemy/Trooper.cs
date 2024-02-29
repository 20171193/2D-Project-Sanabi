using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trooper : EnemyShooter
{
    private CapsuleCollider2D capsuleCol;
    public CapsuleCollider2D CapsuleCol { get { return capsuleCol; } }

    protected override void Awake()
    {
        base.Awake();

        capsuleCol = GetComponent<CapsuleCollider2D>(); 

        TrooperDetect detect = new TrooperDetect(this);
        detect.OnEnableDetect += () => detect.detectRoutine = StartCoroutine(detect.DetectRoutine());
        detect.OnDisableDetect += () => StopCoroutine(detect.detectRoutine);

        fsm.AddState("Detect", new TrooperDetect(this));
        fsm.AddState("Grabbed", new TrooperGrabbed(this));
        fsm.AddState("Die", new TrooperDie(this));

        fsm.Init("Detect");
    }

    public override void Detecting(out Vector3 targetPos)
    {
        lr.positionCount = 2;
        targetPos = playerTr.transform.position;

        // Agent Rotation
        if (transform.position.x > targetPos.x)
            transform.rotation = Quaternion.Euler(0, -180f, 0);
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);

        // Aim Rotation
        Vector3 dir = (targetPos - aimPos.position).normalized;
        aimPos.right = dir;
    }

    public override void Aiming(in Vector3 targetPos)
    {
        base.Aiming(targetPos);
    }

    public override void Shooting()
    {
        base.Shooting();

        EnemyBulletObject bullet = Manager.Pool.GetPool(bulletPrefab, aimPos.position, aimPos.rotation) as EnemyBulletObject;
        bullet.Rigid.AddForce(aimPos.right * bulletPower, ForceMode2D.Impulse);
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
