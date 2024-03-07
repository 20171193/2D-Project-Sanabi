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
    [Header("Chapter Enemy Spawner")]
    [Space(2)]
    [SerializeField]
    private Spawner[] spawnerArray;
    private int destroyedSpawnerCount;

    public UnityAction OnDestroySpawner;

    [Space(3)]
    [Header("Chapter Exit Event")]
    [Space(2)]
    public UnityEvent OnChapterExit;

    private void Awake()
    {
        // ��� spawner�� �ı��Ǹ� é�� Ŭ����
        destroyedSpawnerCount = spawnerArray.Length;

        foreach (Spawner spawner in spawnerArray)
            spawner.OnDestroySpawner += CountDestroySpawner;
    }

    public void CountDestroySpawner()
    {
        destroyedSpawnerCount--;

        // ��� Spawner�� �ı��� ��� ���� é�ͷ� �̵�
        if (destroyedSpawnerCount < 1)
            ExitChapter();
    }

    public void EnterChapter()
    {
        // é�� ���� �� Spawner Ȱ��ȭ
        foreach (Spawner spawner in spawnerArray)
            spawner.EnableSpawner();
    }

    public void ExitChapter()
    {
        // �ִϸ��̼� ���
        OnChapterExit?.Invoke();
    }
}
