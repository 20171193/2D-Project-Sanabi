using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class JusticeBaseState : BaseState
{
    protected Justice owner;
}
public class PowerOff : JusticeBaseState
{
    public PowerOff(Justice owner) { this.owner = owner; }
    public override void Enter()
    {
        owner.BodyRenderer.sortingLayerName = "Default";
    }
}
public class PowerOn : JusticeBaseState
{
    // 초기상태
    public PowerOn(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        owner.BodyRenderer.sortingLayerName = "Enemy";
        owner.Anim.Play("PowerOn");
    }
    public override void Exit()
    {
    }
}

public class BeforeBattleMode : JusticeBaseState
{
    public BeforeBattleMode(Justice owner) { this.owner = owner; }

    // 플레이어가 트리거 진입 시
    public override void Enter()
    {
        owner.CircleCol.enabled = true;
        owner.Anim.Play("BeforeBattleMode");
    }
    public override void Exit()
    {
        // 보스 룸 활성화
        owner.BossRoomController.enabled = true;
    }
}
public class BattleMode : JusticeBaseState
{
    // 한번 공격을 받았을 때
    // 공격모드 진입 상태
    public BattleMode(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        // 플레이어 입력제어
        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled = false;

        owner.Anim.Play("BattleMode");
        Manager.Camera.SetCameraPriority(CameraType.CutScene, owner.JusticeCamera);
    }
    public override void Exit()
    {
        // 플레이어 입력제어 해제
        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled = true;

        // 보스 룸 레일카 스폰
        owner.BossRoomController.IsSpawn = true;
        Manager.Camera.SetCameraPriority(CameraType.Main);
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
        owner.CircleCol.enabled = false;
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

        owner.CircleCol.enabled = false;
        owner.EmbientAnim.Play("DisAppear");
        owner.Anim.Play("TeleportStart");
        // 텔레포트 딜레이 시간만큼 딜레이 후 상태전환
        teleportDelayTimer = owner.StartCoroutine(Extension.DelayRoutine(owner.TeleportTime, () => TeleportEnd()));
    }
    public override void Exit()
    {
        if (teleportDelayTimer != null)
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
                // VFX 활성화
                vfx = owner.ChargeVFXPool.ActiveVFX("CircleSlashCharge").GetComponent<JusticeVFX>();
                isAiming = false;
                break;
            case JusticeAttackType.DashSlash:
                owner.Anim.Play("DashAttackStart");
                // VFX 활성화
                vfx = owner.ChargeVFXPool.ActiveVFX("DashSlashCharge").GetComponent<JusticeVFX>();
                isAiming = true;
                break;
            case JusticeAttackType.CloakingSlash:
                // VFX 활성화
                vfx = owner.ChargeVFXPool.ActiveVFX("CloakingSlashCharge").GetComponent<JusticeVFX>();
                isAiming = false;
                break;
        }

        // vfx 초기위치 설정
        vfx.transform.position = owner.transform.position;
        
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

        owner.CircleCol.enabled = true;
        owner.EmbientAnim.Play("Appear");
        owner.WeaknessController.AppearAll();
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
                owner.CircleCol.enabled = true;

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

        // vfx 위치, 회전 세팅
        vfx.transform.position = owner.transform.position;
        vfx.transform.up = owner.AttackDir;

        attackRoutine = owner.StartCoroutine(AttackDelayTimer());
    }
    public override void Exit()
    {
        if (attackRoutine != null)
            owner.StopCoroutine(attackRoutine);
        
        vfx.Release();
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
    public override void Enter()
    {
        owner.CircleCol.enabled = true;
        // 대쉬, 그랩이 가능한 BossGroggy레이어로 변경
        owner.gameObject.layer = LayerMask.NameToLayer("BossGroggy");
        owner.Anim.Play("GroggyStart");
    }
}
public class Grabbed : JusticeBaseState
{
    public Grabbed(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        Time.timeScale = 0.7f;
        owner.gameObject.layer = LayerMask.NameToLayer("PlayerGrabing");
        owner.Anim.Play("GrabbedStart");
    }
    public override void Exit()
    {
    }
}
public class GrabbedEnd : JusticeBaseState
{
    // 데미지를 받는 상태
    public GrabbedEnd(Justice owner) { this.owner = owner; }

    private Coroutine takeDamageRoutine;
    public override void Enter()
    {
        owner.Anim.Play("TakeDamage");
        owner.CurHp--;
        Debug.Log($"CurHP = {owner.CurHp}");
        if (owner.CurHp <= 0)
        {
            Debug.Log($"CurHP = {owner.CurHp}  Change Phase!");
            // 애니메이션 딜레이 이후 페이즈체인지 상태로 변경
            takeDamageRoutine = owner.StartCoroutine(Extension.DelayRoutine(1f, () => owner.FSM.ChangeState("PhaseChange")));
        }
        else
            // 애니메이션 딜레이 이후 텔레포트 상태로 변경
            takeDamageRoutine = owner.StartCoroutine(Extension.DelayRoutine(1f, () => owner.FSM.ChangeState("Teleport")));
    }

    public override void Exit()
    {
        Time.timeScale = 1f;
        // 대쉬, 그랩이 불가한 Boss레이어로 변경
        owner.gameObject.layer = LayerMask.NameToLayer("Boss");
    }
}

public class PhaseChange : JusticeBaseState
{
    public PhaseChange(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        Debug.Log("PhaseChange");
        owner.BossRoomController.enabled = false;
        owner.EmbientAnim.Play("DisAppear");

        if (!owner.LoadPhaseData()) 
            owner.FSM.ChangeState("LastStanding");
        else
        {
            // 페이즈 변경 시네마틱
            owner.Anim.Play("PhaseChange");
            GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled = false;
            Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 0.5f;
            Manager.Camera.SetCameraPriority(CameraType.CutScene, owner.JusticeCamera);
        }
    }
    public override void Exit()
    {
        owner.BossRoomController.enabled = true;

        owner.EmbientAnim.Play("Appear");

        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled = true;
        Manager.Camera.SetCameraPriority(CameraType.Main);
        Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 2f;
    }
}
public class LastStanding : JusticeBaseState
{
    public LastStanding(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        // 볼륨 세팅, AMB 초기화
        Manager.Sound.UnPlaySound(SoundType.AMB);
        Manager.Sound.PlaySound(SoundType.AMB, "Scene2_Rumble");
        Manager.Sound.AMBSource[0].volume = 0.5f;
        Manager.Sound.BGMSource.volume = 0.3f;

        // 카메라 액션 적용
        Manager.Camera.SetCameraPriority(CameraType.CutScene, owner.JusticeCamera);
        owner.StartCoroutine(Extension.DelayRoutine(2f, () => Manager.Camera.SetCameraPriority(CameraType.Main)));

        // ExecuteTrigger 오브젝트 활성화
        owner.ExecuteTrigger.SetActive(true);

        owner.WeaknessController.DisAppearAll();

        owner.Anim.Play("LastStanding_Appear");
    }
}

public class OnDisable : JusticeBaseState
{
    // 비활성화 상태
    // 플레이어 리스폰 루틴 중

    public OnDisable(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        // 약점 비활성화
        owner.WeaknessController.DisAppearAll();
        // Embient 비활성화 
        owner.EmbientAnim.Play("DisAppear");
        // 정지상태
        owner.Rigid.velocity = Vector3.zero;
        // BGM 정지
        Manager.Sound.BGMSource.Stop();
    }
}