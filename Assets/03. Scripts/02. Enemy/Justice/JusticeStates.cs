using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class JusticeBaseState : BaseState
{
    protected Justice owner;
}

public class Track : JusticeBaseState
{
    // �����ð����� �÷��̾� Ʈ��ŷ
    // : �����ð��ȿ� �÷��̾ �������� ������ ��� �÷��̾� ��ġ�� �����̵�

    private Coroutine trackingRoutine;

    public Track(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        trackingRoutine = owner.StartCoroutine(TrackingTimer());
    }

    public override void FixedUpdate()
    {
        Tracking();
    }

    public override void Exit()
    {
        if (trackingRoutine != null)
            owner.StopCoroutine(trackingRoutine);
    }

    private void Tracking()
    {
        Vector3 dir = (owner.PlayerTr.position - owner.transform.position).normalized;
        owner.Rigid.AddForce(dir * owner.MoveSpeed);
    }

    IEnumerator TrackingTimer()
    {
        yield return new WaitForSeconds(owner.TrackingTime);
        owner.FSM.ChangeState("Teleport");
    }
}
public class Teleport : JusticeBaseState
{
    // �����ð� �� �÷��̾� ��ġ�� �����̵�
    // �̵� ��� ���� �����þ��� ����
    private Coroutine teleportDelayTimer;
    public Teleport(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        owner.Anim.Play("TeleportStart");
        teleportDelayTimer = owner.StartCoroutine(TeleportDelayTimer());
    }

    public override void Exit()
    {
        if(teleportDelayTimer != null)
            owner.StopCoroutine(teleportDelayTimer);
    }

    IEnumerator TeleportDelayTimer()
    {
        yield return new WaitForSeconds(owner.TeleportTime);
        // ���� Ÿ�� ����
        owner.ChangeAttackType(JusticeAttackType.CircleSlash);
        owner.transform.position = owner.PlayerTr.position;
        owner.FSM.ChangeState("Charge");
    }
}

public class Charge : JusticeBaseState
{
    // ���ݹ��� ����, vfx ���
    private Coroutine chargeTimer;
    private bool isAiming = false;
    public Charge(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        switch (owner.currentAttackType)
        {
            case JusticeAttackType.Slash:
                owner.Anim.Play("SlashAttackStart");
                isAiming = true;
                break;
            case JusticeAttackType.CircleSlash:
                isAiming = false;
                break;
            case JusticeAttackType.DashSlash:
                owner.Anim.Play("DashAttackStart");
                isAiming = true;
                break;
        }
        chargeTimer = owner.StartCoroutine(ChargeTimer());  
    }

    private void AimRotation()
    {
    }

    public override void Exit()
    {
        if(chargeTimer != null)
            owner.StopCoroutine(chargeTimer);

        // vfx end
    }

    IEnumerator ChargeTimer()
    {
        float time = 0;
        while(time < owner.AttackChargeTime)
        {
            if(isAiming)
                AimRotation();

            time += Time.deltaTime;
        }

        // ���� �������� ������ ���
        if (owner.currentAttackType == JusticeAttackType.CircleSlash)
        {
            owner.Anim.Play("TeleportEnd");
            yield return new WaitForSeconds(0.5f);
        }

        yield return null;
        owner.FSM.ChangeState("Attack");
    }
}

public class Attack : JusticeBaseState
{
    // �� Ÿ�Ժ� ���� ����
    public Attack(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        switch(owner.currentAttackType)
        {
            case JusticeAttackType.Slash:
                owner.Anim.Play("SlashAttack");
                break;
            case JusticeAttackType.CircleSlash:
                owner.Anim.Play("CircleSlashAttack");
                break;
            case JusticeAttackType.DashSlash:
                owner.Anim.Play("DashAttack");
                break;
        }
    }
}

public class Groggy : JusticeBaseState
{
    // � ���¿����� ���̵� �� �ִ� Groggy����
    // ������ ��� �ı��� ��� Groggy ���� ����
    public Groggy(Justice owner) { this.owner = owner; }

}

