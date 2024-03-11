using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaknessController : MonoBehaviour
{
    [Header("Justice")]
    [Space(2)]
    [SerializeField]
    private Transform justiceTr;

    [Space(3)]
    [Header("Weakness")]
    [Space(2)]
    [SerializeField]
    private Weakness[] weaknesses;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private float rotSpeed;
    public float RotSpeed { get { return rotSpeed; } set { rotSpeed = value; } }

    [Space(3)]
    [Header("Balancing")]
    [Space(2)]
    [SerializeField]
    private bool isActive = false;

    // 유한상태머신
    private StateMachine<WeaknessController> fsm;
    public StateMachine<WeaknessController> FSM { get { return fsm; } }


    private void Start()
    {
        // 약점 오브젝트 초기위치 지정
        for (int i = 0; i < weaknesses.Length; i++)
        {
            Vector3 rot = new Vector3(0, 0, 360 / 3 * i);
            weaknesses[i].transform.Rotate(rot);
            weaknesses[i].transform.Translate(weaknesses[i].transform.up * 5);
        }

    }
    private void Update()
    {
        Rotation();
    }

    private void Rotation()
    {
        transform.Rotate(new Vector3 (0, 0, rotSpeed * Time.deltaTime));
    }
}
