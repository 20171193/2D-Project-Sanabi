using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    [SerializeField]
    private GameData currentData;

    public void SaveData(GameData saveData)
    {
        currentData = saveData;
    }

    public GameData LoadCurrentData()
    {
        return currentData;
    }
}
