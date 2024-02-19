using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private string curState;
    public string CurState { get { return curState; } }

    private Dictionary<string, BaseState> stateDic;
    private List<Transition> anyStateTransition;

    public StateMachine()
    {
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
        anyStateTransition.Add(new Transition(key, condition));
    }

    // 트랜지션 추가
    public void AddTransition(string start, string end, Func<bool> condition)
    {
        stateDic[start].Transitions.Add(new Transition(end, condition));
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
            if (transition.condition())
            {
                Debug.Log("AnyState Change : " + transition.end);
                ChangeState(transition.end);
                return;
            }
        }

        // 상태 전이 확인
        foreach (var transition in stateDic[curState].Transitions)
        {
            if (transition.condition())
            {
                Debug.Log("Transitions Change : " + transition.end);
                ChangeState(transition.end);
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
}