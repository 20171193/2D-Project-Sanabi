using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabMoveable : IGrabable
{
    public void GrabMove(Rigidbody2D ownerRigid);
}
