using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerHooker : PlayerBase
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private GameObject cursorOb;
    [SerializeField]
    private HookAim hookAim;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private float ropeJumpPower;
    public float RopeJumpPower { get { return ropeJumpPower; } }

    [SerializeField]
    private float hookShootPower;
    public float HookShootPower { get { return hookShootPower; } }

    [SerializeField]
    private float hookShootCoolTime;
    public float HookShootCoolTime { get { return hookShootCoolTime; } }
    
    [SerializeField]
    private float rayLength;  // Raycast distance
    public float RayLength { get { return RayLength; } }

    [SerializeField]
    private float maxRopeLength;
    public float MaxRopeLength { get { return maxRopeLength; } }

    private RaycastHit2D hookHitInfo;
    private LineRenderType hitType;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private Vector3 mousePos;

    [SerializeField]
    private IGrabable grabedObject;
    public IGrabable GrabedObject { get { return grabedObject; } set { grabedObject = value; } }

    [SerializeField]
    protected GameObject hookObject;
    public GameObject HookObject { get { return hookObject; } }

    [SerializeField]
    protected Hook firedHook;
    public Hook FiredHook { get { return firedHook; } set { firedHook = value; } }

    private Coroutine hookReloadRoutine;

    protected override void Awake()
    {
        base.Awake();
        HookInitailSetting();
    }

    private void HookInitailSetting()
    {
        // assign player rigidbody2D for DistanceJoint2D
        firedHook.OwnerRigid = rigid;
        firedHook.TrailSpeed = hookShootPower;
        firedHook.MaxDistance = maxRopeLength;

        // hook action setting
        firedHook.OnDestroyHook += OnHookDestroyed;
        firedHook.OnHookHitObject += OnHookHitObject;
        firedHook.OnHookHitGround += OnHookHitGround;
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
        hookHitInfo = Physics2D.Raycast(transform.position, rayDir, rayLength, Manager.Layer.hookInteractableLM);

        if (hookHitInfo && !PrFSM.BeDamaged)
        {
            playerFSM.IsRaycastHit = true;

            // hit is Enemy
            if (Manager.Layer.enemyLM.Contain(hookHitInfo.collider.gameObject.layer))
            {
                hitType = LineRenderType.Enemy;
                hookAim.LineOn(hitType, hookHitInfo.point);
            }
            // hit is Interactable Object
            else if (Manager.Layer.hookingPlatformLM.Contain(hookHitInfo.collider.gameObject.layer))
            {
                hitType = LineRenderType.Interactable;
                hookAim.LineOn(hitType, hookHitInfo.point);
            }
            // hit is Ground
            else
            {
                hitType = LineRenderType.Ground;
                hookAim.LineOn(hitType, hookHitInfo.point);
            }
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
            if (playerFSM.BeDamaged) return;
            if (playerFSM.IsInWall) return;
            if (playerFSM.IsHookShoot) return;
            if (playerFSM.IsGrab || playerFSM.IsJointed) return;
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
            {
                anim.Play("Idle");
                firedHook?.DisConnecting();
            }
        }
    }

    private void RopeJump()
    {
        rigid.AddForce(hookAim.transform.up * ropeJumpPower, ForceMode2D.Impulse);
        anim.Play("RopeJump");
    }
    private void GrabJump()
    {
        rigid.gravityScale = 1;

        grabedObject.GrabEnd();

        rigid.AddForce(hookAim.transform.up * 15f, ForceMode2D.Impulse);
        anim.Play("RopeJump");
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
        if (playerFSM.IsHookShoot) return;

        // reload Routine
        StartCoroutine(HookReloadRoutine());

        hookAim.LineOff();
        anim.Play("RopeShot");

        ActiveHook();
    }
    #endregion
    #region Hooking Action
    public void OnHookDestroyed()
    {
        ReleaseHook();
        playerFSM.IsJointed = false;
    }
    public void OnHookHitGround()
    {
        playerFSM.IsJointed = true;
        playerFSM.ChangeState("Roping");
    }
    public void OnHookHitObject(IGrabable grabed)
    {
        DoImpulse();

        Debug.Log(grabed);
        playerSkill.Dash(grabed);
    }
    #endregion

    private void ActiveHook()
    {
        // Transform Setting
        hookObject.transform.position = hookAim.transform.position;
        hookObject.transform.up = hookAim.transform.up;

        // Target Setting
        firedHook.muzzlePos = hookAim.transform.position;
        firedHook.hitInfo = hookHitInfo;

        hookObject.SetActive(true);
    }
    private void ReleaseHook()
    {
        hookObject.SetActive(false);

        // Transform Initial Setting
        firedHook.transform.position = Vector3.zero;
        firedHook.transform.rotation = Quaternion.identity;

        // Target Initial Setting
        firedHook.muzzlePos = Vector3.zero;
    }

    IEnumerator HookReloadRoutine()
    {
        playerFSM.IsHookShoot = true;
        yield return new WaitForSeconds(hookShootCoolTime);
        playerFSM.IsHookShoot = false;
    }
}
