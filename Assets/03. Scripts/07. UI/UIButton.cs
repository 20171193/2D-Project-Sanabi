using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{
    public void OnClickStartButton()
    {
        Manager.Scene.LoadScene("GameScene_1");
    }
    public void OnClickOptionButton()
    {

    }
    public void OnClickQuitButton()
    {
        Application.Quit();
    }
}
