using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HUD_HP : PlayerHUD
{
    [Header("Specs")]
    [SerializeField]
    private float disappearTime;

    private Coroutine disappearRoutine;

    public void OnDamaged(int hpValue)
    {
        // 荤噶贸府
        if (hpValue <= 0)
        {
            base.DisableHUD();
            return;
        }

        if (disappearRoutine != null)
            StopCoroutine(disappearRoutine);

        anim.Play($"Damaged4_{hpValue}");
        disappearRoutine = StartCoroutine(DisappearRoutine());
    }
    public void OnRestore(int hpValue)
    {
        // 抗寇贸府
        if (hpValue >= 5) return;

        if (disappearRoutine != null)
            StopCoroutine(disappearRoutine);

        anim.Play($"Restore4_{hpValue}");
        disappearRoutine = StartCoroutine(DisappearRoutine());
    }

    IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(disappearTime);
        anim.SetTrigger("OnDisappear");
    }
}
