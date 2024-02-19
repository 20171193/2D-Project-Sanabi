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

    // AnyState �߰�
    // � ���¿����� ���� ����
    public void AddAnyState(string key, Func<bool> condition)
    {
        anyStateTransition.Add(new Transition(key, condition));
    }

    // Ʈ������ �߰�
    public void AddTransition(string start, string end, Func<bool> condition)
    {
        stateDic[start].Transitions.Add(new Transition(end, condition));
    }

    // �ʱ� ���� ����
    public void Init(string entry)
    {
        curState = entry;
        stateDic[entry].Enter();
    }

    // ���� ����
    public void ChangeState(string nextState)
    {
        stateDic[curState].Exit();
        curState = nextState;
        stateDic[curState].Enter();
    }

    public void Update()
    {
        stateDic[curState].Update();

        // �켱���� 1
        // AnyState Ȯ��
        foreach (var transition in anyStateTransition)
        {
            if (transition.condition())
            {
                Debug.Log("AnyState Change : " + transition.end);
                ChangeState(transition.end);
                return;
            }
        }

        // ���� ���� Ȯ��
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