using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossChapterController : MonoBehaviour
{
    [SerializeField]
    private Justice boss;

    [SerializeField]
    private Player player;

    [SerializeField]
    private SavePoint savePoint;

    private void Start()
    {
        savePoint.OnSaveData();
    }

    public void EnableChapter(int phaseNumber)
    {
        boss.nextPhaseIndex = phaseNumber;
        boss.LoadPhaseData();

        if (phaseNumber == 1)
            StartCoroutine(Extension.DelayRoutine(3f, () => boss.FSM.ChangeState("BattleMode")));
        else if (phaseNumber == 2)
            StartCoroutine(Extension.DelayRoutine(3f, () => boss.FSM.ChangeState("PhaseChange")));
    }

    public void OnPlayerDie()
    {
        // 보스 비활성화
        boss.FSM.ChangeState("OnDisable");

        // 맵 이동 중지
        boss.BossRoomController.IsRunning = false;
        // 일정시간 뒤 활성화
        StartCoroutine(Extension.DelayRoutine(3f, () => boss.BossRoomController.IsRunning = true));

    }


    public void OnPhaseChanged()
    {
        
    }
}
