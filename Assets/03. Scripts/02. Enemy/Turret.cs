using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Turret : EnemyShooter, IGrabable
{
    private BoxCollider2D boxCol;
    public BoxCollider2D BoxCol { get { return boxCol; } }

    public UnityAction<Turret> OnTurretDie;

    protected override void Awake()
    {
        base.Awake();

        boxCol = GetComponent<BoxCollider2D>();


        // FSM Setting
        fsm.AddState("PopUp", new TurretPopUp(this));
        TurretDetect detect = new TurretDetect(this);
        detect.OnEnableDetect += () => detect.detectRoutine = StartCoroutine(detect.DetectRoutine());
        detect.OnDisableDetect += () => StopCoroutine(detect.detectRoutine);

        fsm.AddState("Detect", detect);
        fsm.AddState("Grabbed", new TurretGrabbed(this));
        fsm.AddState("Die", new TurretDie(this));

        fsm.Init("PopUp");
        initState = "PopUp";
    }

    public override void Detecting(out Vector3 targetPos)
    {
        targetPos = playerTr.transform.position;

        lrAnim.Play("DetectAim");

        lr.positionCount = 2;
        lr.SetPosition(0, muzzlePos.position);
        lr.SetPosition(1, muzzlePos.position + (targetPos - muzzlePos.position).normalized * 50f);

        // Aim Rotation
        Vector3 dir = (targetPos - aimPos.position).normalized;
        aimPos.up = dir;
    }
    public override void Shooting()
    {
        anim.Play("Attack");
        shootVFXAnim.SetTrigger("OnShoot");

        lr.positionCount = 0;

        EnemyBulletObject bullet = Manager.Pool.GetPool(bulletPrefab, muzzlePos.position, aimPos.rotation) as EnemyBulletObject;
        bullet.transform.up = aimPos.up;
        bullet.Rigid.AddForce(aimPos.up * bulletPower, ForceMode2D.Impulse);
    }

    protected override void Died()
    {
        fsm.ChangeState("Die");
        StartCoroutine(TurretReleaseRoutine());
    }
    IEnumerator TurretReleaseRoutine()
    {
        yield return new WaitForSeconds(releaseTime);
        Release();
        OnTurretDie?.Invoke(this);
        fsm.ChangeState("Pooled");
    }

    #region IGrabable Interface override
    public void Grabbed(Rigidbody2D ownerRigid)
    {
        markerAnim.SetBool("IsEnable", false);

        lr.positionCount = 0;
        fsm.ChangeState("Grabbed");
    }
    public void GrabEnd()
    {
        Died();
    }
    public bool IsMoveable() { return false; }
    public GameObject GetGameObject() { return gameObject; }
    public Vector3 GetGrabPosition()
    {
        Vector3 returnPos = this.transform.position;
        returnPos += this.transform.up * grabbedYPos;

        return new Vector2(returnPos.x, returnPos.y);
    }
    #endregion
}
