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

        // ���� é�Ͱ� ���� ���
        if (nextPhaseNumber < chapterArray.Length)
        {
            chapterChangeEvent.gameObject.SetActive(true);
            // ���̺� �����Ϳ� ������ ����
            chapterSavePoint.OnSaveData(curChapter.ChapterPhase + 1);
            // ���� é�� Ȱ��ȭ
            EnableChapter(nextPhaseNumber);
        }
        else
        {
            // ���� ��� é�� ���� �̺�Ʈ
            curChapter.ExitChapter();
        }
    }
    // é�� Ȱ��ȭ
    public void EnableChapter(int phaseNumber)
    {
        // ��� é�͸� ��Ȱ��ȭ
        ResetChapter();

        // Ÿ�� é�� Ȱ��ȭ
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
