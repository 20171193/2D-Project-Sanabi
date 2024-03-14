using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private void OnEnable()
    {
        // Cursor Setting
        Cursor.visible = false;
    }
}
