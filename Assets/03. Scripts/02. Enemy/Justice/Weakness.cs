using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weakness : MonoBehaviour
{
    [SerializeField]
    private Transform justiceTr;

    void Update()
    {
        Rotation();
    }

    private void Rotation()
    {
        Vector3 lookDir = (justiceTr.position - transform.position).normalized;
        transform.up = lookDir;
    }
}

