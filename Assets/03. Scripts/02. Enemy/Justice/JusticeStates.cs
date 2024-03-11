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
    // 일정시간동안 플레이어 트래킹
    // : 일정시간안에 플레이어에 근접하지 못했을 경우 플레이어 위치로 순간이동

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
    // 일정시간 뒤 플레이어 위치로 순간이동
    // 이동 즉시 원형 슬래시어택 실행
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
        // 공격 타입 변경
        owner.ChangeAttackType(JusticeAttackType.CircleSlash);
        owner.transform.position = owner.PlayerTr.position;
        owner.FSM.ChangeState("Charge");
    }
}

public class Charge : JusticeBaseState
{
    // 공격방향 설정, vfx 출력
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

        // 원형 슬래시터 어택일 경우
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
    // 각 타입별 공격 수행
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
    // 어떤 상태에서든 전이될 수 있는 Groggy상태
    // 약점이 모두 파괴된 경우 Groggy 상태 진입
    public Groggy(Justice owner) { this.owner = owner; }

}

