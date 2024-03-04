using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class HookPooler : ObjectPooler
{
    [SerializeField]
    private PlayerHooker owner;

    private void Awake()
    {
        owner = GameObject.FindWithTag("Player").GetComponent<PlayerHooker>();
    }

    public override void CreatePool(PooledObject prefab, int size, int capacity)
    {
        this.prefab = prefab;
        this.size = size;
        this.capacity = capacity;

        objectPool = new Stack<PooledObject>(capacity);

        for (int i = 0; i < size; i++)
        {
            Hook instance = Instantiate(prefab) as Hook;

            instance.gameObject.SetActive(false);
            instance.Pooler = this;
            instance.transform.parent = transform;

            HookInitialSetting(ref instance);

            objectPool.Push(instance);
        }
    }

    public override PooledObject GetPool(Vector3 position, Quaternion rotation)
    {
        if (objectPool.Count > 0)
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
            Hook instance = Instantiate(prefab) as Hook;
            instance.Pooler = this;
            instance.transform.position = position;
            instance.transform.rotation = rotation;

            HookInitialSetting(ref instance);

            return instance;
        }
    }

    private void HookInitialSetting(ref Hook inst)
    {
        // assign player rigidbody2D for DistanceJoint2D
        inst.OwnerRigid = owner.Rigid;
        inst.TrailSpeed = owner.HookShootPower;
        inst.MaxDistance = owner.MaxRopeLength;

        // hook action setting
        inst.OnDestroyHook += owner.OnHookDisJointed;
        inst.OnHookHitObject += owner.OnHookHitObject;
        inst.OnHookHitGround += owner.OnHookHitGround;
    }
}
