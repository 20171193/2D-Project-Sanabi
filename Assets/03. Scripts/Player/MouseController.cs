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


    [Space(3)]
    [Header("Layer")]
    [Space(2)]
    private LayerMask ropeInteractableLM;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private Vector3 mousePos;
    [SerializeField]
    private RaycastHit2D hookHit;
    [SerializeField]
    private GameObject jointedOb;


    private void Awake()
    {
        if (owner == null)
            owner = GameObject.FindWithTag("Player");

        prAction = owner.GetComponent<PlayerAction>();

        if (lr == null)
            lr = GetComponent<LineRenderer>();
    }


    private void Update()
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
            //Debug.Log("hit!");
            lr.positionCount = 2;
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, hookHit.point);
            DrawDummyRope();
        }
        else
        {
            //Debug.Log("not hit");
            lr.positionCount = 0;
        }
    }

    private void OnMouseClick(InputValue value)
    {
        if (!prAction.IsJointed)
            RopeShoot();
    }
    private void DrawDummyRope()
    {

    }
    private void RopeShoot()
    {
        if (hookHit)
        {
            jointedOb = Instantiate(hookOb, hookHit.point, Quaternion.identity);    // Instantiate Hook Object
            DistanceJoint2D distJoint = jointedOb.GetComponent<DistanceJoint2D>();  
            distJoint.connectedBody = owner.GetComponent<Rigidbody2D>();            // Connect joint to Player
            prAction.IsJointed = true;
            DrawRope();
        }
    }
    private void DrawRope()
    {

    }
    #endregion

}
