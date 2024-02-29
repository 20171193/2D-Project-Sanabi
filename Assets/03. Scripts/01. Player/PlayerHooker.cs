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
    private float ropeJumpPower;
    public float RopeJumpPower { get { return ropeJumpPower; } }

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
        Manager.Pool.CreatePool(hookPrefab, 5, 15);
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

        grabEnemy.Died();
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

        firedHook = Manager.Pool.GetPool(hookPrefab, hookAim.transform.position, hookAim.transform.rotation) as Hook;
        firedHook.muzzlePos = hookAim.transform.position;
        firedHook.targetPos = hookHitInfo.point;
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
    public void OnHookHitEnemy(GameObject target)
    {
        playerSkill.Dash(target);
    }
    #endregion

    IEnumerator HookReloadRoutine()
    {
        playerFSM.IsHookShoot = true;
        yield return new WaitForSeconds(hookShootCoolTime);
        playerFSM.IsHookShoot = false;
    }
}
