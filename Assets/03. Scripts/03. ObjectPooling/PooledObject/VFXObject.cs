using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXObject : MonoBehaviour
{
    public void Release()
    {
        gameObject.SetActive(false);
    }
}
