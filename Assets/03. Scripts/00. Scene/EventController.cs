using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum DeathType
{
    DeadZone,
    Damaged
}

public class EventController : MonoBehaviour
{
    [SerializeField]
    private DeathEvent deathByDeadzone;
    [SerializeField]
    private DeathEvent deathByDamaged;

    [SerializeField]
    private Image fadeImage;

    [SerializeField]
    private float fadeTime;
    public float FadeTime { get { return fadeTime; } set { fadeTime = value; } }


    public void EnableDeathEvent(DeathType type)
    {
        switch (type)
        {
            case DeathType.DeadZone:
                deathByDeadzone.EnableAnimator();
                break;
            case DeathType.Damaged:
                deathByDamaged.EnableAnimator();
                break;
            default:
                Debug.Log("Error(EnableDeathEvent) :  Death 타입이 설정되지 않았습니다.");
                return;
        }
    }

    public void DisableDeathEvent(DeathType type)
    {
        switch (type)
        {
            case DeathType.DeadZone:
                deathByDeadzone.DisableAnimator();
                break;
            case DeathType.Damaged:
                deathByDamaged.DisableAnimator();
                break;
            default:
                Debug.Log("Error(DisableDeathEvent) :  Death 타입이 설정되지 않았습니다.");
                return;
        }
    }

    public void FadeOut()
    {
        StartCoroutine(FadeRoutine(false));
    }
    public void FadeIn()
    {
        StartCoroutine(FadeRoutine(true));
    }

    IEnumerator FadeRoutine(bool isFadeIn)
    {
        float fadeSpeed = 1 / fadeTime;

        fadeImage.enabled = true;
        if (isFadeIn)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            while(fadeImage.color.a < 1)
            {
                fadeImage.color = new Color(0, 0, 0, fadeImage.color.a + fadeSpeed * Time.deltaTime);
                yield return null;
            }
            fadeImage.color = new Color(0, 0, 0, 1);
        }
        else
        {
            fadeImage.color = new Color(0, 0, 0, 1);
            while (fadeImage.color.a > 0)
            {
                fadeImage.color = new Color(0, 0, 0, fadeImage.color.a - fadeSpeed * Time.deltaTime);
                yield return null;
            }
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        fadeImage.enabled = false;
        yield return null;
    }
}

