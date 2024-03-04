using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingHook : Platform, IKnockbackable, IGrabMoveable
{
    [Header("Components")]
    [SerializeField]
    private Transform jointTr;
    [SerializeField]
    private Transform hookTr;

    [Header("Balancing")]
    [SerializeField]
    private Rigidbody2D rigid;

    private void OnEnable()
    {
        lr.positionCount = 2;
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
    }
    public void Grabbed()
    {
    }
    public void GrabEnd()
    {

    }
    public GameObject GetObject() { return this.gameObject; }
    public Vector3 GetGrabPosition() { return new Vector2(this.transform.position.x, this.transform.position.y); }
    public void GrabMove(Rigidbody2D ownerRigid)
    {
        rigid.velocity = ownerRigid.velocity;
    }

}
