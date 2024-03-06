using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGhostTrailer : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem ghostTrailing;

    private Coroutine ghostTrailRoutine;

    public void OnActiveGhostTrailing(float time)
    {
        if (ghostTrailing != null)
            StopCoroutine(ghostTrailRoutine);

        StartCoroutine(GhostTrailRoutine(time));
    }

    IEnumerator GhostTrailRoutine(float time)
    {

        yield return new WaitForSeconds(time);

    }
}
