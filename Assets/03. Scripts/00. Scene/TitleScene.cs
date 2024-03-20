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
        yield return null;
    }

    private void Update()
    { 

    }

    private void OnEnable()
    {
        Cursor.visible = true;
    }
    private void OnDisable()
    {
        Cursor.visible = false;
        Manager.Sound.UnPlaySound(SoundType.BGM);
    }
}
