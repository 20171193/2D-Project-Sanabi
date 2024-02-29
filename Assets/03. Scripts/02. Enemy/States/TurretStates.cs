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
    public Coroutine detectRoutine;
    public UnityAction OnDisableDetect;
    public UnityAction OnEnableDetect;
    // Detected Player Position
    private Vector3 targetPos;

    private bool isDetect = false;

    public TurretDetect(EnemyShooter owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        isDetect = true;
        OnEnableDetect?.Invoke();
    }

    public override void Update()
    {
        if (isDetect)
            owner.Detecting(out targetPos);
    }

    public override void Exit()
    {
        OnDisableDetect?.Invoke();
    }

    public IEnumerator DetectRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(owner.AttackCoolTime);
            isDetect = false;
            owner.Aiming(in targetPos);
            yield return new WaitForSeconds(owner.AttackDelay);
            owner.Shooting();
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