using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScene : BaseScene
{
    [SerializeField]
    private Transform startPos;

    private void Awake()
    {
        Manager.Sound.PlaySound(SoundType.AMB, "Scene2_Rumble");
    }

    private void Start()
    {
        Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
        player.transform.position = startPos.position;
        player.PrFSM.ChangeState("Idle");
    }
    public override IEnumerator LoadingRoutine()
    {
        Manager.Camera.InitCameraSetting();
        yield return null;
    }

    private void OnDisable()
    {
        if(Manager.Sound.BGMSource != null)
            Manager.Sound.BGMSource.volume = 1f;
    }
}
