using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePoint : MonoBehaviour, ISaveable
{
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Transform playerRespawnTr;

    private GameData gameData;

    private void Start()
    {
        gameData = new GameData();
        gameData.sceneIndex = SceneManager.GetActiveScene().buildIndex;
        gameData.saveType = SaveType.GameFlow;
        gameData.startPos = playerRespawnTr.position;
        gameData.confiner = Camera.main.GetComponent<MainCameraController>().Confiner.m_BoundingShape2D;
    }

    public void SaveData()
    {
        Manager.Data.SaveData(gameData);
    }

    public void OnSaveData()
    {
        anim.SetTrigger("OnActive");
        SaveData();
    }
}
