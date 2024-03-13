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

        ghostTrailRoutine = StartCoroutine(GhostTrailRoutine(0.5f, () => PrMover.CurrentMaxRopingPower -= ropeSkillPower));

        // �ܻ� ��ƼŬ ������ �ø�
        if (transform.rotation.y == 0)
            ropeForceParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(0, 0, 0);
        else
            ropeForceParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(1, 0, 0);

        PrFSM.OnRopeForceStart?.Invoke();
        PrMover.CurrentMaxRopingPower += ropeSkillPower;

        // ���� �÷��̾ �ٶ󺸰��ִ� �������� �����Ŀ��� ������
        rigid.AddForce(ropeSkillPower * transform.right, ForceMode2D.Impulse);
    }

    // �뽬 ��ų
    public void Dash(IGrabable grabed)
    {
        // ���� �ӷ°� �߷��� 0���� ����
        rigid.velocity = Vector3.zero;
        rigid.gravityScale = 0;

        Vector3 grabedPos = grabed.GetGrabPosition();
        
        // ���� ������Ʈ�� ��ġ�� �÷��̾� ��ġ�� ����� �÷��̾� ȸ��
        if(transform.position.x < grabedPos.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, -180, 0);

        playerFSM.ChangeState("Dash");

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
        DistanceJoint2D distJoint = PrHooker.FiredHook.DistJoint;

        // �뽬 �� �÷��̾� �������� ����(���̾� ����)
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        // ���ο� ����
        Time.timeScale = 1f;
        // �̺�Ʈ ī�޶� ����
        Manager.Camera.SetEventCamera();
        while (distJoint.distance > 0.1f)
        {
            distJoint.distance -= dashPower*Time.deltaTime;
            Debug.Log(distJoint.distance);
            yield return null;
        }
        // �߻��� �� ��Ȱ��ȭ
        PrHooker.FiredHook.DisConnecting();
        transform.position = grabed.GetGrabPosition();
        // ���� �� ���󺹱�
        Manager.Camera.SetMainCamera();
        gameObject.layer = LayerMask.NameToLayer("Player");
        Time.timeScale = 1f;
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
        target.Grabbed(rigid);

        // ���� ������Ʈ�� Hooker�� �Ҵ�.
        playerHooker.GrabedObject = target;

        playerFSM.ChangeState("Grab");
    }

    // �׷��뽬 ��ų 
    public void CeilingStick()
    {
        PrFSM.IsCeilingStick = true;
        PrFSM.IsGround = false;
        PrFSM.ChangeState("CeilingStickStart");

        ceilingStickRoutine = StartCoroutine(CeilingStickRoutine());
        ghostTrailRoutine = StartCoroutine(GhostTrailRoutine(1f, null));
    }

    IEnumerator CeilingStickRoutine()
    {
        DistanceJoint2D distJoint = PrHooker.FiredHook.DistJoint;

        while (distJoint.distance > 0.1f)
        {
            distJoint.distance -= dashPower * Time.deltaTime;
            Debug.Log(distJoint.distance);
            yield return null;
        }

        transform.position = PrHooker.FiredHook.transform.position;
        // �߻��� �� ��Ȱ��ȭ
        PrHooker.FiredHook.DisConnecting();
        yield return null;
    }

    // �׷��뽬 ��ƾ (Ʈ���ϸ�)
    //IEnumerator CeilingStickRoutine()
    //{
    //    Vector3 startPos = transform.position;
    //    Vector3 endPos = PrHooker.FiredHook.transform.position;

    //    float time = Vector3.Distance(startPos, endPos) / dashPower;
    //    float rate = 0f;

    //    while (rate < 1f)
    //    {
    //        rate += Time.deltaTime / time;
    //        transform.position = Vector3.Lerp(startPos, endPos, rate);
    //        yield return null;
    //    }

    //    transform.position = endPos;

    //    // ������ȯ : CeilingStickStart -> CeilingStickIdle
    //    PrHooker.FiredHook.DisConnecting();
    //    yield return null;
    //}


    // �ܻ� ��ƾ
    IEnumerator GhostTrailRoutine(float activeTime, UnityAction afterAction)
    {
        ropeForceParticle.Play();
        yield return new WaitForSeconds(activeTime);
        afterAction?.Invoke();
        ropeForceParticle.Stop();
    }

}
