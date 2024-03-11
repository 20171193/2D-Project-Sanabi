using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JusticeVFXPooler : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> registVFXList;

    private Dictionary<string, GameObject> nameDic;
    private Dictionary<string, Stack<GameObject>> vfxPool;

    private void Awake()
    {
        nameDic = new Dictionary<string, GameObject>();
        vfxPool = new Dictionary<string, Stack<GameObject>>();

        CreatePool();
    }
    private void CreatePool()
    {
        foreach(GameObject ob in registVFXList)
        {
            JusticeVFX vfx = ob.GetComponent<JusticeVFX>();
            if (vfx == null) continue;
            string name = ob.name.Replace("VFX_Justice", "");
            nameDic.Add(name, ob);

            Stack<GameObject> stack = new Stack<GameObject>();
            for(int i =0; i<vfx.Size; i++)
            {
                GameObject inst = Instantiate(ob);
                JusticeVFX vfxInst = inst.GetComponent<JusticeVFX>();

                inst.gameObject.SetActive(false);
                inst.transform.parent = transform;
                vfxInst.VFXName = name;
                vfxInst.Pooler = this;
                stack.Push(inst);
            }
            vfxPool.Add(name, stack);
        }
    }

    public GameObject ActiveVFX(string name)
    {
        if (!vfxPool.ContainsKey(name)) return null;

        GameObject inst = null;

        if (vfxPool[name].Count > 0)
            inst = vfxPool[name].Pop();
        else
        {
            inst = Instantiate(nameDic[name]);
            JusticeVFX vfxInst = inst.GetComponent<JusticeVFX>();
            vfxInst.VFXName = name;
            vfxInst.Pooler = this;
        }
        inst.transform.parent = null;
        inst.gameObject.SetActive(true);
        return inst;
    }

    public void InActiveVFX(GameObject ob)
    {
        JusticeVFX vfx = ob.GetComponent<JusticeVFX>();
        if (vfx == null) return;

        string name = vfx.VFXName;
        if (!vfxPool.ContainsKey(name)) return;

        ob.SetActive(false);
        ob.transform.position = transform.position;
        ob.transform.rotation = transform.rotation;
        ob.transform.parent = transform;
        vfxPool[name].Push(ob);
    }
}
