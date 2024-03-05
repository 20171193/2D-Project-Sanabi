using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    protected LineRenderer lr;

    public virtual void LineRendering() { }
}
