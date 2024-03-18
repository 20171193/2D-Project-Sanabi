using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Spawner : MonoBehaviour
{
    public UnityAction OnDestroySpawner;
    public abstract void EnableSpawner();
    public abstract void Spawn();
    public abstract void InitSpawner();

    public virtual void DisableSpawner() 
    {
        OnDestroySpawner?.Invoke();
        gameObject.SetActive(false);
    }
}
