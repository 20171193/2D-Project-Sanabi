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
        impulse = GameObject.FindWithTag("Player").GetComponent<CinemachineImpulseSource>();
    }

    public void OnAnimationBeforeBattle()
    {
        owner.SetBeforeBattlePos();
    }
    public void OnAnimationBattle()
    {
        owner.SetBattlePos();
    }
    public void OnAnimationBattle2()
    {
        owner.EnableObjects();
    }
    public void OnDoImpulse()
    {
        impulse.GenerateImpulse();
    }
}
