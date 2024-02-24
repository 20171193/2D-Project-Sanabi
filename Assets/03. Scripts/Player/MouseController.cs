using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    private RaycastHit2D hookHitInfo;

    [SerializeField]
    private Vector3 mousePos;
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
    #region Mouse / Rope Action
    // Raycast to mouse position
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
    // if Raycast hit is not null, linerendering to hit.point
    private void RopeRayCast()
    {
        Vector2 rayDir = (mousePos - transform.position).normalized;
        hookHitInfo = Physics2D.Raycast(transform.position, rayDir, 50f, Manager.Layer.ropeInteractableLM);
        if (hookHitInfo)
        {
            hookAimOb.SetActive(true);
            SetHookObjectPos();
            lr.positionCount = 2;
            lr.SetPosition(0, hookPosOb.transform.position);
            lr.SetPosition(1, hookHitInfo.point);
        } 
        else
        {
            hookAimOb.SetActive(false);
            lr.positionCount = 0;
        }
    }
    // and hook aim transform setting
    private void SetHookObjectPos()
    {
        Vector3 dist = new Vector3(hookHitInfo.point.x - prAction.Rigid.transform.position.x, hookHitInfo.point.y - prAction.Rigid.transform.position.y, 0);
        float zRot = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;

        hookPosOb.transform.rotation = Quaternion.Euler(0, 0, zRot - 90f);
        hookPosOb.transform.position = transform.position + dist.normalized * 2f;
    }

    // hookshot to mouse position
    private void OnMouseClick(InputValue value)
    {
        if(value.isPressed)
        {
            if (!prAction.IsJointed && hookHitInfo)
                HookShot();
        }
        else
        {

        }
    }
    // if hook collide with enemy, Invoke OnGrabbedEnemy
    // else if hook collide with ground, Invoke OnGrabbedGround
    private void HookShot()
    {
        Vector3 dist = new Vector3(hookHitInfo.point.x - prAction.Rigid.transform.position.x, hookHitInfo.point.y - prAction.Rigid.transform.position.y, 0);
        float zRot = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;

        prAction.Anim.Play("RopeShot");
        isHookShot = true;

        GameObject hookOb = Instantiate(hookPrefab, hookPosOb.transform.position, hookPosOb.transform.rotation);
        Hook hook = hookOb.GetComponent<Hook>();

        // CCD setting
        // time = distance / velocity
        hook.ccdRoutine = StartCoroutine(hook.CCD(dist.magnitude/hookShotPower, new Vector3(hookHitInfo.point.x, hookHitInfo.point.y, 0)));
        hook.Owner = prAction;
        
        // 

        // rope shot
        hook.Rigid?.AddForce(dist.normalized * hookShotPower, ForceMode2D.Impulse);
    }

    #endregion

}
