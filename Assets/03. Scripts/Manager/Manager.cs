using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class Manager : MonoBehaviour
{
    #region 싱글턴 메서드
    private static Manager instance = null;
    [SerializeField]
    private CoroutineManager coroutineManager;
    [SerializeField]
    private GameManager gameManager;

    public static GameManager Game { get { return instance.gameManager; } }
    public static CoroutineManager Coroutine { get { return instance.coroutineManager; } }
   

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //this.sceneInfo = instance.sceneInfo;
            Destroy(this.gameObject);
        }
    }
    public static Manager Instance
    {
        get
        {
            if (instance == null)
                return null;
            else
                return instance;
        }
    }
    #endregion
}
