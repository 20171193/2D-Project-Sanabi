using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabable
{
    public void Grabbed(Rigidbody2D ownerRigid);
    public void GrabEnd();

    public Vector3 GetGrabPosition();
}
