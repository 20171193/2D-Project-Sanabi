using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [SerializeField]
    protected bool autoRelease;
    [SerializeField]
    protected float releaseTime;

    private ObjectPooler pooler;
    public ObjectPooler Pooler { get { return pooler; } set { pooler = value; } }

    protected virtual void OnEnable()
    {
        if (autoRelease)
            StartCoroutine(ReleaseRoutine());
    }

    private void Release()
    {
        if(!pooler)
        {
            Destroy(gameObject);
            return;
        }

        pooler?.ReturnPool(this);
    }


    protected IEnumerator ReleaseRoutine()
    {
        yield return new WaitForSeconds(releaseTime);
        Release();
    }

}
