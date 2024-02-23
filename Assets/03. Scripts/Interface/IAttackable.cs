using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackable
{
    public float GetAttackCoolTime();
    public void Attack();
}
