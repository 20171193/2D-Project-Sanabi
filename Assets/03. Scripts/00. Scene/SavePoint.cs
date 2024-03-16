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
        //gameData.sceneIndex = SceneManager.GetActiveScene().buildIndex;
        gameData.saveType = SaveType.GameFlow;
        gameData.startPos = playerRespawnTr.position;
        
    }

    public void SaveData()
    {
        gameData.confiner = Manager.Camera.CurrentConfiner;
        Manager.Data.SaveData(gameData);
    }

    public void OnSaveData()
    {
        anim.SetTrigger("OnActive");
        SaveData();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            OnSaveData();
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
    }

}