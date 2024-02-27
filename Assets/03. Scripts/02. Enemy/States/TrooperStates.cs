using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class TrooperBaseState : BaseState
{
    protected EnemyTrooper owner;
}
public class TrooperDetect : TrooperBaseState
{
    private Coroutine detectRoutine;

    public UnityAction<Vector3> OnShooting;

    // Detected Player Position
    private Vector3 targetPos;

    private bool isDetect = false;

    public TrooperDetect(EnemyTrooper owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        isDetect = true;
        detectRoutine = owner.StartCoroutine(DetectRoutine());
    }

    public override void Update()
    {
        if (isDetect)
            Detecting();
    }

    public override void Exit()
    {
        owner.StopCoroutine(detectRoutine);
    }

    public void Detecting()
    {
        targetPos = owner.PlayerTr.position;

        AgentRotation();
        AimRotation();
    }
    // Agent rotate to Player
    private void AgentRotation()
    {
        Vector3 agentPos = owner.transform.position;

        // Agent Rotation
        if (agentPos.x > targetPos.x)
            owner.transform.rotation = Quaternion.Euler(0, -180f, 0);
        else
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    // Aim rotate to Player
    private void AimRotation()
    {
        Vector3 targetToAim = owner.AimPos.position - targetPos;
        float aimRotZ = Mathf.Atan2(targetToAim.y, targetToAim.x) * Mathf.Rad2Deg;

        // Aim Rotation
        owner.AimPos.transform.rotation = Quaternion.Euler(0, 0, aimRotZ);

        // LineRendering
        owner.Lr.positionCount = 2;
        owner.Lr.SetPosition(0, owner.AimPos.position);
        owner.Lr.SetPosition(1, targetPos);
    }

    IEnumerator DetectRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(owner.DetectingTime);
            isDetect = false;
            //yield return new WaitForSeconds(1f);
            //OnShooting?.Invoke(targetPos);
            yield return new WaitForSeconds(owner.AttackCoolTime);
            isDetect = true;
        }
    }
}
public class TrooperGrabbed : TrooperBaseState
{
    public TrooperGrabbed(EnemyTrooper owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
        owner.Anim.Play("Grabbed");
    }
}
public class TrooperDie : TrooperBaseState
{
    public TrooperDie(EnemyTrooper owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Die");
    }
}