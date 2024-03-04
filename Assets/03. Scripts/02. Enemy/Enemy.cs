using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EnemyType
{
    Shooter,
    Boss
}

// Add Enemy common states, and connect transitions from the child classes
public class Enemy : PooledObject
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
    protected Animator markerAnim;
    public Animator MarkerAnim { get { return markerAnim; } }

    [SerializeField]
    protected Transform playerTr;
    public Transform PlayerTr { get { return playerTr; } }

    [Header("FSM")]
    [Space(2)]
    [SerializeField]
    protected StateMachine<Enemy> fsm;
    public StateMachine<Enemy> FSM { get { return fsm; } }

    [SerializeField]
    protected string initState;
    public string InitState { get { return initState; } }

    public UnityAction OnDie;

    [Header("Ballancing")]
    [Space(2)]
    [SerializeField]
    protected float grabbedYPos;
    public float GrabbedYPos { get { return grabbedYPos; } }

    protected virtual void Awake()
    {
        fsm = new StateMachine<Enemy>(this);
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();

        fsm.AddState("Pooled", new EnemyPooled());
    }

    protected override void OnEnable()
    {
        fsm.ChangeState(initState);
        markerAnim.SetBool("IsEnable", true);
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

    protected void Died()
    {
        fsm.ChangeState("Die");
        StartCoroutine(EnemyReleaseRoutine());
    }
    IEnumerator EnemyReleaseRoutine()
    {
        yield return new WaitForSeconds(releaseTime);
        Release();
        OnDie?.Invoke();
        fsm.Init("Pooled");
    }
}
