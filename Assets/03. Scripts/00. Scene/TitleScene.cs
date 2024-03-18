using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : BaseScene
{
    private void Start()
    {
        Manager.Sound.PlaySound(SoundType.BGM, "Title_Loop");
    }
    public override IEnumerator LoadingRoutine()
    {
        throw new System.NotImplementedException();
    }

    private void Update()
    { 
    }
}
