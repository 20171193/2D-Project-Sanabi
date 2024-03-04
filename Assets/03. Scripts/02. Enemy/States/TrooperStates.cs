using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            owner.LrAnim.Play("Aiming");
            owner.MarkerAnim.SetTrigger("IsAttack");
            yield return new WaitForSeconds(owner.AttackDelay);
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
    private CapsuleCollider2D col;

    public TrooperDie(EnemyShooter owner)
    {
        this.owner = owner;
        col = owner.GetComponent<CapsuleCollider2D>();
    }

    public override void Enter()
    {
        col.enabled = false;
        owner.Anim.Play("Die");
    }
    public override void Exit()
    {
        Debug.Log($"{col.gameObject} : col on");
        col.enabled = true;
    }
}