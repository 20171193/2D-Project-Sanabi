using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<TOwner>
{
    private TOwner owner;
    public TOwner Owner { get { return owner; } }

    private string curState;
    public string CurState { get { return curState; } }

    private Dictionary<string, BaseState> stateDic;
    public Dictionary<string, BaseState> StateDic { get { return stateDic; } }  

    private List<Transition> anyStateTransition;

    public StateMachine(TOwner owner)
    {
        this.owner = owner;
        stateDic = new Dictionary<string, BaseState>();
        anyStateTransition = new List<Transition>();
    }

    public void AddState(string key, BaseState value)
    {
        stateDic.Add(key, value);
    }

    // AnyState 추가
    // 어떤 상태에서든 전이 가능
    public void AddAnyState(string key, Func<bool> condition)
    {
        anyStateTransition.Add(new Transition(key, condition, 0f));
    }

    // 트랜지션 추가
    public void AddTransition(string start, string end, float exitTime, Func<bool> condition)
    {
        stateDic[start].Transitions.Add(new Transition(end, condition, exitTime));
    }

    // 초기 상태 지정
    public void Init(string entry)
    {
        curState = entry;
        stateDic[entry].Enter();
    }

    // 상태 전이
    public void ChangeState(string nextState)
    {
        stateDic[curState].Exit();
        curState = nextState;
        stateDic[curState].Enter();
    }

    public void Update()
    {
        stateDic[curState].Update();

        // 우선순위 1
        // AnyState 확인
        foreach (var transition in anyStateTransition)
        {
            if (transition.condition() && transition.end != curState)
            {
                Manager.Coroutine.StartCoroutine(HasExitTime(transition));
                return;
            }
        }

        // 상태 전이 확인
        foreach (var transition in stateDic[curState].Transitions)
        {
            if (transition.condition())
            {
                Manager.Coroutine.StartCoroutine(HasExitTime(transition));
                return;
            }
        }
    }
    public void LateUpdate()
    {
        stateDic[curState].LateUpdate();
    }
    public void FixedUpdate()
    {
        stateDic[curState].FixedUpdate();
    }

    IEnumerator HasExitTime(Transition transition)
    {
        yield return new WaitForSeconds(transition.exitTime);
        ChangeState(transition.end);
    }
}