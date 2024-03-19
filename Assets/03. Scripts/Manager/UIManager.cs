using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private Slider bgmSlider;
    [SerializeField]
    private Slider sfxSlider;

    [SerializeField]
    private GameObject mainPopUp;
    [SerializeField]
    private GameObject optionPopUp;
    [SerializeField]
    private GameObject currentPopUp;
    public void OnPause()
    {
        Time.timeScale = 0f;
        mainPopUp.SetActive(true);
        currentPopUp = mainPopUp;
    }
    public void OffPause()
    {
        mainPopUp.SetActive(false);
        optionPopUp.SetActive(false);
        currentPopUp = null;
    }

    public void OnClickOptionExit()
    {
        optionPopUp.SetActive(false);
        mainPopUp.SetActive(true);
        currentPopUp = mainPopUp;
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }
    public void OnClickOptionButton()
    {
        mainPopUp.SetActive(false);
        optionPopUp.SetActive(true);
        currentPopUp = optionPopUp;
    }
    public void OnClickContinueButton()
    {
        OffPause();

        Time.timeScale = 1f;
    }

    public void OnBGMControll()
    {
        Manager.Sound.BGMGroup.audioMixer.SetFloat("BGM", bgmSlider.value);
    }
    public void OnSFXControll()
    {
        Manager.Sound.SFXGroup.audioMixer.SetFloat("SFX", sfxSlider.value);
    }
    
}
