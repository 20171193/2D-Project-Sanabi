using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestManager : MonoBehaviour
{
    public UnityEvent OnTestEvent;


    public void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().transform.position = new Vector3(2, 1, 0);
        }
        if(Input.GetKey(KeyCode.Y))
        {
            OnTestEvent.Invoke();
        }
    }
}
