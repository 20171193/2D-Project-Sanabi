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
    Enemy,
    Interactable
}

public class HookAim : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private LineRenderer lr;

    [SerializeField]
    private SpriteRenderer aimSpr;

    [SerializeField]
    private GameObject aimRendererOb;
    [SerializeField]
    private GameObject arrivingAim;

    [SerializeField]
    private Sprite groundArrivingAimSp;
    [SerializeField]
    private Sprite enemyArrivingAimSp;
    
    [SerializeField]
    private Sprite groundAimSp;
    [SerializeField]
    private Sprite enemyAimSp;

    [SerializeField]
    private Material[] groundLineMt;
    [SerializeField]
    private Material[] enemyLineMt;

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
        arrivingAim.SetActive(true);

        switch (type)
        {
            case LineRenderType.Ground:
                lr.materials = groundLineMt;
                aimSpr.sprite = groundAimSp;
                arrivingAim.GetComponent<SpriteRenderer>().sprite = groundArrivingAimSp;
                break;
            case LineRenderType.Enemy:
                lr.materials = enemyLineMt;
                aimSpr.sprite = enemyAimSp;
                arrivingAim.GetComponent<SpriteRenderer>().sprite = enemyArrivingAimSp;
                break;
            case LineRenderType.Interactable:
                lr.materials = groundLineMt;
                break;
            default:
                break;
        }

        lr.positionCount = 2;
        renderTargetPos = targetPos;
        isLineRendering = true;
    }
    public void LineOff()
    {
        if (!isLineRendering) return;

        arrivingAim.SetActive(false);
        aimSpr.sprite = groundAimSp;
        lr.positionCount = 0;
        isLineRendering = false;
    }
    private void LineRendering()
    {
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, renderTargetPos);
        arrivingAim.transform.position = renderTargetPos;
    }
}
