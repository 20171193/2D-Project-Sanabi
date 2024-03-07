using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraManager : MonoBehaviour
{
    enum CameraOrder
    {
        EventCamera,
        MainCamera,
        CurrentCamera
    }

    [SerializeField]
    private CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera MC { get { return mainCamera; } }

    [SerializeField]
    private CinemachineVirtualCamera eventCamera;
    public CinemachineVirtualCamera EC { get { return eventCamera; } }

    [SerializeField]
    private CinemachineVirtualCamera currentCamera;
    public CinemachineVirtualCamera CC { get { return currentCamera; } }

    private void Awake()
    {
        eventCamera.Priority = (int)CameraOrder.EventCamera;
        mainCamera.Priority = (int)CameraOrder.MainCamera;
    }

    private void InitPriority()
    {
        eventCamera.Priority = (int)CameraOrder.EventCamera;
        mainCamera.Priority = (int)CameraOrder.MainCamera;

        if(currentCamera != null)
            currentCamera.Priority = (int)CameraOrder.CurrentCamera;
    }

    public void SetMainCamera()
    {
        InitPriority();
        currentCamera = mainCamera;
        currentCamera.Priority = (int)CameraOrder.CurrentCamera;
    }
    public void SetEventCamera()
    {
        InitPriority();
        currentCamera = eventCamera;
        currentCamera.Priority = (int)CameraOrder.CurrentCamera;
    }

    public void SetCutSceneCamera(CinemachineVirtualCamera cutSceneCamera)
    {
        InitPriority();
        currentCamera = cutSceneCamera;
        currentCamera.Priority = (int)CameraOrder.CurrentCamera;
    }


}
