using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD_HP : PlayerHUD
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
