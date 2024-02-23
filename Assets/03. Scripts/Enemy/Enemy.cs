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

    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    protected StateMachine<Enemy> fsm;
    public StateMachine<Enemy> FSM { get { return fsm; } }

    private void Awake()
    {
        fsm = new StateMachine<Enemy>(this);

        fsm.AddState("Grabbed", new EnemyGrabbed(this));
        fsm.AddState("Die", new EnemyDie(this));

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
