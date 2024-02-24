using System.Buffers;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private Animator anim;

    [SerializeField]
    private LineRenderer lr;

    [Space(3)]
    [Header("Hook Action")]
    [Space(2)]
    public UnityAction<GameObject> OnGrabbedEnemy;
    public UnityAction<Vector3> OnGrabbedGround;
    public UnityAction OnDestroyHook;

    [Header("Ballancing")]
    [SerializeField]
    private Rigidbody2D ownerRigid;
    public Rigidbody2D OwnerRigid { set { ownerRigid = value; } }

    [SerializeField]
    private bool isConnected = false;
    [SerializeField]
    private bool isGrabbed = false;

    public float destroyTime;

    public Coroutine ccdRoutine;
    private Coroutine destroyRoutine;

    private void OnEnable()
    {
        lr.positionCount = 0;
    }
    private void Start()
    {

    }

    private void Update()
    {
        if (isConnected)
            LineRendering();
    }

    private void LineRendering()
    {

    }

    private void Grab(GameObject target)
    {
        isGrabbed = true;
        OnGrabbedEnemy?.Invoke(target);

        Destroy(gameObject);
    }

    private void Conecting(Vector3 pos)
    {
        isConnected = true;
        OnGrabbedGround?.Invoke(pos);

        anim.Play("Grabbing");
        lr.positionCount = 2;

        rigid.velocity = Vector3.zero;
        rigid.isKinematic = true;
        rigid.freezeRotation = true;
        distJoint.enabled = true;
        distJoint.connectedBody = ownerRigid;
    }

    public void DisConnecting()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StopCoroutine(ccdRoutine);
        StopCoroutine(destroyRoutine);

        if (Manager.Layer.hookInteractableLM.Contain(collision.gameObject.layer))
        {
            if (Manager.Layer.enemyLM.Contain(collision.gameObject.layer))
                Grab(collision.gameObject);
            else
                Conecting(transform.position);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Convex Collision Detection
    public IEnumerator CCD(float time, Vector3 limitPosition)
    {
        yield return new WaitForSeconds(time);
        if (!isConnected && !isGrabbed)
            transform.position = limitPosition;
        destroyRoutine = StartCoroutine(DestroyRouine());
    }

    private IEnumerator DestroyRouine()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }


    private void OnDestroy()
    {
        // Player Hook Reloading
        OnDestroyHook?.Invoke();

        StopCoroutine(ccdRoutine);
        StopCoroutine(destroyRoutine);
    }
}
