using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JusticeWeakness : MonoBehaviour
{
    [SerializeField]
    private Transform justiceTr;

    [SerializeField]
    private GameObject[] weaknessOb;

    [Header("Balancing")]
    [SerializeField]
    private bool isActive = false;

    private void Update()
    {
        Rotation();    
    }

    private void Rotation()
    {
        Vector3 dirToJustice = (justiceTr.transform.position - transform.position).normalized;
        transform.up = dirToJustice;
    }
}
