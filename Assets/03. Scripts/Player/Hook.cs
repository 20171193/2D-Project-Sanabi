using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField]
    private Transform hookingPos;
    public Transform HookingPos { get { return hookingPos; } }
}
