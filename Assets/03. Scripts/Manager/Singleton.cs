using System.IO;
using UnityEngine;

// ΩÃ±€≈œ∆–≈œ
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance { get { return instance; } }

    [SerializeField]
    protected string prefabPath;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void CreateInstance(string path)
    {
        T resource = Resources.Load<T>($"Manager/{typeof(T).Name}");
        instance = Instantiate(resource);
    }

    public static void ReleaseInstance()
    {
        if (instance == null)
            return;

        Destroy(instance.gameObject);
        instance = null;
    }
}