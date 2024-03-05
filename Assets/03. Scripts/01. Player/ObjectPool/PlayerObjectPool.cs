using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectPool : MonoBehaviour
{
    public enum ObjectType
    {
        Hook,
        VFX
    }

    [SerializeField]
    private PlayerBase owner;

    [SerializeField]
    private Hook hookPrefab;
    public Hook HookPrefab { get { return hookPrefab; } }

    [SerializeField]
    //private Dictionary<string, PlayerVFX> vfxDic;



    public void GetObject(ObjectType type, Vector3 position, Quaternion rotation)
    {
        switch(type)
        {
            case ObjectType.Hook:
                break;
            case ObjectType.VFX:
                break;
            default:
                break;
        }
    }
    public void ReturnObject()
    {

    }

    private void EnableVFX()
    {

    }

    private void DisableVFX()
    {
    }
}
