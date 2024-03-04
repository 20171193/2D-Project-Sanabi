using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabable
{
    public void Grabbed();
    public void GrabEnd();

    public Vector3 GetGrabPosition();
    public GameObject GetObject();
}
