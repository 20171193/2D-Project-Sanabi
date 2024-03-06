using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [Header("Pooling Option")]
    [SerializeField]
    private int size;
    public int Size { get { return size; }}

    [SerializeField]
    private string vfxName;
    public string VFXName { get { return vfxName; } set { vfxName = value; } }

    [SerializeField]
    private PlayerVFXPooler pooler;
    public PlayerVFXPooler Pooler { get { return pooler; } set { pooler = value; } }
    
    [SerializeField]
    private Animator anim;

    private void OnEnable()
    {
        if(anim != null)
            anim.enabled = true;
    }
    private void OnDisable()
    {
        if(anim != null)
            anim.enabled = false;
    }

    public void Release()
    {
        pooler.ReturnPool(this.gameObject);
    }
}
