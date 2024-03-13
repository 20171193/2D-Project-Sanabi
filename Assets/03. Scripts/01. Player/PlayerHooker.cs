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
    public HookAim Aim { get { return hookAim; } }

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
    public Vector3 MousePos { get { return mousePos; } }

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

    [SerializeField]
    private bool isRaycastHit = false;
    public bool IsRaycastHit { get { return isRaycastHit; } set { isRaycastHit = value; } }

    [SerializeField]
    private bool isHookShootDelay = false;
    public bool IsHookShootDelay { get { return isHookShootDelay; } set { isHookShootDelay = value; } }


    protected override void Awake()
    {
        base.Awake();
        cursorOb = Instantiate(cursorOb);
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

        if (!PrFSM.IsCeilingStick && !PrFSM.IsInWall && !PrFSM.IsJointed && !PrFSM.IsGrab && !PrFSM.IsDash)
            RopeRayCast();
        else
            hookAim.LineOff();
    }
    // if Raycast hit is not null, linerendering to hit.point
    private void RopeRayCast()
    {
        Vector2 rayDir = (mousePos - transform.position).normalized;
        hookHitInfo = Physics2D.Raycast(transform.position, rayDir, rayLength, Manager.Layer.hookInteractableLM);

        if (!hookHitInfo || PrFSM.BeDamaged || Manager.Layer.rayBlockObjectLM.Contain(hookHitInfo.collider.gameObject.layer))
        {
            IsRaycastHit = false;
            hookAim.LineOff();
            return;
        }

        IsRaycastHit = true;

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
    // hookshot to mouse position
    private void OnMouseClick(InputValue value)
    {
        if (value.isPressed)
        {
            // 훅 샷이 가능한 상태
            // : Idle, Run, RunStop, Jump, Fall
            // 조건
            // : IsRaycastHit
            if (!IsRaycastHit) return;       // 레이캐스트에 실패한 경우
            if (isHookShootDelay) return;    // 딜레이 중
            if (!PrFSM.IsHookable()) return; // 훅 샷이 불가능한 상태 

            HookShoot();
        }
        else
        {
            // 로프액션 중
            if (PrFSM.FSM.CurState == "Roping")
            {
                playerFSM.IsJointed = false;
                firedHook?.DisConnecting();
                RopeJump();
            }
            // 그랩 중
            else if (playerFSM.FSM.CurState == "Grab")
            {
                playerFSM.IsGrab = false;
                GrabJump();
            }
            // 훅 샷이 닿기 전에 마우스를 뗀 경우 
            else if (isHookShootDelay)
            {
                firedHook?.DisConnecting();
                PrFSM.FSM.ChangeState("Idle");
            }
            // 그 외 상태
            else
                return;            
        }
    }

    private void RopeJump()
    {
        rigid.AddForce(hookAim.transform.up * ropeJumpPower, ForceMode2D.Impulse);
        anim.SetFloat("MovePower", rigid.velocity.magnitude);
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

    // 훅이 잡을 수 있는 적과 닿은 경우, Invoke OnGrabbedEnemy
    // 훅이 지면과 연결된 경우, Invoke OnGrabbedGround
    private void HookShoot()
    {
        playerFSM.OnHookShoot?.Invoke();

        // reload Routine
        hookReloadRoutine = StartCoroutine(HookReloadRoutine());

        hookAim.LineOff();
        PrFSM.FSM.ChangeState("HookShoot");
        ActiveHook();
    }
    #endregion
    #region Hooking Action
    public void OnHookDestroyed()
    {
        // 훅 해제
        ReleaseHook();

        // 로프파워스킬 재 활성화
        PrSkill.IsEnableRopeForce = true;
        // 로프액션 종료 이벤트 발생
        PrFSM.OnRopeForceEnd?.Invoke();
        // 로프 연결상태 해제
        playerFSM.IsJointed = false;
    }
    public void OnHookHitGround()
    {
        playerFSM.IsJointed = true;
        playerFSM.ChangeState("Roping");
    }
    public void OnHookHitObject(IGrabable grabed)
    {
        // 카메라 흔들림 효과 적용
        DoImpulse();

        PrFSM.IsEnableGrabMove = grabed.IsMoveable();

        Debug.Log(grabed);
        playerSkill.Dash(grabed);
    }
    #endregion

    // 훅 활성화 상태
    // 대상 오브젝트와 연결
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

    // 훅 비활성화 상태
    // 대상 오브젝트와 연결해제
    private void ReleaseHook()
    {
        hookObject.SetActive(false);
        hookObject.transform.parent = transform;
        // Transform Initial Setting
        firedHook.transform.position = Vector3.zero;
        firedHook.transform.rotation = Quaternion.identity;

        // Target Initial Setting
        firedHook.muzzlePos = Vector3.zero;
    }

    IEnumerator HookReloadRoutine()
    {
        isHookShootDelay = true;
        yield return new WaitForSeconds(hookShootCoolTime);
        isHookShootDelay = false;
    }
}
