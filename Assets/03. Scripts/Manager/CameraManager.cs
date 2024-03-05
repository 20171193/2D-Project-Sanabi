using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera mainChapterCamera;
    public CinemachineVirtualCamera MCC { get { return mainChapterCamera; } }

    [SerializeField]
    private CinemachineVirtualCamera eventCamera;
    public CinemachineVirtualCamera EC { get { return eventCamera; } }

    private const int FirstPriority = 5;
    private const int LastPriority = 2;

    public void SetMainCamera()
    {
        eventCamera.Priority = LastPriority;
        mainChapterCamera.Priority = FirstPriority;
    }
    public void SetEventCamera()
    {
        mainChapterCamera.Priority = LastPriority;
        eventCamera.Priority = FirstPriority;
    }
}
