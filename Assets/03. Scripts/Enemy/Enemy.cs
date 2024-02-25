using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EnemyType
{
    Common,
    Activable
}

// Add Enemy common states, and connect transitions from the child classes
public abstract class Enemy : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    protected Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }

    [SerializeField]
    protected Animator anim;
    public Animator Anim { get { return anim; } }

    [SerializeField]
    protected Transform playerTr;
    public Transform PlayerTr { get { return playerTr; } }

    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    protected StateMachine<Enemy> fsm;
    public StateMachine<Enemy> FSM { get { return fsm; } }

    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    protected float grabbedYPos;
    public float GrabbedYPos { get { return grabbedYPos; } }

    protected virtual void Awake()
    {
        fsm = new StateMachine<Enemy>(this);
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();    
    }

    private void Update()
    {
        fsm.Update();
    }
    private void FixedUpdate()
    {
        fsm.FixedUpdate();
    }
    private void LateUpdate()
    {
        fsm.LateUpdate();
    }

    public abstract void Grabbed();
}
