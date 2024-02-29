using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void OnEnable()
    {
        // Cursor Setting
        Cursor.lockState = CursorLockMode.Locked;
    }
}
