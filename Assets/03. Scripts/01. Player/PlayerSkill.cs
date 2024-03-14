using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSkill : PlayerBase
{
    [Header("Component")]
    [SerializeField]
    private ParticleSystem ropeForceParticle;

    [Header("Specs")]
    [SerializeField]
    private float ropeSkillPower;
    public float RopeSkillPower { get { return ropeSkillPower; } }

    [Header("Ballaincing")]
    private bool isEnableRopeForce = true;
    public bool IsEnableRopeForce { get { return isEnableRopeForce; } set { isEnableRopeForce = value; } }

    [SerializeField]
    private float dashPower;
    public float DashPower { get { return dashPower; } }

    [SerializeField]
    private float slowMotionTime;

    private Coroutine dashCoroutine;
    private Coroutine ropeForceRoutine;
    private Coroutine ghostTrailRoutine;    // �ܻ�ȿ��
    private Coroutine ceilingStickRoutine;

    protected override void Awake()
    {
        base.Awake();
    }
    
    // �����Ŀ� ��ų
    public void RopeForce()
    {
        // Shift Key : RopeForceSkill
        if (!isEnableRopeForce) return;

        // ���� ���� ���� �� �����Ŀ���ų�� �ѹ� �� ��밡��.
        // ���� ���� ���ῡ�� �ٽ� ��밡��.
        isEnableRopeForce = false; // PlayerHooker�� OnHookDestroyed ȣ�� �� true

        // ���� �ִ� �ӵ��� �����Ŀ��ӵ���ŭ ���� ��ų ����� �ִ� �ӵ��� ������Ŵ
        // ���� ��ƾ(�ڷ�ƾ) ���� �� �ʱ�ȭ
        if (ghostTrailRoutine != null)
            StopCoroutine(ghostTrailRoutine);

        // �ܻ� ��ƼŬ ���
        ghostTrailRoutine = StartCoroutine(GhostTrailRoutine(0.5f, () => Player.PrMover.CurrentMaxRopingPower -= ropeSkillPower));

        // �ܻ� ��ƼŬ ������ �ø�
        if (transform.rotation.y == 0)
            ropeForceParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(0, 0, 0);
        else
            ropeForceParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(1, 0, 0);

        Player.OnRopeForceStart?.Invoke();
        Player.PrMover.CurrentMaxRopingPower += ropeSkillPower;

        // ���� �÷��̾ �ٶ󺸰��ִ� �������� �����Ŀ��� ������
        Player.Rigid.AddForce(ropeSkillPower * transform.right, ForceMode2D.Impulse);
    }

    // �뽬 ��ų
    public void Dash(IGrabable grabed)
    {
        // ���� �ӷ°� �߷��� 0���� ����
        Player.Rigid.velocity = Vector3.zero;
        Player.Rigid.gravityScale = 0;

        Vector3 grabedPos = grabed.GetGrabPosition();

        // �÷��̾� ��� vfx ����
        GameObject vfx = Player.PrVFX.GetVFX("PlayerDash");
        Vector3 vec = grabedPos - transform.position;
        vfx.transform.right = vec.normalized;
        // ���� �Ÿ� 1/4 ������ vfx ��ġ ����
        vfx.transform.position = transform.position + vec.normalized * 2f;


        // ���� ������Ʈ�� ��ġ�� �÷��̾� ��ġ�� ����� �÷��̾� ȸ��
        if (transform.position.x < grabedPos.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, -180, 0);

        Player.PrFSM.ChangeState("Dash");

        // ���� ������Ʈ���� Ʈ���ϸ�(���� �̵��� �ƴ� �������� �̵����)
        //dashCoroutine = StartCoroutine(DashTrailRoutine(grabed));


        // �� DistanceJoint2D�� distance�� ���̸� �̵��ϴ� ���
        //Vector3 dir = (grabedPos - transform.position).normalized;
        //rigid.AddForce(dir * 50f, ForceMode2D.Impulse);
        dashCoroutine = StartCoroutine(DashTrail(grabed));
    }
    // fix
    IEnumerator DashTrail(IGrabable grabed)
    {
        DistanceJoint2D distJoint = Player.PrHooker.FiredHook.DistJoint;

        // �뽬 �� �÷��̾� �������� ����(���̾� ����)
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        while (distJoint.distance > 0.1f)
        {
            distJoint.distance -= dashPower*Time.deltaTime;
            yield return null;
        }
        // �߻��� �� ��Ȱ��ȭ
        Player.PrHooker.FiredHook.DisConnecting();
        transform.position = grabed.GetGrabPosition();
        gameObject.layer = LayerMask.NameToLayer("Player");
        Debug.Log("object Grabstart");
        Grab(grabed);
        yield return null;
    }
    //// �뽬 Ʈ���ϸ�
    //IEnumerator DashTrailRoutine(IGrabable grabed)
    //{
    //    Vector3 startPos = transform.position;
    //    Vector3 endPos = grabed.GetGrabPosition();

    //    float time = Vector3.Distance(startPos, endPos) / dashPower;
    //    float rate = 0f;

    //    // �뽬 �� �÷��̾� �������� ����(���̾� ����)
    //    gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");

    //    // ���ο� ����
    //    Time.timeScale = 0.5f;
    //    while (rate < 1f)
    //    {
    //        // ī�޶� ���� ����
    //        if (rate >= 0.6f && rate <= 0.7f)
    //            Manager.Camera.SetEventCamera();

    //        rate += Time.deltaTime / time;
    //        transform.position = Vector3.Lerp(startPos, endPos, rate);
    //        yield return null;
    //    }

    //    // ī�޶� ���� ���󺹱�
    //    Manager.Camera.SetMainCamera();

    //    gameObject.layer = LayerMask.NameToLayer("Player");
    //    transform.position = endPos;
    //    Time.timeScale = 1f;

    //    // ��ǥ������ ������ �� ���� ������Ʈ�� �׷�
    //    Grab(grabed);
    //    yield return null;
    //}

    // �׷� ��ų
    public void Grab(IGrabable target)
    {
        target.Grabbed(Player.Rigid);

        // ���� ������Ʈ�� Hooker�� �Ҵ�.
        Player.PrHooker.GrabedObject = target;
        Debug.Log(target);
        Player.PrFSM.ChangeState("Grab");
    }

    // �׷��뽬 ��ų 
    public void CeilingStick()
    {
        Player.PrFSM.IsCeilingStick = true;
        Player.PrFSM.IsGround = false;
        Player.PrFSM.ChangeState("CeilingStickStart");

        // ���� ������Ʈ�� ��ġ�� �÷��̾� ��ġ�� ����� �÷��̾� ȸ��
        if (transform.position.x < Player.PrHooker.FiredHook.transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, -180, 0);

        ghostTrailRoutine = StartCoroutine(GhostTrailRoutine(1f, null));
        Player.PrHooker.FiredHook.DistJoint.distance = 0.8f;
        // ���� ���̴� PlayerFSM�� OnTriggerEnter2D���� ����
        // CeilingCheck�� �浹 �� -> CeilingStickIdle�� ��ȯ
    }

    // �ܻ� ��ƾ
    IEnumerator GhostTrailRoutine(float activeTime, UnityAction afterAction)
    {
        ropeForceParticle.Play();
        yield return new WaitForSeconds(activeTime);
        afterAction?.Invoke();
        ropeForceParticle.Stop();
    }

}
