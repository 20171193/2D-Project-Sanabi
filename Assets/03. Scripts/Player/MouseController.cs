using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private GameObject owner;
    private PlayerAction prAction;
    [SerializeField]
    private GameObject hookOb;
    private Hook hook;

    [SerializeField]
    private GameObject cursorOb;
    [SerializeField]
    private Transform hookPos;
    [SerializeField]
    private LineRenderer lr;

    [SerializeField]
    private Material[] dummyRopeMt;
    [SerializeField]
    private Material[] realRopeMt;

    [Space(3)]
    [Header("Layer")]
    [Space(2)]
    [SerializeField]
    private LayerMask ropeInteractableLM;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private Vector3 mousePos;
    [SerializeField]
    private RaycastHit2D hookHit;

    [SerializeField]
    private GameObject dummyHook;


    private void Awake()
    {
        if (owner == null)
            owner = GameObject.FindWithTag("Player");

        prAction = owner.GetComponent<PlayerAction>();
        //hook = hookOb.GetComponent<Hook>(); 

        if (lr == null)
            lr = GetComponent<LineRenderer>();

        cursorOb = Instantiate(cursorOb, transform.position, Quaternion.identity);
    }


    private void Update()
    {
        if (lr.positionCount > 0)
            DrawRope();
    }
    private void DrawRope()
    {
        lr.SetPosition(0, hookPos.position);
        lr.SetPosition(1, hookHit.point);
    }

    #region Mouse / Rope Action
    private void OnMousePos(InputValue value)
    {
        // cursorPos is mousePos
        // +Linerendering
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // move cursor
        cursorOb.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        SetHookPos();

        if (!prAction.IsJointed)
            RopeRayCast();
    }
    private void SetHookPos()
    {
       
    }
    private void RopeRayCast()
    {
        Vector2 rayDir = (mousePos - transform.position).normalized;
        hookHit = Physics2D.Raycast(transform.position, rayDir, 100f, ropeInteractableLM);

        if (hookHit)
            lr.positionCount = 2;
        else
            lr.positionCount = 0;
        
    }
    private void OnMouseClick(InputValue value)
    {
        if (!prAction.IsJointed && hookHit)
            RopeShoot();
    }
    private void RopeShoot()
    {
        GameObject hook = Instantiate(dummyHook, transform.position, Quaternion.identity);
        Rigidbody2D rd = hook.AddComponent<Rigidbody2D>();
        rd.gravityScale = 0;

        Vector3 dist = new Vector3(hookHit.point.x - prAction.Rigid.transform.position.x, hookHit.point.y - prAction.Rigid.transform.position.y, 0);
        float zRot = Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;

        // È¸Àü
        rd.transform.rotation = Quaternion.Euler(0, 0, zRot-90f);  
        // ½¸
        rd.AddForce(dist.normalized * 1000f, ForceMode2D.Impulse);
        Destroy(hook, 2.0f);
    }

    #endregion

}
