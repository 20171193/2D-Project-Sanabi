using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerVFXPooler : MonoBehaviour
{
    [SerializeField]
    private Dictionary<string, PlayerVFX> vfxPool;

    private void Awake()
    {
        GameObject[] vfxArray = transform.GetComponentsInChildren<GameObject>();
        foreach(GameObject ob in vfxArray)
        {
            vfxPool.Add(ob.name.Replace("VFX_", ""), ob.GetComponent<PlayerVFX>());
        }
    }
}
