using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseScene : MonoBehaviour
{
    public abstract IEnumerator LoadingRoutine();
}
