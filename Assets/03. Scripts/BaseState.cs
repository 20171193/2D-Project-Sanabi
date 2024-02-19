using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// node transition
// stateDic�� key������ ����
public struct Transition
{
    public string end;
    public Func<bool> condition;

    public Transition(string end, Func<bool> condition)
    {
        this.end = end;
        this.condition = condition;
    }
}
public class BaseState
{
    protected List<Transition> transitions = new List<Transition>();
    public List<Transition> Transitions { get { return transitions; } }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}

