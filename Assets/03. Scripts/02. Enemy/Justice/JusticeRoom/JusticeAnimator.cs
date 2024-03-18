using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JusticeAnimator : MonoBehaviour
{
    private Justice owner;
    private CinemachineImpulseSource impulse;
    private Coroutine bgmRoutine;
    private void Awake()
    {
        owner = transform.parent.GetComponent<Justice>();
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    public void OnChangeBGM()
    {
        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        Manager.Sound.PlaySound(SoundType.BGM, "BossScene");
    }
    public void StopBGM()
    {
        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);
        Manager.Sound.UnPlaySound(SoundType.BGM);
    }
    public void OnPlayMainBGM()
    {
        Manager.Sound.PlaySound(SoundType.BGM, "Boss_Intro");
        bgmRoutine = StartCoroutine(Extension.DelayRoutine(Manager.Sound.BGMDic["Boss_Intro"].length, () => Manager.Sound.PlaySound(SoundType.BGM, "Boss_Loop")));
    }
    public void OnPlayAMB()
    {
        Manager.Sound.PlaySound(SoundType.AMB, "Scene2_CarRail");
    }

    public void OnAnimationBattle()
    {
        owner.SetBattlePos();
    }
    public void OnAnimationBattle2()
    {
        owner.EnableObjects();
    }
    public void OnEndBattleMode()
    {
        owner.BattleModeEnd();
    }
    public void OnPhaseChangeMove()
    {
        owner.OnPhaseChangeMove();
    }
    public void OnPhaseChangeEnd()
    {
        owner.OnPhaseChangeEnd();
    }
    public void OnDoImpulse()
    {
        impulse.GenerateImpulse();
    }
}
