using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Cinemachine;

public class PlayerBase : MonoBehaviour
{
    [SerializeField]
    protected Player player;
    public Player Player { get { return player; } } 

    protected virtual void Awake()
    {
        player = GetComponent<Player>();
    }
}
