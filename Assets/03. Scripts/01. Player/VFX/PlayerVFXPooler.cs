using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerVFXPooler : MonoBehaviour
{
    [SerializeField]
    private Dictionary<string, GameObject> vfxPool;

    [SerializeField]
    private Transform vfxTop;
    [SerializeField]
    private Dictionary<string, Transform> vfxTranform;

    private void Awake()
    {
        vfxPool = new Dictionary<string, GameObject>();
        vfxTranform = new Dictionary<string, Transform>();

        // Assign vfx object
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.GetComponent<PlayerVFX>().Pooler = this;

            child.SetActive(false);
            vfxPool.Add(child.name.Replace("VFX_", ""), child);
        }

        // Assign vfx transform
        for(int i = 0; i < vfxTop.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            vfxTranform.Add(child.gameObject.name.Replace("VFX_", ""), child);
        }
    }

    public void ActiveVFX(string name)
    {
        GameObject getObject = vfxPool[name];
        Transform getTransform = vfxTranform[name];

        getObject.transform.position = getTransform.position;
        getObject.transform.rotation = getTransform.rotation;
        getObject.transform.parent = null;

        // reset gameobject
        if (getObject.activeSelf)
            getObject.SetActive(false);

        getObject.SetActive(true);
    }

    public void ReturnPool(GameObject vfx)
    {
        vfx.SetActive(false);
        vfx.transform.parent = this.transform;
    }
}
