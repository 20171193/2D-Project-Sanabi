using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField]
    protected Stack<PooledObject> objectPool;
    [SerializeField]
    protected PooledObject prefab;

    [SerializeField]
    protected int size;
    [SerializeField]
    protected int capacity;

    public virtual void CreatePool(PooledObject prefab, int size, int capacity)
    {
        this.prefab = prefab;
        this.size = size;
        this.capacity = capacity;

        objectPool = new Stack<PooledObject>(capacity);

        for(int i =0; i<size; i++)
        {
            PooledObject instance = Instantiate(prefab);
            instance.gameObject.SetActive(false);
            instance.Pooler = this;
            instance.transform.parent = transform;
            objectPool.Push(instance);
        }
    }
    public virtual PooledObject GetPool(Vector3 position, Quaternion rotation)
    {
        if(objectPool.Count > 0)
        {
            PooledObject instance = objectPool.Pop();
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.gameObject.SetActive(true);
            return instance;
        }
        // if pool has no element, instantiate prefab
        else
        {
            PooledObject instance = Instantiate(prefab);
            instance.Pooler = this;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            return instance;
        }
    }
    public void ReturnPool(PooledObject instance)
    {
        if (objectPool.Count < capacity)
        {
            instance.gameObject.SetActive(false);
            instance.transform.parent = transform;
            objectPool.Push(instance);
        }
        // if pool is full, destroy created instance
        else
            Destroy(instance.gameObject);
    }
}
