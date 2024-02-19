using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    #region 싱글턴 메서드
    private static Manager instance = null;
    [SerializeField] CoroutineManager coroutineManager;
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
