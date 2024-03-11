using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JusticeVFX : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Animator anim;

    [Space(3)]
    [Header("Pooling Option")]
    [Space(2)]
    [SerializeField]
    private int size;
    public int Size { get { return size; } }

    [SerializeField]
    private string vfxName;
    public string VFXName { get { return vfxName; } set { vfxName = value; } }

    [Space(3)]
    [Header("Auto Release")]
    [Space(2)]
    [SerializeField]
    private float releaseTime;
    public float ReleaseTime { get { return releaseTime; } set { releaseTime = value; } }

    private Coroutine releaseRoutine;

    private JusticeVFXPooler pooler;
    public JusticeVFXPooler Pooler { get { return pooler; } set { pooler = value; } }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (releaseTime > 0)
            releaseRoutine = StartCoroutine(ReleaseRoutine());
    }
    private void OnDisable()
    {
        if (releaseRoutine != null)
            StopCoroutine(releaseRoutine);
    }

    public void Release()
    {
        pooler.InActiveVFX(this.gameObject);
    }
    IEnumerator ReleaseRoutine()
    {
        yield return new WaitForSeconds(releaseTime);
        pooler.InActiveVFX(this.gameObject);
    }
}
