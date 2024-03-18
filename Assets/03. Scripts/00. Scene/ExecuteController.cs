using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class ExecuteController : MonoBehaviour
{
    [SerializeField]
    private Justice justice;

    [SerializeField]
    private Player player;

    [SerializeField]
    private CinemachineVirtualCamera cam;

    [SerializeField]
    private GameObject buttonClickHUD;
    [SerializeField]
    private GameObject buttonHoldHUD;
    [SerializeField]
    private Image buttonFillImage;

    private int currentPhase = 1;
    private Coroutine increaseTimer;
    private Coroutine decreaseTimer;

    private void Awake()
    {
        player.OnClickUp += OnClickUp;
        player.OnClickDown += OnClickDown;
    }

    public void EnterCurrentPhase()
    {
        if (currentPhase == 1)
            buttonClickHUD.SetActive(true);
        else
            buttonHoldHUD.SetActive(true);

        if (player.PrFSM.FSM.CurState == "Execute") return;

        Manager.Camera.SetCameraPriority(CameraType.CutScene, cam);

        player.Rigid.velocity = Vector3.zero;
        player.PrFSM.ChangeState("Execute");
    }

    public void OnEndCurrentPhase()
    {
        if (currentPhase == 1)
        {
            buttonClickHUD.SetActive(false);
            player.Anim.Play("QTEA_Start");
        }
        else if (currentPhase == 2)
        {
            buttonHoldHUD.SetActive(false);
            player.Anim.Play("QTEB_Start");
        }
        else 
            return;
        currentPhase++;
        StartCoroutine(Extension.DelayRoutine(2f, () => EnterCurrentPhase()));
    }

    public void PlayCurrentPhase()
    {
        Time.timeScale = 0.5f;
        StartCoroutine(Extension.DelayRoutine(0.5f, () => Time.timeScale = 1f));
        StartCoroutine(Extension.DelayRoutine(2f, () => OnEndCurrentPhase()));

        if (currentPhase == 1)
        {
            player.Anim.Play("ExcecuteJusticeBash");
            justice.Anim.SetTrigger("OnKnockDown");
        }
        else if(currentPhase == 2)
        {
            player.Anim.SetTrigger("OnQTEA");
            justice.Anim.Play("LastStanding_GroggyStart");
        }
        else if(currentPhase == 3)
        {
            player.Anim.SetTrigger("OnQTEB");
            Manager.Scene.FadeTime = 1.5f;
            Manager.Scene.LoadScene("Title");
        }
    }

    public void OnClickUp()
    {
        if (currentPhase == 1)
        {

            return;
        }
        else
        {
            Debug.Log("OnClickUp");
            if (increaseTimer != null)
                StopCoroutine(increaseTimer);

            decreaseTimer = StartCoroutine(DecreaseTimer());
        }
    }
    public void OnClickDown()
    {
        if(currentPhase == 1)
        {
            PlayCurrentPhase();
            return;
        }
        else
        {
            if (decreaseTimer != null)
                StopCoroutine(decreaseTimer);

            increaseTimer = StartCoroutine(IncreaseTimer());
        }        
    }

    IEnumerator IncreaseTimer()
    {
        while(buttonFillImage.fillAmount < 1f)
        {
            buttonFillImage.fillAmount += Time.deltaTime*currentPhase/10;
            yield return null;
        }
        buttonFillImage.fillAmount = 1f;
        yield return new WaitForSeconds(0.3f);
        buttonHoldHUD.SetActive(false);
        PlayCurrentPhase();
        yield return null;
    }
    IEnumerator DecreaseTimer()
    {
        while (buttonFillImage.fillAmount > 0f)
        {
            buttonFillImage.fillAmount -= Time.deltaTime*2f;
            yield return null;
        }
        buttonFillImage.fillAmount = 0f;
        yield return null;
    }
}
