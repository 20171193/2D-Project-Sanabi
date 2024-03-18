using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScene : BaseScene
{
    private void Awake()
    {
        Manager.Sound.PlaySound(SoundType.AMB, "Scene2_Rumble");
    }

    private void Start()
    {
        GameObject.FindWithTag("Player").GetComponent<Player>().PrFSM.ChangeState("Idle");
    }
    public override IEnumerator LoadingRoutine()
    {
        Manager.Camera.InitCameraSetting();
        yield return null;
    }
}
