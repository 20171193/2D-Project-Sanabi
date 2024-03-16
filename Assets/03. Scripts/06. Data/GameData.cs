using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SaveType
{
    GameFlow,
    BossBattle
}
public class GameData
{
    [SerializeField]
    public int sceneIndex;

    [SerializeField]
    public SaveType saveType;

    [SerializeField]
    public Collider2D confiner;

    [SerializeField]
    public Vector3 startPos;
}
