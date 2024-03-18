using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class GameChapter : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private GameObject doorObject;

    [SerializeField]
    private Animator doorAnim;

    [SerializeField]
    private CinemachineVirtualCamera cutSceneCamera;
    public CinemachineVirtualCamera CutSceneCamera { get { return cutSceneCamera; } }

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private int chapterPhase;
    public int ChapterPhase { get { return chapterPhase; } }

    [Space(3)]
    [Header("Chapter Enemy Spawner")]
    [Space(2)]
    [SerializeField]
    private Spawner[] spawnerArray;

    [SerializeField]
    private int destroyedSpawnerCount;

    public UnityAction OnDestroySpawner;

    [Space(3)]
    [Header("Chapter Exit Event")]
    [Space(2)]
    public UnityAction<GameChapter> OnClearChapter;
    public UnityEvent OnChapterExit;

    private void Awake()
    {
        // 모든 spawner가 파괴되면 챕터 클리어
        destroyedSpawnerCount = spawnerArray.Length;

        for(int i =0; i< spawnerArray.Length; i++)
        {
            spawnerArray[i].OnDestroySpawner += CountDestroySpawner;
        }
    }
    public void OnEnable()
    {

    }

    public void CountDestroySpawner()
    {
        destroyedSpawnerCount--;

        // 모든 Spawner가 파괴된 경우 문 열기
        if (destroyedSpawnerCount < 1)
        {
            OnClearChapter?.Invoke(this);
            doorAnim.SetBool("IsEnable", false);
        }
    }

    public void EnterChapter()
    {
        // 챕터 입장 시 Spawner 활성화
        foreach (Spawner spawner in spawnerArray)
        {
            spawner.gameObject.SetActive(true);
            spawner.EnableSpawner();
        }

        doorAnim.SetBool("IsEnable", true);
    }
    public void ExitChapter()
    {
        // 애니메이션 출력, 데이터 저장 등
        OnChapterExit?.Invoke();
    }
}
