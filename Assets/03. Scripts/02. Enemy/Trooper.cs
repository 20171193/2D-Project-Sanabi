using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Trooper : EnemyShooter, IKnockbackable, IGrabable
{
    private CapsuleCollider2D capsuleCol;
    public CapsuleCollider2D CapsuleCol { get { return capsuleCol; } }

    [Header("Components")]
    [SerializeField]
    private RelativeJoint2D rtvJoint;
    public RelativeJoint2D RTVJoint { get { return rtvJoint; } }

    public UnityAction OnTurretDie;

    protected override void Awake()
    {
        base.Awake();

        capsuleCol = GetComponent<CapsuleCollider2D>();
        grabbedYPos = rtvJoint.linearOffset.y;

        // FSM Setting
        TrooperDetect detect = new TrooperDetect(this);
        detect.OnEnableDetect += () => detect.detectRoutine = StartCoroutine(detect.DetectRoutine());
        detect.OnDisableDetect += () => StopCoroutine(detect.detectRoutine);

        fsm.AddState("Detect", detect);
        fsm.AddState("Grabbed", new TrooperGrabbed(this));
        fsm.AddState("Die", new TrooperDie(this));

        fsm.Init("Detect");
        initState = "Detect";
    }

    public override void Detecting(out Vector3 targetPos)
    {
        targetPos = playerTr.transform.position;

        lrAnim.Play("DetectAim");

        lr.positionCount = 2;
        lr.SetPosition(0, muzzlePos.position);
        lr.SetPosition(1, (targetPos - muzzlePos.position).normalized * 100f);

        // Agent Rotation
        if (transform.position.x > targetPos.x)
            transform.rotation = Quaternion.Euler(0, -180f, 0);
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);

        // Aim Rotation
        Vector3 dir = (targetPos - aimPos.position).normalized;
        aimPos.right = dir;
    }
    public override void Shooting()
    {
        anim.Play("Attack");
        shootVFXAnim.Play("Shooting");

        lr.positionCount = 0;

        EnemyBulletObject bullet = Manager.Pool.GetPool(bulletPrefab, muzzlePos.position, aimPos.rotation) as EnemyBulletObject;
        bullet.transform.up = AimPos.right;
        bullet.Rigid.AddForce(aimPos.right * bulletPower, ForceMode2D.Impulse);
    }


    public void Grabbed(Rigidbody2D ownerRigid)
    {
        markerAnim.SetBool("IsEnable", false);

        // RelativeJoint2D Setting
        rtvJoint.enabled = true;
        rtvJoint.connectedBody = ownerRigid;

        lr.positionCount = 0;
        fsm.ChangeState("Grabbed");
    }
    public void GrabEnd()
    {
        // RelativeJoint2D Setting
        rtvJoint.enabled = false;
        rtvJoint.connectedBody = null;

        Died();
    }
    public bool IsMoveable() { return true; }
    public GameObject GetGameObject() { return gameObject; }
    public Vector3 GetGrabPosition() { return new Vector2(this.transform.position.x, this.transform.position.y + grabbedYPos); }

    public void KnockBack(Vector3 force)
    {
        anim.Play("OnHitted");
        rigid.AddForce(force, ForceMode2D.Impulse);
    }
}
