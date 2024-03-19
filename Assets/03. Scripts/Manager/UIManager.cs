using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private Slider bgmSlider;
    [SerializeField]
    private Slider sfxSlider;

    [SerializeField]
    private Canvas myCanvas;
    private GraphicRaycaster graphicRaycaster;

    [SerializeField]
    private AudioSource dummySFX;

    [SerializeField]
    private GameObject mainPopUp;
    [SerializeField]
    private GameObject optionPopUp;
    [SerializeField]
    private GameObject currentPopUp;

    private bool isPause = false;
    public bool IsPause { get { return isPause; } }

    private PlayerInput prInput;

    private void Start()
    {
        myCanvas.worldCamera = Camera.main;
        graphicRaycaster = myCanvas.GetComponent<GraphicRaycaster>();
    }

    public void OnPause(PlayerInput prInput = null)
    {
        this.prInput = prInput;

        if (isPause) 
        {
            OnClickContinueButton();
            return;
        }

        if(this.prInput)
            this.prInput.enabled = false;


        Cursor.visible = true;

        graphicRaycaster.enabled = true;
        isPause = true;
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

        if (this.prInput)
            this.prInput.enabled = true;

        Cursor.visible = false;
        graphicRaycaster.enabled = false;
        isPause = false;
        Time.timeScale = 1f;
    }

    public void OnBGMControll()
    {
        Manager.Sound.BGMGroup.audioMixer.SetFloat("BGM", bgmSlider.value);
        if (bgmSlider.value <= -39f) Manager.Sound.BGMGroup.audioMixer.SetFloat("BGM", -80f);
    }
    public void OnSFXControll()
    {
        // 더미 사운드 출력
        // dummySFX?.Play();

        Manager.Sound.SFXGroup.audioMixer.SetFloat("SFX", sfxSlider.value);
        if(sfxSlider.value <= -39f) Manager.Sound.SFXGroup.audioMixer.SetFloat("SFX", -80f);
    }
    
}
