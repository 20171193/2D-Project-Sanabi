using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Spawner : MonoBehaviour
{
    public UnityAction OnDestroySpawner;
    public abstract void EnableSpawner();
    public abstract void Spawn();
    public virtual void DestroySpawner() 
    {
        OnDestroySpawner?.Invoke();
        Destroy(gameObject); 
    }
}
