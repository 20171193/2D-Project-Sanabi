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
    [SerializeField]
    private GameObject hud_ArmJammed;

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
    private float hookAttackFailedTime;
    public float HookAttackFailedTime { get { return hookAttackFailedTime; } }

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
    private Coroutine hookAttackFailedRoutine;

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
        firedHook.OwnerRigid = Player.Rigid;
        firedHook.TrailSpeed = hookShootPower;
        firedHook.MaxDistance = maxRopeLength;

        // hook action setting
        firedHook.OnDestroyHook += OnHookDestroyed;
        firedHook.OnHookHitObject += OnHookHitObject;
        firedHook.OnHookHitGround += OnHookHitGround;
        firedHook.OnHookAttacked += OnHookAttacked;
        firedHook.OnHookAttackFailed += OnHookAttackFailed;
    }


    #region Mouse / Rope Action
    // Raycast to mouse position
    private void OnMousePos(InputValue value)
    {
        // cursorPos is mousePos
        // +Linerendering
        mousePos = Player.Cam.ScreenToWorldPoint(Input.mousePosition);
        // move cursor
        cursorOb.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        HookAimSet();

        if (Player.PrFSM.IsHookable())
            RopeRayCast();
        else
            hookAim.LineOff();
    }
    // if Raycast hit is not null, linerendering to hit.point
    private void RopeRayCast()
    {
        Vector2 rayDir = (mousePos - transform.position).normalized;
        hookHitInfo = Physics2D.Raycast(transform.position, rayDir, rayLength, Manager.Layer.hookInteractableLM);

        if (!hookHitInfo || Player.PrFSM.FSM.CurState == "Damaged" || Manager.Layer.rayBlockObjectLM.Contain(hookHitInfo.collider.gameObject.layer))
        {
            IsRaycastHit = false;
            hookAim.LineOff();
            return;
        }

        IsRaycastHit = true;

        int layer = hookHitInfo.collider.gameObject.layer;
        // hit is Enemy
        if (Manager.Layer.enemyLM.Contain(layer) || Manager.Layer.bossWeaknessLM.Contain(layer))
        {
            hitType = LineRenderType.Enemy;
            hookAim.LineOn(hitType, hookHitInfo.point);
        }
        // hit is Interactable Object
        else if (Manager.Layer.hookingPlatformLM.Contain(layer))
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
            if (!Player.PrFSM.IsHookable()) return; // 훅 샷이 불가능한 상태 

            HookShoot();
        }
        else
        {
            // 로프액션 혹은 그랩 중 -> 점프
            if (Player.PrFSM.FSM.CurState == "Roping" ||
                Player.PrFSM.FSM.CurState == "Grab")
                HookingJump();
            // 훅 샷이 닿기 전에 마우스를 뗀 경우 
            else if (isHookShootDelay)
            {
                firedHook?.DisConnecting();
                Player.PrFSM.FSM.ChangeState("Idle");
            }
            // 그 외 상태
            else
                return;            
        }
    }

    private void HookingJump()
    {
        Player.Rigid.gravityScale = 1;
        
        // 연결된 훅이 있을 경우 해제
        firedHook?.DisConnecting();
        Player.PrFSM.IsJointed = false;
        // 그랩중인 오브젝트가 있을경우 그랩해제
        grabedObject?.GrabEnd();

        Player.PrFSM.FSM.ChangeState("HookingJump");
        Player.Rigid.AddForce(hookAim.transform.up * ropeJumpPower, ForceMode2D.Impulse);
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
        Player.OnHookShoot?.Invoke();

        // reload Routine
        hookReloadRoutine = StartCoroutine(HookReloadRoutine());
        hookAim.LineOff();
        Player.PrFSM.FSM.ChangeState("HookShoot");

        ActiveHook();
    }
    #endregion
    #region Hooking Action
    public void OnHookDestroyed()
    {
        // 훅 해제
        ReleaseHook();

        // 로프파워스킬 재 활성화
        Player.PrSkill.IsEnableRopeForce = true;
        // 로프액션 종료 이벤트 발생
        Player.OnRopeForceEnd?.Invoke();
        // 로프 연결상태 해제
        Player.PrFSM.IsJointed = false;
    }
    public void OnHookAttacked(IHookAttackable attackable)
    {
        // 약점 파괴
        attackable.Hitted();
        // 일정확률로 보스는 카운터 실행
        // 카운터 시 공격 실패모드로 변경
    }

    public void OnHookAttackFailed()
    {
        if (hookReloadRoutine != null)
            StopCoroutine(hookReloadRoutine);
        firedHook?.DisConnecting();
        hookAttackFailedRoutine = StartCoroutine(HookAttackFailedRoutine());
    }

    public void OnHookHitGround()
    {
        // 튀어오르는 효과 적용
        Player.Rigid.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);

        Player.PrFSM.IsJointed = true;
        Player.PrFSM.ChangeState("Roping");
    }
    public void OnHookHitObject(IGrabable grabed)
    {
        // 튀어오르는 효과 적용
        Player.Rigid.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);

        // 카메라 흔들림 효과 적용
        Player.DoImpulse();

        Player.PrFSM.IsEnableGrabMove = grabed.IsMoveable();

        Debug.Log(grabed);
        Player.PrSkill.Dash(grabed);
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

    IEnumerator HookAttackFailedRoutine()
    {
        isHookShootDelay = true;
        hud_ArmJammed.SetActive(true);
        yield return new WaitForSeconds(hookAttackFailedTime);
        isHookShootDelay = false;
        hud_ArmJammed.SetActive(false);
    }
    IEnumerator HookReloadRoutine()
    {
        isHookShootDelay = true;
        yield return new WaitForSeconds(hookShootCoolTime);
        isHookShootDelay = false;
    }
}
