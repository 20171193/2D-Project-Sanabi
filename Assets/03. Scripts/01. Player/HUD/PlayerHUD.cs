using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    protected Animator anim;
    [SerializeField]
    protected SpriteRenderer spr;

    [Space(3)]
    [Header("Auto Release Setting")]
    [Space(2)]
    [SerializeField]
    private bool isAutoRelease;
    [SerializeField]
    private float autoReleaseTime;

    private Coroutine releaseRoutine;

    public virtual void EnableHUD()
    {
        SetEnable(true);
    }

    public virtual void DisableHUD()
    {
        // isAutoRelease �� true�� ���, autoReleaseTime��ŭ�� �ڷ�ƾ�� ������ ��(������) hud�� ��.
        if (isAutoRelease)
            releaseRoutine = StartCoroutine(ReleaseRoutine());
        // false�� ���, �ٷ� hud�� ��.
        else
            SetEnable(false);
    }

    protected virtual void SetEnable(bool isEnable)
    {
        anim.enabled = isEnable;
        spr.enabled = isEnable;
    }

    IEnumerator ReleaseRoutine()
    {
        yield return new WaitForSeconds(autoReleaseTime);
        SetEnable(false);
    }
}
