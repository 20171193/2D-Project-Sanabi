using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().transform.position = new Vector3(2, 1, 0);
        }
    }
}
