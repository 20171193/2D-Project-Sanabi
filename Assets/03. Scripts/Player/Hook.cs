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

    [SerializeField]
    private PlayerAction owner;
    public PlayerAction Owner { get { return owner; } set { owner = value; } }

    [SerializeField]
    private Transform hookingPos;
    public Transform HookingPos { get { return hookingPos; } }

    public UnityAction OnJointed;
    public UnityAction OnDisJointed;

    [Header("Ballancing")]
    [SerializeField]
    private float restorationTime;

    public Coroutine ccdRoutine;

    private void OnEnable()
    {
        lr.positionCount = 0;
    }
    private void Start()
    {

    }

    private void Update()
    {
        if (lr.positionCount > 0)
        {
            anim.SetFloat("Velocity", owner.Rigid.velocity.magnitude);
            lr.SetPosition(0, hookingPos.transform.position);
            lr.SetPosition(1, owner.transform.position);
        }
    }

    private void Conecting()
    {
        anim.Play("Grabbing");
        lr.positionCount = 2;

        // LineRenderer Setting
        OnJointed?.Invoke();

        rigid.velocity = Vector3.zero;
        rigid.isKinematic = true;
        rigid.freezeRotation = true;
        distJoint.enabled = true;
        distJoint.connectedBody = owner.Rigid;
        owner.IsJointed = true;
        owner.JointedHook = this;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Manager.Layer.ropeInteractableLM.Contain(collision.gameObject.layer))
            Conecting();
        else
            Destroy(gameObject);
    }

    public IEnumerator CCD(float time, Vector3 limitPosition)
    {
        yield return new WaitForSeconds(time);
        if (!owner.IsJointed)
        {
            transform.position = limitPosition;
            Conecting();
        }
    }

    private void OnDestroy()
    {
        // LineRenderer Setting
        OnDisJointed?.Invoke();
        StopCoroutine(ccdRoutine);
    }
}
