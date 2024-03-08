using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectEventTrigger : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLM;

    [SerializeField]
    private bool playOnce;

    public UnityEvent OnEnterTrigger;
    public UnityEvent OnExitTrigger;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(targetLM.Contain(collision.gameObject.layer))
        {
            OnEnterTrigger?.Invoke();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (targetLM.Contain(collision.gameObject.layer))
        {
            OnExitTrigger?.Invoke();

            if (playOnce)
                enabled = false;
        }
    }
}
