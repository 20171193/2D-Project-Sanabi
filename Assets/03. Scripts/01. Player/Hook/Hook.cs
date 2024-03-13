using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Hook : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }
    [SerializeField]
    private DistanceJoint2D distJoint;
    public DistanceJoint2D DistJoint { get { return distJoint; } }
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private LineRenderer lr;
    [SerializeField]
    private GameObject hookHittedVFX;


    // should be set by HookPooler
    [Space(3)]
    [Header("Pooler Setting")]
    [Space(2)]
    [SerializeField]
    public UnityAction<IGrabable> OnHookHitObject;
    [SerializeField]
    public UnityAction OnHookHitGround;
    [SerializeField]
    public UnityAction OnDestroyHook;

    [SerializeField]
    private Rigidbody2D ownerRigid;
    public Rigidbody2D OwnerRigid { set { ownerRigid = value; } }

    [SerializeField]
    private float trailSpeed;
    public float TrailSpeed { get { return trailSpeed; } set { trailSpeed = value; } }

    [SerializeField]
    private float maxDistance;
    public float MaxDistance { get { return maxDistance; } set { maxDistance = value; } }

    [Header("Specs")]
    [SerializeField]
    private float knockBackPower;
    public float KnockBackPower { get { return knockBackPower; }  }

    [Header("Ballancing")]
    public Vector3 muzzlePos;
    public RaycastHit2D hitInfo;

    [SerializeField]
    private bool isConnected = false;
    private Rigidbody2D connectedRigid;
    [SerializeField]
    private Transform grabedTr = null; 

    public float destroyTime;
    private Coroutine trailRoutine;
    private Coroutine hookHittedRoutine;

    private void OnEnable()
    {
        lr.positionCount = 0;
        anim.Play("HookStart");
        trailRoutine = StartCoroutine(TrailRoutine());
    }

    private void Update()
    {
        if (isConnected)
            LineRendering();
        if(grabedTr!= null)
            transform.position = grabedTr.position;
    }

    private void LineRendering()
    {
        // ����� ���¿��� ���η����� : �÷��̾�� �� ���� ü�� ������
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, ownerRigid.position);
    }


    // ������Ʈ �׷�
    private void Grab(IGrabable grabed)
    {
        Connecting(true);

        // �׼ǿ� ��ϵ� PlayerHooker�� �Լ� ����
        OnHookHitObject?.Invoke(grabed);

        // �׷��� ��� �׷��� ������Ʈ�� ���� ���� ��ġ�� �����ؾ���.
        grabedTr = grabed.GetGameObject().transform;

        hookHittedRoutine = StartCoroutine(HookHittedRoutine());

        GameObject grabedOb = grabed.GetGameObject();
        IKnockbackable knockbacked = grabedOb.GetComponent<IKnockbackable>();
        knockbacked?.KnockBack((grabedOb.transform.position - muzzlePos).normalized * knockBackPower);

    }

    // ���� �׸�
    private void Grip()
    {
        Connecting(false);
        // ��� ����Ʈ�� �Ÿ��� 1��ŭ �ٿ� �÷��̾ ��¦ ���� ȿ��
        distJoint.distance -= 1f;

        // �׼ǿ� ��ϵ� PlayerHooker �Լ� ����
        OnHookHitGround?.Invoke();
    }

    private void Connecting(bool isGrab)
    {
        isConnected = true;
        //OnHookHitGround?.Invoke();

        anim.Play("Grabbing");

        rigid.isKinematic = true;
        rigid.freezeRotation = true;

        float distance = (ownerRigid.transform.position - transform.position).magnitude;
        distJoint.distance = distance > maxDistance ? maxDistance : distance;

        // �׷��� ��쿣 �Ÿ��� �� ª�� ����
        //if (isGrab) distance /= 2f;

        distJoint.enabled = true;
        distJoint.connectedBody = ownerRigid;
    }
    public void DisConnecting()
    {
        distJoint.enabled = false;
        rigid.isKinematic = false;
        rigid.freezeRotation = false;
        isConnected = false;
        grabedTr = null;

        OnDestroyHook?.Invoke();
    }

    IEnumerator TrailRoutine()
    {
        float time = Vector3.Distance(muzzlePos, hitInfo.point) / trailSpeed;
        float rate = 0f;

        while(rate < 1f)
        {
            rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(muzzlePos, hitInfo.point, rate);
            yield return null;
        }

        transform.position = hitInfo.point;

        // �� vfx ���
        hookHittedRoutine = StartCoroutine(HookHittedRoutine());

        if (Manager.Layer.hookingGroundLM.Contain(hitInfo.collider.gameObject.layer))
        {
            transform.parent = hitInfo.collider.gameObject.transform;
            Grip();
        }
        else
            Grab(hitInfo.collider.gameObject.GetComponent<IGrabable>());

        yield return null;
    }
    IEnumerator HookHittedRoutine()
    {
        hookHittedVFX.transform.parent = null;
        hookHittedVFX.transform.position = transform.position;
        hookHittedVFX.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        hookHittedVFX.SetActive(false);
        hookHittedVFX.transform.parent = ownerRigid.transform;
    }

    private void OnDisable()
    {
        if (trailRoutine != null)
            StopCoroutine(trailRoutine);
        if (hookHittedRoutine != null)
            StopCoroutine(hookHittedRoutine);
        hookHittedVFX.SetActive(false);
        hookHittedVFX.transform.parent = ownerRigid.transform;
    }
    private void OnDestroy()
    {
        Debug.Log("Hook Destroyed");
    }
}
