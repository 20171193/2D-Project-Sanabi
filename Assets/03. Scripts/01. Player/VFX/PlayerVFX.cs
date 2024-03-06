using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField]
    private PlayerVFXPooler pooler;
    public PlayerVFXPooler Pooler { get { return pooler; } set { pooler = value; } }
    
    [SerializeField]
    private Animator anim;

    private void OnEnable()
    {
        anim.enabled = true;
    }
    private void OnDisable()
    {
        anim.enabled = false;
    }

    public void Release()
    {
        pooler.ReturnPool(this.gameObject);
    }
}
