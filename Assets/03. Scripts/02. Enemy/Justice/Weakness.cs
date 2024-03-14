using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Events;

public class Weakness : MonoBehaviour, IHookAttackable
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Transform justiceTr;
    public Transform JusticeTr { get { return justiceTr; } }

    [SerializeField]
    private Animator anim;
    public Animator Anim { get { return anim; } }

    [SerializeField]
    private CapsuleCollider2D capCol;
    public CapsuleCollider2D CapCol { get { return capCol; } }

    [Space(3)]
    [Header("Unity Action")]
    [Space(2)]
    public UnityAction OnHitted;
    public UnityAction OnDestroyed;

    [Space(3)]
    [Header("Balancing")]
    [Space(2)]
    private bool isActive = false;
    public bool IsActive { get { return isActive; } set { isActive = value; } }

    // 유한상태머신
    private StateMachine<Weakness> fsm;
    public StateMachine<Weakness> FSM { get { return fsm; } }

    private void Awake()
    {
        fsm = new StateMachine<Weakness>(this);

        fsm.AddState("Default", new Default());
        fsm.AddState("Appear", new Appear(this));
        fsm.AddState("DisAppear", new DisAppear(this));
        fsm.AddState("Active", new Active(this));
        fsm.AddState("Idle", new Idle(this));
        fsm.AddState("Destroy", new Destroy(this));

        fsm.Init("Default");
    }

    private void Update()
    {
        fsm.Update();
    }
    private void FixedUpdate()
    {
        
    }

    public void Hitted()
    {
        fsm.ChangeState("Destroy");
        OnHitted?.Invoke();
    }
}

