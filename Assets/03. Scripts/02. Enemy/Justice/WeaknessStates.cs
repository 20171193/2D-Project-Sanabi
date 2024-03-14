using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class WeaknessBaseState : BaseState
{
    protected Weakness owner;

    protected Coroutine exitTimer;

    protected void ChangeWithDelay(string next) 
    {
        exitTimer = owner.StartCoroutine(ExitTimer(next));
    }
    IEnumerator ExitTimer(string next)
    {
        yield return new WaitForSeconds(0.5f);
        owner.FSM.ChangeState(next);
    }
}

public class Default : WeaknessBaseState
{
}

public class Appear : WeaknessBaseState
{
    public Appear(Weakness owner) { this.owner= owner; }
    public override void Enter()
    {
        owner.Anim.Play("Appear");
        ChangeWithDelay("Idle");
    }
}

public class DisAppear : WeaknessBaseState
{
    public DisAppear(Weakness owner) { this.owner = owner; }
    public override void Enter()
    {
        owner.Anim.Play("DisAppear");
    }
}
public class Active : WeaknessBaseState
{
    public Active(Weakness owner) { this.owner = owner; }
    public override void Enter()
    {
        owner.Anim.Play("Activate");
        owner.IsActive = true;
        ChangeWithDelay("Idle");
    }
}

public class Idle : WeaknessBaseState
{
    public Idle(Weakness owner) { this.owner = owner; }

    public override void Enter()
    {
        if (owner.IsActive)
        {
            // 활성화되어있는 중에만 콜라이더 활성화
            owner.CapCol.enabled = true;
            owner.Anim.Play("Active_Idle");
        }
        else
        {
            owner.CapCol.enabled = false;
            owner.Anim.Play("InActive_Idle");
        }
    }

    public override void Update()
    {
        Rotation();
    }
    public override void Exit()
    {
        owner.CapCol.enabled = false;
    }

    private void Rotation()
    {
        Vector3 lookDir = (owner.JusticeTr.position - owner.transform.position).normalized;
        owner.transform.up = lookDir;
    }
}

public class Destroy : WeaknessBaseState
{
    public Destroy(Weakness owner) { this.owner = owner; }

    public override void Enter()
    {
        owner.Anim.Play("Destroy");
        owner.CapCol.enabled = false;
        owner.IsActive = false;
        owner.OnDestroyed?.Invoke();
        ChangeWithDelay("Default");
    }
}