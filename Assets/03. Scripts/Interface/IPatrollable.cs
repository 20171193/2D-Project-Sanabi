using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPatrollable
{
    public float GetSpeed();
    public Vector3 GetDestination();
}
