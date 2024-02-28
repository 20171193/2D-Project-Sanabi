using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class TrooperDetect : EnemyShooterBase
{
    private Coroutine detectRoutine;
    public UnityAction<Vector3> OnShooting;
    // Detected Player Position
    private Vector3 targetPos;

    private bool isDetect = false;
    private bool isLineRendering = false;

    public TrooperDetect(EnemyShooter owner)
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
            AimSetting();
        if (isLineRendering)
            LineRendering();
        else
            owner.Lr.positionCount = 0;
    }

    public override void Exit()
    {
        owner.StopCoroutine(detectRoutine);
    }

    public void AimSetting()
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
    private void LineRendering()
    {
        owner.Lr.positionCount = 2;
        owner.Lr.SetPosition(0, owner.AimPos.position);
        owner.Lr.SetPosition(1, (targetPos - owner.AimPos.position) * 10f);
    }
    private void Shooting()
    {
        GameObject bullet = GameObject.Instantiate(owner.BulletPrefab, owner.AimPos.position, owner.AimPos.rotation);
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
            isDetect = true;
            Shooting();
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