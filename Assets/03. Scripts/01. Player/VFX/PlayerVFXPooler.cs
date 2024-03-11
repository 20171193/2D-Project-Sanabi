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
        // pooler의 역할을 할 구조체


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
        CreatePool();
    }

    private void CreatePool()
    {
        foreach (GameObject vfxOb in registVFXList)
        {
            //   key : name
            // value : PlayerVFX 
            prefabDic.Add(vfxOb.name.Replace("VFX_", ""), vfxOb);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            // 풀링 된 상태의 기본 트랜스폼 위치
            Transform originTr = transform.GetChild(i);
            // 이름, 트랜스폼으로 관리
            string name = originTr.gameObject.name.Replace("POS_", "");

            if (!prefabDic.ContainsKey(name)) continue;

            GameObject vfxOb = prefabDic[name];
            PlayerVFX vfx = vfxOb.GetComponent<PlayerVFX>();

            // 스택활용하여 게임오브젝트 풀링
            Stack<GameObject> stack = new Stack<GameObject>(vfx.Size);

            for (int j = 0; j < vfx.Size; j++)
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
        if (!vfxPool.ContainsKey(name))
        {
            Debug.Log($"{name} pool does not exist");
            return;
        }

        GameObject instance = null; 

        // 스택에 사용하지 않은 오브젝트가 존재할 경우 
        if (vfxPool[name].vfxObjectPool.Count > 0)
            instance = vfxPool[name].vfxObjectPool.Pop();
        // 스택 내 모든 오브젝트를 사용한 경우
        else
        {
            instance = Instantiate(prefabDic[name]);
            PlayerVFX vfxInst = instance.GetComponent<PlayerVFX>();

            instance.transform.parent = null;
            vfxInst.VFXName = name;
            vfxInst.Pooler = this;
        }

        instance.transform.parent = null;
        instance.transform.position = vfxPool[name].originTr.position;
        instance.transform.rotation = vfxPool[name].originTr.rotation;
        instance.gameObject.SetActive(true);
    }


    public void ReturnPool(GameObject getObject)
    {
        // 비주얼 효과가 비활성화 될 때 다시 풀의 자식으로 초기화
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
