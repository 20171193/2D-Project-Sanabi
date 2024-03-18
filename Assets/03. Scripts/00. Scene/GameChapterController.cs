using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GameChapterController : MonoBehaviour
{
    [SerializeField]
    private SavePoint chapterSavePoint;

    [SerializeField]
    private ObjectEventTrigger chapterChangeEvent;

    [SerializeField]
    private GameObject[] chapterArray;

    private void Awake()
    {
        foreach(GameObject chapter in chapterArray)
        {
            chapter.GetComponent<GameChapter>().OnClearChapter += OnClearChapter;
        }
    }

    public void OnClearChapter(GameChapter curChapter)
    {
        int nextPhaseNumber = curChapter.ChapterPhase + 1;

        // 다음 챕터가 있을 경우
        if (nextPhaseNumber < chapterArray.Length)
        {
            chapterChangeEvent.gameObject.SetActive(true);
            // 세이브 포인터에 데이터 저장
            chapterSavePoint.OnSaveData(curChapter.ChapterPhase + 1);
            // 다음 챕터 활성화
            EnableChapter(nextPhaseNumber);
        }
        else
        {
            // 없을 경우 챕터 엔딩 이벤트
            curChapter.ExitChapter();
        }
    }
    // 챕터 활성화
    public void EnableChapter(int phaseNumber)
    {
        // 모든 챕터를 비활성화
        ResetChapter();

        // 타깃 챕터 활성화
        chapterArray[phaseNumber].SetActive(true);
        GameChapter chapter = chapterArray[phaseNumber].GetComponent<GameChapter>();
        chapter.EnterChapter();
    }

    private void ResetChapter()
    {
        foreach (GameObject chapter in chapterArray)
            chapter.SetActive(false);
    }
}
