using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraManager : MonoBehaviour
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
    private CinemachineVirtualCamera eventCamera;
    public CinemachineVirtualCamera EC { get { return eventCamera; } }

    [SerializeField]
    private CinemachineVirtualCamera currentCamera;
    public CinemachineVirtualCamera CC { get { return currentCamera; } }

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<CinemachineVirtualCamera>();
        eventCamera = GameObject.FindWithTag("EventCamera").GetComponent<CinemachineVirtualCamera>();
        mainCamera.Follow = player.transform;
        eventCamera.Follow = player.transform;

        eventCamera.Priority = (int)CameraOrder.IdleCamera;
        mainCamera.Priority = (int)CameraOrder.CurrentCamera;
        currentCamera = mainCamera;
    }

    private void InitPriority()
    {
        eventCamera.Priority = (int)CameraOrder.IdleCamera;
        mainCamera.Priority = (int)CameraOrder.CurrentCamera;

        currentCamera = mainCamera;
    }

    public void SetMainCamera()
    {
        InitPriority();
        
        currentCamera.Priority = (int)CameraOrder.IdleCamera;
        mainCamera.Priority = (int)CameraOrder.CurrentCamera;

        currentCamera = mainCamera;
    }
    public void SetEventCamera()
    {
        InitPriority();
        currentCamera.Priority = (int)CameraOrder.IdleCamera;
        eventCamera.Priority = (int)CameraOrder.CurrentCamera;

        currentCamera = eventCamera;
    }

    public void SetCutSceneCamera(CinemachineVirtualCamera cutSceneCamera)
    {
        InitPriority();
        currentCamera.Priority = (int)CameraOrder.IdleCamera;
        cutSceneCamera.Priority = (int)CameraOrder.CurrentCamera;

        currentCamera = cutSceneCamera;
    }


}
