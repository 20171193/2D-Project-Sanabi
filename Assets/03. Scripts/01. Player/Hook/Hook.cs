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
        // 연결된 상태에서 라인렌더링 : 플레이어와 훅 사이 체인 렌더링
        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, ownerRigid.position);
    }


    // 오브젝트 그랩
    private void Grab(IGrabable grabed)
    {
        Connecting(true);

        // 액션에 등록된 PlayerHooker의 함수 실행
        OnHookHitObject?.Invoke(grabed);

        // 그랩한 경우 그랩된 오브젝트를 따라 훅의 위치를 변경해야함.
        grabedTr = grabed.GetGameObject().transform;

        hookHittedRoutine = StartCoroutine(HookHittedRoutine());

        GameObject grabedOb = grabed.GetGameObject();
        IKnockbackable knockbacked = grabedOb.GetComponent<IKnockbackable>();
        knockbacked?.KnockBack((grabedOb.transform.position - muzzlePos).normalized * knockBackPower);

    }

    // 지면 그립
    private void Grip()
    {
        Connecting(false);
        // 상대 조인트의 거리를 1만큼 줄여 플레이어를 살짝 띄우는 효과
        distJoint.distance -= 1f;

        // 액션에 등록된 PlayerHooker 함수 실행
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

        // 그랩의 경우엔 거리를 더 짧게 설정
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

        // 훅 vfx 출력
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
