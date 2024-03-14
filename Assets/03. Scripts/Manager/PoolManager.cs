using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, ObjectPooler> poolDic = new Dictionary<string, ObjectPooler>();

    public void CreatePool(PooledObject prefab, int size, int capacity)
    {
        GameObject gameObject = new GameObject();
        gameObject.name = $"Pool_{prefab.name}";

        ObjectPooler pooler = gameObject.AddComponent<ObjectPooler>();

        pooler.CreatePool(prefab, size, capacity);
        poolDic.Add(prefab.name, pooler);
    }
    public void DestroyPool(PooledObject prefab)
    {
        ObjectPooler pooler = poolDic[prefab.name];
        Destroy(pooler.gameObject);

        poolDic.Remove(prefab.name);
    }
    public PooledObject GetPool(PooledObject prefab, Vector3 position, Quaternion rotation)
    {
        return poolDic[prefab.name].GetPool(position, rotation);
    }
}
