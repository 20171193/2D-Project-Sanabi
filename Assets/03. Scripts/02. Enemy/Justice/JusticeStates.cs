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
    // �ʱ����
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

    // �÷��̾ Ʈ���� ���� ��
    public override void Enter()
    {
        owner.CircleCol.enabled = true;
        owner.Anim.Play("BeforeBattleMode");
    }
    public override void Exit()
    {
        // ���� �� Ȱ��ȭ
        owner.BossRoomController.enabled = true;
    }
}
public class BattleMode : JusticeBaseState
{
    // �ѹ� ������ �޾��� ��
    // ���ݸ�� ���� ����
    public BattleMode(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        // �÷��̾� �Է�����
        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled = false;

        owner.Anim.Play("BattleMode");
        Manager.Camera.SetCameraPriority(CameraType.CutScene, owner.JusticeCamera);
    }
    public override void Exit()
    {
        // �÷��̾� �Է����� ����
        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled = true;

        // ���� �� ����ī ����
        owner.BossRoomController.IsSpawn = true;
        Manager.Camera.SetCameraPriority(CameraType.Main);
    }
}
public class Track : JusticeBaseState
{
    // �����ð����� �÷��̾� Ʈ��ŷ
    // : �����ð��ȿ� �÷��̾ �������� ������ ��� �÷��̾� ��ġ�� �����̵�

    // ���� ����
    // - Ʈ��ŷ -> ����
    // - Ʈ��ŷ -> �ڷ���Ʈ

    private Coroutine trackingRoutine;
    private bool isTracking = true;

    private Coroutine animationRoutine;

    public Track(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        // ���� ǥ��
        if (owner.WeaknessController.IsDisAppear)
            owner.WeaknessController.AppearAll();

        // �ִϸ��̼� ��ȯ �ڷ�ƾ ����
        owner.Anim.Play("MoveStart");
        // MoveStart -> Moving
        animationRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.Anim.Play("Moving")));

        // Ʈ��ŷ �ڷ�ƾ ����
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

        // Ʈ��ŷ �� Ž���� ���� �ǽ�
        Detecting(distToPlayer.magnitude);
    }

    private void Detecting(float distance)
    {
        // Ÿ���� ���ݹ����� �����ִ��� üũ
        if ((owner.CurrentAttackType == JusticeAttackType.Slash && distance <= owner.SlashAttackRange - Justice.Attack_Threshold) ||
            (owner.CurrentAttackType == JusticeAttackType.DashSlash && distance <= owner.DashSlashAttackRange - Justice.Attack_Threshold))
        {
            isTracking = false;

            // ���� ��ȯ �ڷ�ƾ ����
            owner.Anim.Play("MoveEnd");
            // Ʈ��ŷ -> ����
            animationRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.FSM.ChangeState("Charge")));
        }
    }

    IEnumerator TrackingRoutine()
    {
        isTracking = true;
        yield return new WaitForSeconds(owner.TrackingTime);
        isTracking = false;

        // ���� ��ȯ �ڷ�ƾ ����
        owner.Anim.Play("MoveEnd");
        // Ʈ��ŷ -> �ڷ���Ʈ
        owner.CircleCol.enabled = false;
        animationRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.FSM.ChangeState("Teleport")));
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
        if (!owner.WeaknessController.IsDisAppear)
            owner.WeaknessController.DisAppearAll();

        owner.CircleCol.enabled = false;
        owner.EmbientAnim.Play("DisAppear");
        owner.Anim.Play("TeleportStart");
        // �ڷ���Ʈ ������ �ð���ŭ ������ �� ������ȯ
        teleportDelayTimer = owner.StartCoroutine(Extension.DelayRoutine(owner.TeleportTime, () => TeleportEnd()));
    }
    public override void Exit()
    {
        if (teleportDelayTimer != null)
            owner.StopCoroutine(teleportDelayTimer);
    }

    private void TeleportEnd()
    {
        // ���� Ÿ�� ����
        owner.ChangeAttackType(JusticeAttackType.CloakingSlash);
        owner.transform.position = owner.PlayerTr.position;

        // ���� ���� : �ڷ���Ʈ -> ����
        owner.FSM.ChangeState("Charge");
    }
}
public class Charge : JusticeBaseState
{
    // ���ݹ��� ����, vfx ���
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

                // VFX Ȱ��ȭ
                vfx = owner.ChargeVFXPool.ActiveVFX("SlashCharge").GetComponent<JusticeVFX>();
                isAiming = true;
                break;
            case JusticeAttackType.CircleSlash:
                // VFX Ȱ��ȭ
                vfx = owner.ChargeVFXPool.ActiveVFX("CircleSlashCharge").GetComponent<JusticeVFX>();
                isAiming = false;
                break;
            case JusticeAttackType.DashSlash:
                owner.Anim.Play("DashAttackStart");
                // VFX Ȱ��ȭ
                vfx = owner.ChargeVFXPool.ActiveVFX("DashSlashCharge").GetComponent<JusticeVFX>();
                isAiming = true;
                break;
            case JusticeAttackType.CloakingSlash:
                // VFX Ȱ��ȭ
                vfx = owner.ChargeVFXPool.ActiveVFX("CloakingSlashCharge").GetComponent<JusticeVFX>();
                isAiming = false;
                break;
        }

        // vfx �ʱ���ġ ����
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

        // �ڷ���Ʈ ������ ���
        if (owner.CurrentAttackType == JusticeAttackType.CloakingSlash)
        {
            owner.Anim.Play("TeleportEnd");
            yield return new WaitForSeconds(0.3f);
        }


        // ���� ���� : ���� -> ����
        owner.FSM.ChangeState("Attack");
        yield return null;
    }
}
public class Attack : JusticeBaseState
{
    // �� Ÿ�Ժ� ���� ����
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
                // vfx ���
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
                // �뽬 ���� ���� �� slashcount �ʱ�ȭ
                owner.CurSlashCount = 0;
                break;
        }

        // vfx ��ġ, ȸ�� ����
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
        // ���� ���� : ���� -> Ʈ��ŷ
        owner.FSM.ChangeState("Track");
    }
}
public class Counter : JusticeBaseState
{
    // � ���¿����� ���̵� �� �ִ� ����
    // Ʈ��ŷ �� �������� ���� ī���� ��Ȳ���� ������ ���� ��� ��� ī���� ���� ����
    public Counter(Justice owner) { this.owner = owner; }


}

public class Groggy : JusticeBaseState
{
    // � ���¿����� ���̵� �� �ִ� ����
    // ������ ��� �ı��� ��� Groggy ���� ����
    public Groggy(Justice owner) { this.owner = owner; }
    public override void Enter()
    {
        owner.CircleCol.enabled = true;
        // �뽬, �׷��� ������ BossGroggy���̾�� ����
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
    // �������� �޴� ����
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
            // �ִϸ��̼� ������ ���� ������ü���� ���·� ����
            takeDamageRoutine = owner.StartCoroutine(Extension.DelayRoutine(1f, () => owner.FSM.ChangeState("PhaseChange")));
        }
        else
            // �ִϸ��̼� ������ ���� �ڷ���Ʈ ���·� ����
            takeDamageRoutine = owner.StartCoroutine(Extension.DelayRoutine(1f, () => owner.FSM.ChangeState("Teleport")));
    }

    public override void Exit()
    {
        Time.timeScale = 1f;
        // �뽬, �׷��� �Ұ��� Boss���̾�� ����
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
            // ������ ���� �ó׸�ƽ
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
        // ���� ����, AMB �ʱ�ȭ
        Manager.Sound.UnPlaySound(SoundType.AMB);
        Manager.Sound.PlaySound(SoundType.AMB, "Scene2_Rumble");
        Manager.Sound.AMBSource[0].volume = 0.5f;
        Manager.Sound.BGMSource.volume = 0.3f;

        // ī�޶� �׼� ����
        Manager.Camera.SetCameraPriority(CameraType.CutScene, owner.JusticeCamera);
        owner.StartCoroutine(Extension.DelayRoutine(2f, () => Manager.Camera.SetCameraPriority(CameraType.Main)));

        // ExecuteTrigger ������Ʈ Ȱ��ȭ
        owner.ExecuteTrigger.SetActive(true);

        owner.WeaknessController.DisAppearAll();

        owner.Anim.Play("LastStanding_Appear");
    }
}

public class OnDisable : JusticeBaseState
{
    // ��Ȱ��ȭ ����
    // �÷��̾� ������ ��ƾ ��

    public OnDisable(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        // ���� ��Ȱ��ȭ
        owner.WeaknessController.DisAppearAll();
        // Embient ��Ȱ��ȭ 
        owner.EmbientAnim.Play("DisAppear");
        // ��������
        owner.Rigid.velocity = Vector3.zero;
        // BGM ����
        Manager.Sound.BGMSource.Stop();
    }
}