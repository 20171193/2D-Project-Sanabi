using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private PlayerAction prAction;

    [SerializeField]
    private GameObject cursorOb;

    [SerializeField]
    private GameObject hookPrefab;
    [SerializeField]
    private GameObject hookPosOb;
    [SerializeField]
    private GameObject hookAimOb;

    [SerializeField]
    private LineRenderer lr;

    [SerializeField]
    private Material[] dummyRopeMt;
    [SerializeField]
    private Material[] realRopeMt;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private Vector3 mousePos;
    [SerializeField]
    private RaycastHit2D hookHit;
    [SerializeField]
    private float hookShotPower;

    [SerializeField]
    private bool isHookShot;

    private void Awake()
    {
        if (lr == null)
            lr = GetComponent<LineRenderer>();

        cursorOb = Instantiate(cursorOb, transform.position, Quaternion.identity);
    }


    private void Update()
    {

    }
    private void DrawRope(bool drawing)
    {
        
    }

    #region Mouse / Rope Action
    private void OnMousePos(InputValue value)
    {
        // cursorPos is mousePos
        // +Linerendering
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // move cursor
        cursorOb.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        if (!prAction.IsJointed)
        {
            RopeRayCast();
        }
        else
        {
            hookAimOb.SetActive(false);
            lr.positionCount = 0;
        }
    }
    private void RopeRayCast()
    {
        Vector2 rayDir = (mousePos - transform.position).normalized;
        hookHit = Physics2D.Raycast(transform.position, rayDir, 100f, Manager.Layer.ropeInteractableLM);

        if (hookHit)
        {
            hookAimOb.SetActive(true);
            SetHookObjectPos();
            lr.positionCount = 2;
            lr.SetPosition(0, hookPosOb.transform.position);
            lr.SetPosition(1, hookHit.point);
        } 
        else
        {
            hookAimOb.SetActive(false);
            lr.positionCount = 0;
        }
    }

    // hooking aim setting
    private void SetHookObjectPos()
    {
        Vector3 dist = new Vector3(hookHit.point.x - prAction.Rigid.transform.position.x, hookHit.point.y - prAction.Rigid.transform.position.y, 0);
        float zRot = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;

        hookPosOb.transform.rotation = Quaternion.Euler(0, 0, zRot - 90f);
        hookPosOb.transform.position = transform.position + dist.normalized * 2f;
    }

    private void OnMouseClick(InputValue value)
    {
        if (!prAction.IsJointed && hookHit)
            RopeShot();
    }
    private void RopeShot()
    {
        Vector3 dist = new Vector3(hookHit.point.x - prAction.Rigid.transform.position.x, hookHit.point.y - prAction.Rigid.transform.position.y, 0);
        float zRot = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;

        prAction.Anim.Play("RopeShot");
        isHookShot = true;

        GameObject hookOb = Instantiate(hookPrefab, hookPosOb.transform.position, hookPosOb.transform.rotation);
        Hook hook = hookOb.GetComponent<Hook>();
        // CCD 세팅
        // 시간 : 거리/속력
        hook.ccdRoutine = StartCoroutine(hook.CCD(dist.magnitude/hookShotPower, new Vector3(hookHit.point.x, hookHit.point.y, 0)));
        hook.Owner = prAction;
        // rope shot
        hook.Rigid?.AddForce(dist.normalized * hookShotPower, ForceMode2D.Impulse);
    }

    #endregion

}
