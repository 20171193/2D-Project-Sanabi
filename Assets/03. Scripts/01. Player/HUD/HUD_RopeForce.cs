using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_RopeForce : PlayerHUD
{
    public override void EnableHUD()
    {
        base.EnableHUD();
        anim.SetBool("IsEnable", true);
    }
    public override void DisableHUD()
    {
        base.DisableHUD();
        anim.SetBool("IsEnable", false);
    }
}
