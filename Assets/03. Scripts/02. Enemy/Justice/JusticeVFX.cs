using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JusticeVFX : MonoBehaviour
{
    [Header("Pooling Option")]
    [SerializeField]
    private int size;
    public int Size { get { return size; } }

    [SerializeField]
    private string vfxName;
    public string VFXName { get { return vfxName; } set { vfxName = value; } }

    [SerializeField]
    private JusticeVFXPooler pooler;
    public JusticeVFXPooler Pooler { get { return pooler; } set { pooler = value; } }

    [SerializeField]
    private Animator anim;

    private void OnEnable()
    {

    }
    private void OnDisable()
    {

    }

    public void Release()
    {
        //pooler.ReturnPool(this.gameObject);
    }

}
