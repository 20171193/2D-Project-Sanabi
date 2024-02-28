using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHooker : PlayerBase
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private GameObject cursorOb;
    [SerializeField]
    private HookAim hookAim;
    [SerializeField]
    private Hook hookPrefab;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private float hookShootPower;
    public float HookShootPower { get { return hookShootPower; } }

    [SerializeField]
    private float hookShootCoolTime;
    public float HookShootCoolTime { get { return hookShootCoolTime; } }
    
    [SerializeField]
    private float ropeLength;  // Raycast distance
    public float RopeLength { get { return ropeLength; } }

    private RaycastHit2D hookHitInfo;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private Vector3 mousePos;

    [SerializeField]
    private Enemy grabEnemy;
    public Enemy GrabEnemy { get { return grabEnemy; } set { grabEnemy = value; } }

    [SerializeField]
    protected Hook firedHook;
    public Hook FiredHook { get { return firedHook; } }

    private Coroutine hookReloadRoutine;

    protected override void Awake()
    {
        base.Awake();
    }

    #region Mouse / Rope Action
    // Raycast to mouse position
    private void OnMousePos(InputValue value)
    {
        // cursorPos is mousePos
        // +Linerendering
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        // move cursor
        cursorOb.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        HookAimSet();

        if (!playerFSM.IsJointed && !playerFSM.IsGrab && !playerFSM.IsDash)
            RopeRayCast();
        else
            hookAim.LineOff();
    }
    // if Raycast hit is not null, linerendering to hit.point
    private void RopeRayCast()
    {
        Vector2 rayDir = (mousePos - transform.position).normalized;
        hookHitInfo = Physics2D.Raycast(transform.position, rayDir, ropeLength, Manager.Layer.hookInteractableLM);

        if (hookHitInfo)
        {
            playerFSM.IsRaycastHit = true;

            // hit is Enemy
            if (Manager.Layer.enemyLM.Contain(hookHitInfo.collider.gameObject.layer))
                hookAim.LineOn(LineRenderType.Enemy, hookHitInfo.point);
            // hit is Ground
            else
                hookAim.LineOn(LineRenderType.Ground, hookHitInfo.point);
        }
        else
        {
            playerFSM.IsRaycastHit = false;
            hookAim.LineOff();
        }

    }
    // hookshot to mouse position
    private void OnMouseClick(InputValue value)
    { 
        if (value.isPressed)
        {
            if(playerFSM.IsHookShoot) return;
            if(playerFSM.IsGrab || playerFSM.IsJointed) return;
            if(!playerFSM.IsRaycastHit) return;

            HookShoot();
        }
        else
        {
            if (playerFSM.IsJointed)
            {
                playerFSM.IsJointed = false;
                firedHook?.DisConnecting();
                RopeJump();
            }
            else if (playerFSM.IsGrab)
            {
                playerFSM.IsGrab = false;
                GrabJump();
            }
            else
                Destroy(firedHook);
        }
    }
    private void RopeJump()
    {
        rigid.AddForce(hookAim.transform.up * 15f, ForceMode2D.Impulse);
        anim.Play("RopeJump");
        HookReloading();
    }
    private void GrabJump()
    {
        rigid.gravityScale = 1;

        grabEnemy.Died();
        rigid.AddForce(hookAim.transform.up * 15f, ForceMode2D.Impulse);
        anim.Play("RopeJump");
        HookReloading();
    }
    #endregion

    #region Hooking
    private void HookAimSet()
    {
        Vector3 dist = mousePos - transform.position;
        float zRot = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;

        hookAim.transform.rotation = Quaternion.Euler(0, 0, zRot - 90f);
        hookAim.transform.position = transform.position + hookAim.transform.up * 1.7f;
    }

    // if hook collide with enemy, Invoke OnGrabbedEnemy
    // else if hook collide with ground, Invoke OnGrabbedGround
    private void HookShoot()
    {
        playerFSM.IsHookShoot = true;
        hookAim.LineOff();
        anim.Play("RopeShot");

        firedHook = Instantiate(hookPrefab, hookAim.transform.position, hookAim.transform.rotation);
        FiredHookInitialSetting(firedHook);

        // Hook shoot
        firedHook.Rigid?.AddForce(hookAim.transform.position.GetDirectionTo2DTarget(hookHitInfo.point) * hookShootPower, ForceMode2D.Impulse);
    }
    private void FiredHookInitialSetting(Hook hook)
    {
        // assign player rigidbody2D for DistanceJoint2D
        hook.OwnerRigid = rigid;

        // hook action setting
        hook.OnDestroyHook += HookReloading;
        hook.OnHookHitEnemy += HookHitEnemy;
        hook.OnHookHitGround += HookHitGround;

        // Convex Collision Detection setting
        // Time = Distance / Velocity
        hook.ccdRoutine = hook.StartCoroutine(hook.CCD(ropeLength / hookShootPower, new Vector3(hookHitInfo.point.x, hookHitInfo.point.y, 0)));
    }
    #endregion
    #region Hooking Action
    private void HookReloading()
    {
        if (hookReloadRoutine != null)
            StopCoroutine(hookReloadRoutine);

        playerFSM.IsHookShoot = false;
    }
    private void HookHitGround()
    {
        StopCoroutine(firedHook.ccdRoutine);

        playerFSM.IsJointed = true;
        playerFSM.ChangeState("Roping");
    }
    private void HookHitEnemy(GameObject target)
    {
        StopCoroutine(firedHook.ccdRoutine);

        playerSkill.Dash(target);
    }
    #endregion

    IEnumerator HookReloadRoutine()
    {
        yield return new WaitForSeconds(hookShootCoolTime);
        playerFSM.IsHookShoot = false;
    }
}
