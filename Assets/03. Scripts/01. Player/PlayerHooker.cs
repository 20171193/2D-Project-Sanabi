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
            // �� ���� ������ ����
            // : Idle, Run, RunStop, Jump, Fall
            // ����
            // : IsRaycastHit
            if (!IsRaycastHit) return;       // ����ĳ��Ʈ�� ������ ���
            if (isHookShootDelay) return;    // ������ ��
            if (!Player.PrFSM.IsHookable()) return; // �� ���� �Ұ����� ���� 

            HookShoot();
        }
        else
        {
            // �����׼� Ȥ�� �׷� �� -> ����
            if (Player.PrFSM.FSM.CurState == "Roping" ||
                Player.PrFSM.FSM.CurState == "Grab")
                HookingJump();
            // �� ���� ��� ���� ���콺�� �� ��� 
            else if (isHookShootDelay)
            {
                firedHook?.DisConnecting();
                Player.PrFSM.FSM.ChangeState("Idle");
            }
            // �� �� ����
            else
                return;            
        }
    }

    private void HookingJump()
    {
        Player.Rigid.gravityScale = 1;
        
        // ����� ���� ���� ��� ����
        firedHook?.DisConnecting();
        Player.PrFSM.IsJointed = false;
        // �׷����� ������Ʈ�� ������� �׷�����
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

    // ���� ���� �� �ִ� ���� ���� ���, Invoke OnGrabbedEnemy
    // ���� ����� ����� ���, Invoke OnGrabbedGround
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
        // �� ����
        ReleaseHook();

        // �����Ŀ���ų �� Ȱ��ȭ
        Player.PrSkill.IsEnableRopeForce = true;
        // �����׼� ���� �̺�Ʈ �߻�
        Player.OnRopeForceEnd?.Invoke();
        // ���� ������� ����
        Player.PrFSM.IsJointed = false;
    }
    public void OnHookAttacked(IHookAttackable attackable)
    {
        // ���� �ı�
        attackable.Hitted();
        // ����Ȯ���� ������ ī���� ����
        // ī���� �� ���� ���и��� ����
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
        // Ƣ������� ȿ�� ����
        Player.Rigid.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);

        Player.PrFSM.IsJointed = true;
        Player.PrFSM.ChangeState("Roping");
    }
    public void OnHookHitObject(IGrabable grabed)
    {
        // Ƣ������� ȿ�� ����
        Player.Rigid.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);

        // ī�޶� ��鸲 ȿ�� ����
        Player.DoImpulse();

        Player.PrFSM.IsEnableGrabMove = grabed.IsMoveable();

        Debug.Log(grabed);
        Player.PrSkill.Dash(grabed);
    }
    #endregion

    // �� Ȱ��ȭ ����
    // ��� ������Ʈ�� ����
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

    // �� ��Ȱ��ȭ ����
    // ��� ������Ʈ�� ��������
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
