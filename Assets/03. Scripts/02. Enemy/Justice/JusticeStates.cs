using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class JusticeBaseState : BaseState
{
    protected Justice owner;
}

public class Init : JusticeBaseState
{
    // ������ �����ϱ� ���� ����
    private Coroutine initRoutine;
    public Init(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        owner.Anim.Play("Init");

        initRoutine = owner.StartCoroutine(InitRoutine());
    }
    public override void Exit()
    {
        if (initRoutine != null)
            owner.StopCoroutine(initRoutine);
    }

    IEnumerator InitRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        owner.FSM.ChangeState("BattleMode");
    }
}
public class BattleMode : JusticeBaseState
{
    // ���ݸ�� ���� ����
    private Coroutine battleModeTimer;
    public BattleMode(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        owner.Anim.Play("ActiveBattleMode");
        battleModeTimer = owner.StartCoroutine(BattleModeTimer());
    }
    public override void Exit()
    {

        if (battleModeTimer != null)
            owner.StopCoroutine(battleModeTimer);
    }

    IEnumerator BattleModeTimer()
    {
        yield return new WaitForSeconds(1.5f);
        // �������� : ��Ʋ��� -> Ʈ��ŷ
        owner.FSM.ChangeState("Track");
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

    public Track(Justice owner) { this.owner = owner; }

    public override void Enter()
    {
        // ���� ǥ��
        if (owner.WeaknessController.IsDisAppear)
            owner.WeaknessController.AppearAll();


        Debug.Log("Justice Tracking");
        owner.Anim.Play("Moving");
        trackingRoutine = owner.StartCoroutine(TrackingRoutine());
    }

    public override void Update()
    {
        AgentRotation();
        Tracking();
        owner.Anim.SetFloat("MoveSpeed", owner.Rigid.velocity.magnitude);
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
        Detecting(distToPlayer.magnitude);
    }

    private void Detecting(float distance)
    {
        // Ÿ���� ���ݹ����� �����ִ��� üũ
        if ((owner.CurrentAttackType == JusticeAttackType.Slash && distance <= owner.SlashAttackRange - Justice.Attack_Threshold)
            || (owner.CurrentAttackType == JusticeAttackType.DashSlash && distance <= owner.DashSlashAttackRange - Justice.Attack_Threshold))
        { 
            // ���� ���� : Ʈ��ŷ -> ����
            owner.FSM.ChangeState("Charge");
        }
    }

    IEnumerator TrackingRoutine()
    {
        yield return new WaitForSeconds(owner.TrackingTime);
        // ���� ���� : Ʈ��ŷ -> �ڷ���Ʈ
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
        if (!owner.WeaknessController.IsDisAppear)
            owner.WeaknessController.AppearAll();

        Debug.Log("Justice Teleport");

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
                isAiming = false;

                // VFX Ȱ��ȭ
                vfx = owner.ChargeVFXPool.ActiveVFX("CircleSlashCharge").GetComponent<JusticeVFX>();
                break;
            case JusticeAttackType.DashSlash:
                owner.Anim.Play("DashAttackStart");

                // VFX Ȱ��ȭ
                vfx = owner.ChargeVFXPool.ActiveVFX("DashSlashCharge").GetComponent<JusticeVFX>();
                isAiming = true;
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

        // ���� �������� ������ ���
        if (owner.CurrentAttackType == JusticeAttackType.CircleSlash)
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
                owner.Anim.Play("CircleSlashAttack");
                vfx = owner.AgentVFXPool.ActiveVFX("CircleSlash").GetComponent<JusticeVFX>();

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

        // vfx ȸ��
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

}

