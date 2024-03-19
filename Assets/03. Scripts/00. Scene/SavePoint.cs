using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint : MonoBehaviour, ISaveable
{
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Transform playerRespawnTr;
    [SerializeField]
    private AudioSource triggerAudio;

    [SerializeField]
    private SaveType saveType;

    [SerializeField]
    private GameChapterController gameChapterController;

    [SerializeField]
    private BossChapterController bossChapterController;

    private GameData gameData;

    private bool isSave = false;
    

    private void Awake()
    {
        triggerAudio= GetComponent<AudioSource>();
        gameData = new GameData();
        //gameData.sceneIndex = SceneManager.GetActiveScene().buildIndex;
        gameData.saveType = saveType;
        gameData.startPos = playerRespawnTr.position;
        gameData.savePoint = this;
    }

    private void Start()
    {

    }

    public void OnPlayerRespawn()
    {
        // 현재 챕터 실행
        gameChapterController?.EnableChapter(gameData.phaseNumber);

        // 현재 보스 페이즈 실행
        bossChapterController?.EnableChapter(gameData.phaseNumber);
    }
    public void SaveData()
    {
        gameData.confiner = Manager.Camera.CurrentConfiner;
        Manager.Data.SaveData(gameData);
    }
    public void OnSaveData(int phaseNumber = 0)
    {
        gameData.phaseNumber = phaseNumber;
        anim.SetTrigger("OnActive");
        SaveData();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !isSave)
        {
            triggerAudio?.Play();
            OnSaveData();
            isSave = true;
        }
    }

}