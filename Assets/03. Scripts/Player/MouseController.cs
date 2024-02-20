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


    private void Awake()
    {
        if (owner == null)
            owner = GameObject.FindWithTag("Player");

        prAction = owner.GetComponent<PlayerAction>();

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
        // Draw Real Rope
        if (prAction.IsJointed)
        {
            lr.materials = realRopeMt;
            lr.SetPosition(1, prAction.JointedOB.GetComponent<Hook>().HookingPos.position);
        }
        // Draw Dummy Rope
        else
        {
            lr.materials = dummyRopeMt;
            lr.SetPosition(1, hookHit.point);
        }
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
        {
            lr.positionCount = 2;
        }
        else
        {
            lr.positionCount = 0;
        }
    }
    private void OnMouseClick(InputValue value)
    {
        if (!prAction.IsJointed)
            RopeShoot();
    }
    private void RopeShoot()
    {
        if (hookHit)
        {
            prAction.JointedOB = Instantiate(hookOb, hookHit.point + hookHit.normal*0.5f, Quaternion.identity);    // Instantiate Hook Object
            DistanceJoint2D distJoint = prAction.JointedOB.GetComponent<DistanceJoint2D>();  
            distJoint.connectedBody = owner.GetComponent<Rigidbody2D>();            // Connect joint to Player
            prAction.IsJointed = true;
        }
    }
    #endregion

}
