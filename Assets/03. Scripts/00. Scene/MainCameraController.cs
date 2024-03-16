using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    [SerializeField]
    private CinemachineConfiner2D confiner;
    public CinemachineConfiner2D Confiner { get { return confiner; } }

    public void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();   
    }

    public void SetConfiner(PolygonCollider2D col)
    {
        confiner.m_BoundingShape2D = col;
    }
}
