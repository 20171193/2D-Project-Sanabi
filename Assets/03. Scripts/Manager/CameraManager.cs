using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum CameraType
{
    Main,
    Zoom,
    CutScene
}

public class CameraManager : Singleton<CameraManager>
{
    enum CameraOrder
    {
        IdleCamera,
        CurrentCamera
    }

    [SerializeField]
    private CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera MC { get { return mainCamera; } }

    [SerializeField]
    private CinemachineVirtualCamera zoomCamera;
    public CinemachineVirtualCamera ZC { get { return zoomCamera; } }

    [SerializeField]
    private CinemachineVirtualCamera currentCamera;
    public CinemachineVirtualCamera CC { get { return currentCamera; } }

    [SerializeField]
    private CinemachineConfiner2D mainConfiner;

    protected override void Awake()
    {
        base.Awake();
        mainConfiner = mainCamera.GetComponent<CinemachineConfiner2D>();
        currentCamera = mainCamera;
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        mainCamera.Follow = player.transform;
        zoomCamera.Follow = player.transform;

        zoomCamera.Priority = (int)CameraOrder.IdleCamera;
        mainCamera.Priority = (int)CameraOrder.CurrentCamera;
    }

    public void SetCameraPriority(CameraType type, CinemachineVirtualCamera cc = null)
    {
        currentCamera.Priority = (int)CameraOrder.IdleCamera;

        switch (type)
        {
            case CameraType.Main:
                mainCamera.Priority = (int)CameraOrder.CurrentCamera;
                currentCamera = mainCamera;
                break;
            case CameraType.Zoom:
                zoomCamera.Priority = (int)CameraOrder.CurrentCamera;
                currentCamera = zoomCamera;
                break;
            case CameraType.CutScene:
                if (cc == null) break;
                cc.Priority = (int)CameraOrder.CurrentCamera;
                currentCamera = cc;
                break;
            default:
                break;
        }
    }

    public void SetConfiner(Collider2D shape)
    {
        mainConfiner.m_BoundingShape2D = shape;
    }

}
