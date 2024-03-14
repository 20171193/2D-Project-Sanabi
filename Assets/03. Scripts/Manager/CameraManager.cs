using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum CameraType
{
    Main,
    Zoom
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
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        mainCamera.Follow = player.transform;
        zoomCamera.Follow = player.transform;

        zoomCamera.Priority = (int)CameraOrder.IdleCamera;
        mainCamera.Priority = (int)CameraOrder.CurrentCamera;
        currentCamera = mainCamera;
    }


    private void InitPriority()
    {
        if(currentCamera)
            currentCamera.Priority = (int)CameraOrder.IdleCamera;

        zoomCamera.Priority = (int)CameraOrder.IdleCamera;
        mainCamera.Priority = (int)CameraOrder.CurrentCamera;

        currentCamera = mainCamera;
    }

    public void SetCameraPriority(CameraType type)
    {
        InitPriority();

        switch(type)
        {
            case CameraType.Main:
                break;
            case CameraType.Zoom:
                zoomCamera.Priority = (int)CameraOrder.CurrentCamera;
                mainCamera.Priority = (int)CameraOrder.IdleCamera;
                currentCamera = zoomCamera;
                break;
            default:
                break;
        }
    }
    public void SetCutSceneCamera(CinemachineVirtualCamera cutSceneCamera)
    {
        InitPriority();
        currentCamera.Priority = (int)CameraOrder.IdleCamera;
        cutSceneCamera.Priority = (int)CameraOrder.CurrentCamera;

        currentCamera = cutSceneCamera;
    }

    public void SetConfiner(Collider2D shape)
    {
        mainConfiner.m_BoundingShape2D = shape;
    }

}
