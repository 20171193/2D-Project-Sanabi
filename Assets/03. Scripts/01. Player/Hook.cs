using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Hook : PooledObject
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }
    [SerializeField]
    private DistanceJoint2D distJoint;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private LineRenderer lr;

    // should be set by HookPooler
    [Space(3)]
    [Header("Pooler Setting")]
    [Space(2)]
    [SerializeField]
    public UnityAction<GameObject> OnHookHitEnemy;
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

    [Header("Ballancing")]
    public Vector3 muzzlePos;
    public Vector3 targetPos;

    [SerializeField]
    private bool isConnected = false;

    public float destroyTime;
    private Coroutine trailRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();

        lr.positionCount = 0;
        anim.Play("HookStart");
    }

    private void Update()
    {
        Trailing();

        if (isConnected)
            LineRendering();
    }
    private void Trailing()
    {
        trailRoutine = StartCoroutine(TrailRoutine());
    }

    private void LineRendering()
    {
        lr.positionCount = 2;
        lr.SetPosition(0, targetPos);
        lr.SetPosition(1, ownerRigid.position);
    }
    private void Grab(GameObject target)
    {
        OnHookHitEnemy?.Invoke(target);

        Release();
    }
    private void Conecting()
    {
        isConnected = true;
        OnHookHitGround?.Invoke();

        anim.Play("Grabbing");

        rigid.isKinematic = true;
        rigid.freezeRotation = true;

        distJoint.enabled = true;
        distJoint.connectedBody = ownerRigid;
    }
    public void DisConnecting()
    {
        Release();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Hook Trigger : {collision.name}");
        rigid.velocity = Vector3.zero;

        if (!Manager.Layer.hookInteractableLM.Contain(collision.gameObject.layer))
            Release();

        // enemy hook balancing
        if (Manager.Layer.enemyLM.Contain(collision.gameObject.layer))
        {
            Grab(collision.gameObject);
            return;
        }

        // wall hook
        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer))
            Conecting();
    }

    IEnumerator TrailRoutine()
    {
        float time = Vector3.Distance(muzzlePos, targetPos) / trailSpeed;
        float rate = 0f;

        while(rate < 1f)
        {
            rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(muzzlePos, targetPos, rate);
            yield return null;
        }

        transform.position = targetPos;
        yield return null;
    }

    private void OnDisable()
    {
        // Player Hook Reloading
        OnDestroyHook?.Invoke();
        
        if (trailRoutine != null)
            StopCoroutine(trailRoutine);
    }
    private void OnDestroy()
    {
        Debug.Log("Hook Destroyed");
    }
}
