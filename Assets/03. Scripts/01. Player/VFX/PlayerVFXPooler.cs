using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.Image;

public class PlayerVFXPooler : MonoBehaviour
{
    [Serializable]
    struct VFX
    {
        public Stack<GameObject> vfxObjectPool;
        // return Transform
        public Transform originTr;

        public VFX(Stack<GameObject> vfxObjectPool, Transform originTr)
        {
            this.vfxObjectPool = vfxObjectPool;
            this.originTr = originTr;
        }
    }
    [Header("Regist VFX Object List")]
    [SerializeField]
    private List<GameObject> registVFXList;

    private Dictionary<string, GameObject> prefabDic;
    private Dictionary<string, VFX> vfxPool;

    private void Awake()
    {
        vfxPool = new Dictionary<string, VFX>();
        prefabDic = new Dictionary<string, GameObject>();

        foreach(GameObject vfxOb in registVFXList)
        {
            //   key : name
            // value : PlayerVFX 
            prefabDic.Add(vfxOb.name.Replace("VFX_", ""), vfxOb);
        }
       
        for (int i = 0; i < transform.childCount; i++)
        {
            // vfx prefabs parent transform
            Transform originTr = transform.GetChild(i);
            // vfx prefabs name
            string name = originTr.gameObject.name.Replace("POS_", "");

            // If there is no Transform matching the name of the prefab, it skips.
            if (!prefabDic.ContainsKey(name)) continue;

            GameObject vfxOb = prefabDic[name];
            PlayerVFX vfx = vfxOb.GetComponent<PlayerVFX>();

            // vfx object pooling using a stack
            Stack<GameObject> stack = new Stack<GameObject>(vfx.Size);

            for(int j = 0; j< vfx.Size; j++)
            {
                GameObject instance = Instantiate(vfxOb);
                PlayerVFX vfxInst = instance.GetComponent<PlayerVFX>();

                instance.gameObject.SetActive(false);
                instance.transform.parent = originTr;
                vfxInst.VFXName = name;
                vfxInst.Pooler = this;
                stack.Push(instance);
            }

            vfxPool.Add(name, new VFX(stack, originTr));
        }
    }

    public void ActiveVFX(string name)
    {
        if(!vfxPool.ContainsKey(name))
        {
            Debug.Log($"{name} pool does not exist");
            return;
        }

        // When the visual effect is triggered, change the parent of the transform to null
        if (vfxPool[name].vfxObjectPool.Count > 0)
        {
            GameObject instance = vfxPool[name].vfxObjectPool.Pop();
            instance.transform.parent = null;
            instance.transform.position = vfxPool[name].originTr.position;
            instance.transform.rotation = vfxPool[name].originTr.rotation;
            instance.gameObject.SetActive(true);
        }
    }

    public void ReturnPool(GameObject getObject)
    {
        // When the effect ends, revert the parent of the transform to its original transform.
        PlayerVFX getVfx = getObject.GetComponent<PlayerVFX>();
        if (getVfx == null) return;

        string name = getVfx.VFXName;
        if (!vfxPool.ContainsKey(name)) return;

        getObject.SetActive(false);
        getObject.transform.position = vfxPool[name].originTr.position;
        getObject.transform.rotation = vfxPool[name].originTr.rotation;
        getObject.transform.parent = vfxPool[name].originTr;
        vfxPool[name].vfxObjectPool.Push(getObject);
    }
}
