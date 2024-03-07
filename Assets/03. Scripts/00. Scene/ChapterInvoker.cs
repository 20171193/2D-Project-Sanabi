using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChapterInvoker : MonoBehaviour
{
    public UnityEvent OnChapterInteract;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Interact Player");
            OnChapterInteract?.Invoke();

            gameObject.SetActive(false);
        }
    }
}
