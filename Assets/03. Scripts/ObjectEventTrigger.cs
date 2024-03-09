using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Cinemachine;
public class ObjectEventTrigger : MonoBehaviour
{
    [SerializeField]
    private LayerMask targetLM;

    [SerializeField]
    private bool playOnce;

    [SerializeField]
    private CinemachineVirtualCamera eventCamera;
    [SerializeField]
    private float cameraActionTime;
    private Coroutine cameraActionRoutine;

    public UnityEvent OnEnterTrigger;
    public UnityEvent OnExitTrigger;

    public void OnCameraAction()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerInput>().enabled s= false;
    }

    

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
