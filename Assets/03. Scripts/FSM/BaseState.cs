using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// node transition
// stateDic의 key값으로 동작
public struct Transition
{
    public string end;
    public Func<bool> condition;
    public float exitTime;  // animation Has Exit Time

    public Transition(string end, Func<bool> condition, float exitTime)
    {
        this.end = end;
        this.condition = condition;
        this.exitTime = exitTime;
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

