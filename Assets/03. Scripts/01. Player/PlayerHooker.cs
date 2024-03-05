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
    protected Hook hook;
    public Hook FiredHook { get { return hook; } }

    private Coroutine hookReloadRoutine;

    protected override void Awake()
    {
        base.Awake();
    }

    private void HookInitialSetting()
    {
        // assign player rigidbody2D for DistanceJoint2D
        hook.OwnerRigid = rigid;
        hook.TrailSpeed = hookShootPower;
        hook.MaxDistance = maxRopeLength;

        // hook action setting
        hook.OnDestroyHook += OnHookDisJointed;
        hook.OnHookHitObject += OnHookHitObject;
        hook.OnHookHitGround += OnHookHitGround;
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

        if (hookHitInfo)
        {
            playerFSM.IsRaycastHit = true;

            // hit is Enemy
            if (Manager.Layer.enemyLM.Contain(hookHitInfo.collider.gameObject.layer))
            {
                hitType = LineRenderType.Enemy;
                hookAim.LineOn(hitType, hookHitInfo.point);
            }
            // hit is Interactable Object
            else if (Manager.Layer.enemyLM.Contain(hookHitInfo.collider.gameObject.layer))
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
                hook?.DisConnecting();
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
                hook?.DisConnecting();
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
        // reload Routine
        StartCoroutine(HookReloadRoutine());

        hookAim.LineOff();
        anim.Play("RopeShot");

        // FiredHook Setting
        hook.muzzlePos = hookAim.transform.position;
        hook.hitInfo = hookHitInfo;
    }
    #endregion
    #region Hooking Action
    public void OnHookDisJointed()
    {
        playerFSM.IsJointed = false;
    }
    public void OnHookHitGround()
    {
        playerFSM.IsJointed = true;
        playerFSM.ChangeState("Roping");
    }
    public void OnHookHitObject(IGrabable grabed)
    {
        playerSkill.Dash(grabed);
    }
    #endregion

    IEnumerator HookReloadRoutine()
    {
        playerFSM.IsHookShoot = true;
        yield return new WaitForSeconds(hookShootCoolTime);
        playerFSM.IsHookShoot = false;
    }
}
