using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JusticeAnimator : MonoBehaviour
{
    private Justice owner;
    private CinemachineImpulseSource impulse;

    private void Awake()
    {
        owner = transform.parent.GetComponent<Justice>();
        impulse = GetComponent<CinemachineImpulseSource>();
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
