using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Common,
    Activable
}

// Add Enemy common states, and connect transitions from the child classes
public class Enemy : MonoBehaviour
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
    protected PlayerAction prAction;
    public PlayerAction PrAction { get { return prAction; } }

    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    protected StateMachine<Enemy> fsm;
    public StateMachine<Enemy> FSM { get { return fsm; } }

    protected virtual void Awake()
    {
        fsm = new StateMachine<Enemy>(this);

        // find Player
        prAction = GameObject.FindWithTag("Player").GetComponent<PlayerAction>();
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
}
