using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    #region 싱글턴 메서드
    public static LayerManager Layer { get { return LayerManager.Instance; }} 
    public static PoolManager Pool { get { return PoolManager.Instance; } }
    public static GameManager Game { get { return GameManager.Instance; } }
    public static CameraManager Camera { get { return CameraManager.Instance; }}
    public static CoroutineManager Coroutine { get { return CoroutineManager.Instance;} }
    public static DataManager Data { get { return DataManager.Instance; } }

    // 씬이 로드되기 전에 싱글턴 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        GameManager.ReleaseInstance();
        LayerManager.ReleaseInstance();
        PoolManager.ReleaseInstance();
        CameraManager.ReleaseInstance();
        CoroutineManager.ReleaseInstance();
        DataManager.ReleaseInstance();

        GameManager.CreateInstance();
        LayerManager.CreateInstance();
        PoolManager.CreateInstance();
        CameraManager.CreateInstance();
        CoroutineManager.CreateInstance();
        DataManager.CreateInstance();
    }
    #endregion
}
