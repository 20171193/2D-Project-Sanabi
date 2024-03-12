using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class JusticeBaseState : BaseState
{
    protected Justice owner;
}

public class Init : JusticeBaseState
{
    // 공격을 시작하기 이전 상태
    private Coroutine initRoutine;
    public Init(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        owner.Anim.Play("Init");

        initRoutine = owner.StartCoroutine(Extension.DelayRoutine(1.5f, () => owner.FSM.ChangeState("BattleMode")));
    }
    public override void Exit()
    {
        if (initRoutine != null)
            owner.StopCoroutine(initRoutine);
    }
}
public class BattleMode : JusticeBaseState
{
    // 공격모드 진입 상태
    private Coroutine battleModeTimer;
    public BattleMode(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        owner.Anim.Play("ActiveBattleMode");
        battleModeTimer = owner.StartCoroutine(Extension.DelayRoutine(1.5f, ()=>owner.FSM.ChangeState("Track")));
    }
    public override void Exit()
    {
        if (battleModeTimer != null)
            owner.StopCoroutine(battleModeTimer);
    }
}
public class Track : JusticeBaseState
{
    // 일정시간동안 플레이어 트래킹
    // : 일정시간안에 플레이어에 근접하지 못했을 경우 플레이어 위치로 순간이동

    // 전이 상태
    // - 트래킹 -> 차지
    // - 트래킹 -> 텔레포트

    private Coroutine trackingRoutine;
    private bool isTracking = true;

    private Coroutine animationRoutine;

    public Track(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        // 약점 표시
        if (owner.WeaknessController.IsDisAppear)
            owner.WeaknessController.AppearAll();

        // 애니메이션 전환 코루틴 실행
        owner.Anim.Play("MoveStart");
        // MoveStart -> Moving
        animationRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.Anim.Play("Moving")));

        // 트래킹 코루틴 실행
        trackingRoutine = owner.StartCoroutine(TrackingRoutine());
    }

    public override void Update()
    {
        if (!isTracking) return;
        
        AgentRotation();
        Tracking();
    }

    public override void Exit()
    {
        if (trackingRoutine != null)
            owner.StopCoroutine(trackingRoutine);
    }

    private void AgentRotation()
    {
        if (owner.PlayerTr.transform.position.x >= owner.transform.position.x)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }
    private void Tracking()
    {
        Vector3 distToPlayer = owner.PlayerTr.position - owner.transform.position;
        owner.transform.Translate(distToPlayer.normalized * owner.MoveSpeed * Time.deltaTime, Space.World);

        // 트래킹 중 탐색도 같이 실시
        Detecting(distToPlayer.magnitude);
    }

    private void Detecting(float distance)
    {
        // 타깃이 공격범위에 들어와있는지 체크
        if ((owner.CurrentAttackType == JusticeAttackType.Slash && distance <= owner.SlashAttackRange - Justice.Attack_Threshold) ||
            (owner.CurrentAttackType == JusticeAttackType.DashSlash && distance <= owner.DashSlashAttackRange - Justice.Attack_Threshold))
        {
            isTracking = false;

            // 상태 전환 코루틴 실행
            owner.Anim.Play("MoveEnd");
            // 트래킹 -> 차지
            animationRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.FSM.ChangeState("Charge")));
        }
    }

    IEnumerator TrackingRoutine()
    {
        isTracking = true;
        yield return new WaitForSeconds(owner.TrackingTime);
        isTracking = false;

        // 상태 전환 코루틴 실행
        owner.Anim.Play("MoveEnd");
        // 트래킹 -> 텔레포트
        animationRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.FSM.ChangeState("Teleport")));
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
        if (!owner.WeaknessController.IsDisAppear)
            owner.WeaknessController.DisAppearAll();

        owner.Anim.Play("TeleportStart");
        // 텔레포트 딜레이 시간만큼 딜레이 후 상태전환
        teleportDelayTimer = owner.StartCoroutine(Extension.DelayRoutine(owner.TeleportTime, () => TeleportEnd()));
    }
    public override void Exit()
    {
        if(teleportDelayTimer != null)
            owner.StopCoroutine(teleportDelayTimer);
    }

    private void TeleportEnd()
    {
        // 공격 타입 변경
        owner.ChangeAttackType(JusticeAttackType.CloakingSlash);
        owner.transform.position = owner.PlayerTr.position;

        // 상태 전이 : 텔레포트 -> 차지
        owner.FSM.ChangeState("Charge");
    }
}
public class Charge : JusticeBaseState
{
    // 공격방향 설정, vfx 출력
    private Coroutine chargeTimer;
    private JusticeVFX vfx;

    private bool isAiming = false;
    public Charge(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        Debug.Log("Justice Charge");

        switch (owner.CurrentAttackType)
        {
            case JusticeAttackType.Slash:
                owner.Anim.Play("SlashAttackStart");

                // VFX 활성화
                vfx = owner.ChargeVFXPool.ActiveVFX("SlashCharge").GetComponent<JusticeVFX>();
                isAiming = true;
                break;
            case JusticeAttackType.CircleSlash:
                isAiming = false;

                // VFX 활성화
                vfx = owner.ChargeVFXPool.ActiveVFX("CircleSlashCharge").GetComponent<JusticeVFX>();
                break;
            case JusticeAttackType.DashSlash:
                owner.Anim.Play("DashAttackStart");

                // VFX 활성화
                vfx = owner.ChargeVFXPool.ActiveVFX("DashSlashCharge").GetComponent<JusticeVFX>();
                isAiming = true;
                break;
            case JusticeAttackType.CloakingSlash:
                isAiming = false;

                // VFX 활성화
                vfx = owner.ChargeVFXPool.ActiveVFX("CloakingSlashCharge").GetComponent<JusticeVFX>();
                break;
        }

        chargeTimer = owner.StartCoroutine(ChargeTimer());  
    }

    private void Aiming()
    {
        owner.AttackDir = (owner.PlayerTr.position - owner.transform.position).normalized;
        vfx.transform.up = owner.AttackDir;
        vfx.transform.position = owner.transform.position + vfx.transform.up * 10f;
    }
    private void AgentRotation()
    {
        if (owner.PlayerTr.transform.position.x >= owner.transform.position.x)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    public override void Exit()
    {
        if(chargeTimer != null)
            owner.StopCoroutine(chargeTimer);

        owner.Rigid.velocity = Vector3.zero;
        // vfx end
        vfx.Release();
    }

    IEnumerator ChargeTimer()
    {
        float time = 0;
        float chargeTime = owner.CurAttackChargeTime;

        while (time < chargeTime)
        {
            if(isAiming)
                Aiming();

            AgentRotation();
            time += Time.deltaTime;
            yield return null;
        }

        // 텔레포트 어택일 경우
        if (owner.CurrentAttackType == JusticeAttackType.CloakingSlash)
        {
            owner.Anim.Play("TeleportEnd");
            yield return new WaitForSeconds(0.3f);
        }


        // 상태 전이 : 차지 -> 어택
        owner.FSM.ChangeState("Attack");
        yield return null;
    }
}
public class Attack : JusticeBaseState
{
    // 각 타입별 공격 수행
    private Coroutine attackRoutine;
    private JusticeVFX vfx;
    public Attack(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        Debug.Log("Justice Attack");

        switch (owner.CurrentAttackType)
        {
            case JusticeAttackType.Slash:
                owner.Anim.Play("SlashAttack");
                // vfx 출력
                vfx = owner.AgentVFXPool.ActiveVFX("Slash").GetComponent<JusticeVFX>();

                owner.CurSlashCount++;
                break;
            case JusticeAttackType.CircleSlash:
            case JusticeAttackType.CloakingSlash:
                owner.Anim.Play("CircleSlashAttack");
                vfx = owner.AgentVFXPool.ActiveVFX("CircleSlash").GetComponent<JusticeVFX>();

                owner.CurSlashCount++;
                break;
            case JusticeAttackType.DashSlash:
                DashSlash();
                owner.Anim.Play("DashAttack");
                vfx = owner.AgentVFXPool.ActiveVFX("DashSlash").GetComponent<JusticeVFX>();
                // 대쉬 공격 진행 후 slashcount 초기화
                owner.CurSlashCount = 0;
                break;
        }

        // vfx 회전
        vfx.transform.up = owner.AttackDir;

        attackRoutine = owner.StartCoroutine(AttackDelayTimer());
    }
    public override void Exit()
    {
        if (attackRoutine != null)
            owner.StopCoroutine(attackRoutine);
        
        vfx.Release();
    }

    private void VFXRotation()
    {
    }

    private void DashSlash()
    {
        owner.Rigid.AddForce(owner.AttackDir * 50f, ForceMode2D.Impulse);
    }


    IEnumerator AttackDelayTimer()
    {
        yield return new WaitForSeconds(0.3f);
        owner.Rigid.velocity = Vector3.zero;

        yield return new WaitForSeconds(owner.AttackDelayTime);
        // 상태 전이 : 어택 -> 트래킹
        owner.FSM.ChangeState("Track");
    }
}
public class Counter : JusticeBaseState
{
    // 어떤 상태에서든 전이될 수 있는 상태
    // 트래킹 중 랜덤으로 전이 카운터 상황에서 공격을 받을 경우 즉시 카운터 어택 실행
    public Counter(Justice owner) { this.owner = owner; }


}

public class Groggy : JusticeBaseState
{
    // 어떤 상태에서든 전이될 수 있는 상태
    // 약점이 모두 파괴된 경우 Groggy 상태 진입
    public Groggy(Justice owner) { this.owner = owner; }

}

