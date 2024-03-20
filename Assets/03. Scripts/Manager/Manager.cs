using UnityEngine;

public class Manager : MonoBehaviour
{
    public static LayerManager Layer { get { return LayerManager.Instance; } }
    public static PoolManager Pool { get { return PoolManager.Instance; } }
    public static GameManager Game { get { return GameManager.Instance; } }
    public static CameraManager Camera { get { return CameraManager.Instance; } }
    public static DataManager Data { get { return DataManager.Instance; } }
    public static SoundManager Sound { get { return SoundManager.Instance; } }
    public static CoroutineManager Coroutine { get { return CoroutineManager.Instance; } }
    public static SceneManager Scene { get { return SceneManager.Instance; } }
    public static UIManager UI { get { return UIManager.Instance; } }

    // 씬이 로드되기 전에 싱글턴 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        GameManager.ReleaseInstance();
        LayerManager.ReleaseInstance();
        PoolManager.ReleaseInstance();
        CameraManager.ReleaseInstance();
        DataManager.ReleaseInstance();
        SoundManager.ReleaseInstance();
        CoroutineManager.ReleaseInstance();
        SceneManager.ReleaseInstance();
        UIManager.ReleaseInstance();

        GameManager.CreateInstance();
        LayerManager.CreateInstance();
        PoolManager.CreateInstance();
        CameraManager.CreateInstance();
        DataManager.CreateInstance();
        SoundManager.CreateInstance();
        CoroutineManager.CreateInstance();
        SceneManager.CreateInstance();
        UIManager.CreateInstance();
    }
}
