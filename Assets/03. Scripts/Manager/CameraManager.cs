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
    public Collider2D CurrentConfiner { get { return mainConfiner?.m_BoundingShape2D; } }

    [SerializeField]
    private CinemachineBrain cameraBrain;
    public CinemachineBrain CameraBrain { get { return cameraBrain; } }

    [SerializeField]
    private float originBlendTime = 2f;
    public float OriginBlendTime { get { return originBlendTime; } }

    [SerializeField]
    private GlitchEffect glitch;

    protected override void Awake()
    {
        base.Awake();

        mainConfiner = mainCamera.GetComponent<CinemachineConfiner2D>();
        currentCamera = mainCamera;
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        glitch = Camera.main.GetComponent<GlitchEffect>();
        cameraBrain = Camera.main.GetComponent<CinemachineBrain>();

        mainCamera.Follow = player.transform;
        zoomCamera.Follow = player.transform;

        zoomCamera.Priority = (int)CameraOrder.IdleCamera;
        mainCamera.Priority = (int)CameraOrder.CurrentCamera;
    }

    // 카메라 전환 블렌딩타임 적용
    public void SetBlendTime(float time)
    {
        cameraBrain.m_DefaultBlend.m_Time = time;
    }
    // 카메라 전환 블렌딩타임 원복
    public void SetDefaultBlendTime()
    {
        cameraBrain.m_DefaultBlend.m_Time = originBlendTime;
    }

    // 카메라 우선순위 적용
    public void SetCameraPriority(CameraType type, CinemachineVirtualCamera cc = null)
    {
        mainCamera.Priority = (int)CameraOrder.IdleCamera;
        zoomCamera.Priority=(int)CameraOrder.IdleCamera;
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
    
    // Confiner 적용
    public void SetConfiner(Collider2D shape)
    {
        mainConfiner.m_BoundingShape2D = shape;
    }

    // 글리치 이펙트 
    public void OnGlitchEffect()
    {
        glitch.enabled = true;
    }
    public void OffGlitchEffect()
    {
        glitch.enabled = false;
    }



}
