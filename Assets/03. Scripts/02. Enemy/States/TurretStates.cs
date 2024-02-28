using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurretPopUp : EnemyShooterBase
{
    private Coroutine popupRoutine;
    public TurretPopUp(EnemyShooter owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("PopUp");
        popupRoutine = owner.StartCoroutine(PopUpRoutine());
    }

    public override void Exit()
    {
        Enable();
    }


    private void Enable()
    {
        owner.AimPos.gameObject.SetActive(true);
        owner.Lr.enabled = true;

        Turret turret = owner as Turret;
        turret.BoxCol.enabled = true;
    }

    IEnumerator PopUpRoutine()
    {
        yield return new WaitForSeconds(3.0f);
        owner.FSM.ChangeState("Detect");
    }
}

public class TurretDetect : EnemyShooterBase
{
    private Coroutine detectRoutine;
    public UnityAction<Vector3> OnShooting;
    // Detected Player Position
    private Vector3 targetPos;

    private bool isDetect = false;
    private bool isLineRendering = false;

    public TurretDetect(EnemyShooter owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Detect");
        isDetect = true;
        detectRoutine = owner.StartCoroutine(DetectRoutine());
    }
    public override void Update()
    {
        if (isDetect)
            Detecting();
        if (isLineRendering)
            LineRendering();
        else
            owner.Lr.positionCount = 0;
    }
    public override void Exit()
    {
        owner.StopCoroutine(detectRoutine);
    }

    public void Detecting()
    {
        targetPos = owner.PlayerTr.position;
        
        AimRotation();
    }
    // Aim rotate to Player
    private void AimRotation()
    {
        Vector3 dir = (targetPos - owner.AimPos.position).normalized;
        owner.AimPos.up = dir;

        //if (targetPos.x < owner.Rigid.position.x)
        //    owner.AimPos.transform.rotation = Quaternion.Euler(0, -180, owner.AimPos.transform.rotation.z);
        //else
        //    owner.AimPos.transform.rotation = Quaternion.Euler(0, 0, owner.AimPos.transform.rotation.z);
    }

    private void LineRendering()
    {
        owner.Lr.positionCount = 2;
        owner.Lr.SetPosition(0, owner.MuzzlePos.position);
        owner.Lr.SetPosition(1, targetPos);
    }
    private void Shooting()
    {
        GameObject bullet = GameObject.Instantiate(owner.BulletPrefab, owner.MuzzlePos.position, owner.AimPos.rotation);
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        rigid.AddForce(owner.AimPos.up * owner.BulletPower, ForceMode2D.Impulse);
    }

    IEnumerator DetectRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(owner.AttackCoolTime);
            isDetect = false;
            isLineRendering = true;
            yield return new WaitForSeconds(owner.AttackDelay);
            isLineRendering = false;
            owner.Anim.Play("Shooting");
            Shooting();
            yield return new WaitForSeconds(1.5f);
            isDetect = true;
            owner.Anim.Play("Detect");
        }
    }
}

public class TurretGrabbed : EnemyShooterBase
{
    public TurretGrabbed(EnemyShooter owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
        owner.Anim.Play("Grabbed");
    }
}
public class TurretDie : EnemyShooterBase
{
    public TurretDie(EnemyShooter owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
        owner.Anim.Play("Die");
    }
}