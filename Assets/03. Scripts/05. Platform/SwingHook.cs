using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingHook : Platform, IKnockbackable, IGrabable
{
    [Header("Components")]
    [SerializeField]
    private Transform jointTr;
    [SerializeField]
    private Transform hookTr;
    [SerializeField]
    private RelativeJoint2D rtvJoint;
    [SerializeField]
    private Rigidbody2D rigid;
    [SerializeField]
    private Animator anim;

    [Header("Specs")]
    private float grabbedYPos;

    private void OnEnable()
    {
        lr.positionCount = 2;
        grabbedYPos = rtvJoint.linearOffset.y;
    }

    private void Update()
    {
        LineRendering();
    }

    public override void LineRendering()
    {
        lr.SetPosition(0, jointTr.position);
        lr.SetPosition(1, hookTr.position);
    }

    public void KnockBack(Vector3 force)
    {
        anim.SetTrigger("OnGrabbed");
        rigid.AddForce(force, ForceMode2D.Impulse);
    }
    public void Grabbed(Rigidbody2D ownerRigid)
    {
        // RelativeJoint2D Setting
        rtvJoint.enabled = true;
        rtvJoint.connectedBody = ownerRigid;
    }
    public void GrabEnd()
    {
        // RelativeJoint2D Setting
        rtvJoint.enabled = false;
        rtvJoint.connectedBody = null;
    }

    public Vector3 GetGrabPosition()
    {
        return new Vector2(hookTr.position.x, hookTr.position.y + grabbedYPos);
    }
}
