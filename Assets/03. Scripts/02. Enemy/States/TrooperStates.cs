using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class TrooperDetect : EnemyShooterBase
{
    public Coroutine detectRoutine;
    public UnityAction OnDisableDetect;
    public UnityAction OnEnableDetect;
    // Detected Player Position
    private Vector3 targetPos;

    private bool isDetect = false;

    public TrooperDetect(EnemyShooter owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        isDetect = true;
        OnEnableDetect?.Invoke();
        Debug.Log(detectRoutine);
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
            Debug.Log("Tropper aiming");
            owner.Aiming(in targetPos);
            yield return new WaitForSeconds(owner.AttackDelay);
            Debug.Log("Tropper shooting");
            owner.Shooting();
            yield return new WaitForSeconds(1.5f);
            isDetect = true;
            owner.Anim.Play("Detect");
        }
    }
}
public class TrooperGrabbed : EnemyShooterBase
{
    public TrooperGrabbed(EnemyShooter owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
        owner.Anim.Play("Grabbed");
    }
}
public class TrooperDie : EnemyShooterBase
{
    public TrooperDie(EnemyShooter owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Die");
    }
}