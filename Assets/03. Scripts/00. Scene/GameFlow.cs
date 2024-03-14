using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    // 현재 씬의 Confiner모음
    [SerializeField]
    private List<Collider2D> confinerList;
    [SerializeField]
    private int curIndex = 0;

    private void Start()
    {
        Manager.Camera.SetConfiner(confinerList[curIndex]);
    }

    public void SetNextConfiner()
    {
        if (curIndex + 1 >= confinerList.Count) return;
        Manager.Camera.SetConfiner(confinerList[++curIndex]);
    }
    public void SetPrevConfiner()
    {
        if (curIndex == 0) return;
        Manager.Camera.SetConfiner(confinerList[--curIndex]);
    }
}
