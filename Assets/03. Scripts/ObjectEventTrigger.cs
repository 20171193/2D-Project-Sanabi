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
    [Space(2)]
    [SerializeField]
    private float blendTime;
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

    [Space(3)]
    [Header("Balancing")]
    [Space(2)]
    [SerializeField]
    private float originBlendTime;

    private CinemachineBrain cinemachineBrain;
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        originBlendTime = cinemachineBrain.m_DefaultBlend.BlendTime;
        cinemachineBrain.m_DefaultBlend.m_Time = blendTime;

        prInput = GameObject.FindWithTag("Player").GetComponent<PlayerInput>();
    }

    public void DoImpulse()
    {
        impulseSource.GenerateImpulse();
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
            Destroy(gameObject, cinemachineBrain.m_DefaultBlend.BlendTime);
    }

    private void OnDestroy()
    {
        cinemachineBrain.m_DefaultBlend.m_Time = originBlendTime; 
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
