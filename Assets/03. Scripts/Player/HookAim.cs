using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum LineRenderType
{
    Ground,
    Enemy
}

public class HookAim : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private LineRenderer lr;

    [SerializeField]
    private GameObject aimRendererOb;

    [SerializeField]
    private Material[] enemyLineMt;
    [SerializeField]
    private Material[] groundLineMt;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    private Vector3 renderTargetPos;
    public Vector3 RenderTargetPos { get { return renderTargetPos; } }

    [SerializeField]
    private bool isLineRendering;


    private void Awake()
    {
        if (lr == null)
            lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if(isLineRendering) 
            LineRendering();
    }
    public void LineOn(LineRenderType type, Vector3 targetPos)
    {
        aimRendererOb.SetActive(true);

        switch (type)
        {
            case LineRenderType.Ground:
                lr.materials = groundLineMt;
                break;
            case LineRenderType.Enemy:
                lr.materials = groundLineMt;
                break;
        }

        lr.positionCount = 2;
        renderTargetPos = targetPos;
        isLineRendering = true;
    }
    public void LineOff()
    {
        if (!isLineRendering) return;

        lr.positionCount = 0;
        isLineRendering = false;
    }
    private void LineRendering()
    {
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, renderTargetPos);
    }
}
