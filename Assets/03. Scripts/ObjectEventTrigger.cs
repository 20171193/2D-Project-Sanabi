using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Cinemachine;
public class ObjectEventTrigger : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLM;

    [SerializeField]
    private bool playOnce;

    [Header("Camera Action")]
    [SerializeField]
    private bool isCameraAciton;
    [SerializeField]
    private CinemachineVirtualCamera eventCamera;
    [SerializeField]
    private float cameraActionTime;
    private Coroutine cameraActionRoutine;
    private PlayerInput prInput;    // 플레이어 입력제어

    public UnityEvent OnEnterTrigger;
    public UnityEvent OnExitTrigger;

    private void OnEnable()
    {
        prInput = GameObject.FindWithTag("Player").GetComponent<PlayerInput>();
    }

    public void OnCameraAction()
    {
        cameraActionRoutine = StartCoroutine(CameraActionRoutine());
    }
    IEnumerator CameraActionRoutine()
    {
        // 플레이어 입력 제어
        prInput.enabled = false;
        // 이벤트 카메라로 변경
        Manager.Camera.SetCameraPriority(CameraType.CutScene, eventCamera);

        yield return new WaitForSeconds(cameraActionTime);
        
        // 원상 복구
        prInput.enabled = true;
        Manager.Camera.SetCameraPriority(CameraType.Main);

        if (playOnce == true)
            Destroy(gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(targetLM.Contain(collision.gameObject.layer))
        {
            OnEnterTrigger?.Invoke();
            if (isCameraAciton && enabled == true)
            {
                OnCameraAction();
            }
            else if (playOnce == true)
                Destroy(gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (targetLM.Contain(collision.gameObject.layer))
        {
            OnExitTrigger?.Invoke();
        }
    }
}
