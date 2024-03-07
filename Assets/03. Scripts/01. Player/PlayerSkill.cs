using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    protected override void Awake()
    {
        base.Awake();
    }

    public void RopeForce()
    {
        // Shift Key : RopeForceSkill
        if (!isEnableRopeForce) return;

        // ���� ���� ���� �� �����Ŀ���ų�� �ѹ� �� ��밡��.
        // ���� ���� ���ῡ�� �ٽ� ��밡��.
        isEnableRopeForce = true; // PlayerHooker�� OnHookDestroyed ȣ�� �� false�� ����

        // ���� �ִ� �ӵ��� �����Ŀ��ӵ���ŭ ���� ��ų ����� �ִ� �ӵ��� ������Ŵ
        // ���� ��ƾ(�ڷ�ƾ) ���� �� �ʱ�ȭ
        if (ropeForceRoutine != null)
            StopCoroutine(ropeForceRoutine);

        ropeForceRoutine = StartCoroutine(RopeForceRoutine());

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
    IEnumerator RopeForceRoutine()
    {

        ropeForceParticle.Play();
        yield return new WaitForSeconds(0.5f);
        PrMover.CurrentMaxRopingPower -= ropeSkillPower;
        ropeForceParticle.Stop();
    }


    public void Dash(IGrabable grabed)
    {
        // ���� ���� �ƴ� ��ȣ�ۿ� ������ ������Ʈ�� ���� ��� �뽬��ų �ߵ�
        playerFSM.IsDash = true;

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
        
        // ���� ������Ʈ���� Ʈ���ϸ� (���� �̵��� �ƴ� �������� �̵����) 
        dashCoroutine = StartCoroutine(DashTrailRoutine(grabed));
    }
    IEnumerator DashTrailRoutine(IGrabable grabed)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = grabed.GetGrabPosition();

        float time = Vector3.Distance(startPos, endPos) / dashPower;
        float rate = 0f;

        // �뽬 �� �÷��̾� �������� ����(���̾� ����)
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        
        // ���ο� ����
        Time.timeScale = 0.5f;
        while(rate < 1f)
        {
            // ī�޶� ���� ����
            if (rate >= 0.6f && rate <= 0.7f)
                Manager.Camera.SetEventCamera();

            rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(startPos, endPos, rate);
            yield return null;
        }

        // ī�޶� ���� ���󺹱�
        Manager.Camera.SetMainCamera();

        gameObject.layer = LayerMask.NameToLayer("Player");
        transform.position = endPos;
        Time.timeScale = 1f;
        
        // ��ǥ������ ������ �� ���� ������Ʈ�� �׷�
        Grab(grabed);
        yield return null;
    }
    public void Grab(IGrabable target)
    {
        // �뽬���� ��ȯ�� �׷�
        playerFSM.IsDash = false;

        target.Grabbed(rigid);

        // ���� ������Ʈ�� Hooker�� �Ҵ�.
        playerHooker.GrabedObject = target;
        playerFSM.IsGrab = true;

        playerFSM.ChangeState("Grab");
    }
}
