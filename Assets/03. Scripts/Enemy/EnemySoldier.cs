using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemySoldier : Enemy, IPatrollable
{
    [Header("Specs")]
    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private Vector3 patrollDestination;
    //[Header("Ballancing")]

    private void Awake()
    {
        fsm.AddState("Patroll", new EnemyPatroll(this));
        fsm.AddState("Atttack", new EnemyAttack(this));

        // test
        fsm.Init("Patroll");
    }

    public float GetSpeed()
    {
        return moveSpeed;
    }
    public Vector3 GetDestination()
    {
        return patrollDestination;
    }
}