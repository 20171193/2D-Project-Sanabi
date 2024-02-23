using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Android;


/*****************************************
 * <Status by Enemy Type>
 * <Common>
 * {
 *  - Idle
 *  - Grabbed
 *  - Die
 *  <Activable>
 *  { 
 *   - Detect
 *      [Turret]
 *      {
 *       - Attack
 *      }
 *      [Soldier]
 *      {
 *       - Attack
 *       - Patroll
 *      }
 *      [Defender]
 *      {
 *       - Defence
 *      }
 *  }
 * }
******************************************/
public class EnemyBaseState : BaseState
{
    protected Enemy owner;
}

#region CommonState
// Grabbed, Detect, Die
public class EnemyGrabbed : EnemyBaseState
{
    public EnemyGrabbed(Enemy owner)
    {
        this.owner = owner;
    }
}

public class EnemyDie : EnemyBaseState
{
    public EnemyDie(Enemy owner)
    {
        this.owner = owner;
    }
}

#region extra State
// Attack, Patroll, Defence
public class EnemyAttack : EnemyBaseState
{
    public EnemyAttack(Enemy owner)
    {
        this.owner = owner;
    }
}
public class EnemyPatroll : EnemyBaseState
{
    private float patrollSpeed = 0f;
    private Vector3 patrollDestination = Vector3.zero;

    public EnemyPatroll(Enemy owner)
    {
        this.owner = owner;
        if (owner is IPatrollable)
        {
            IPatrollable agent = (IPatrollable)owner;
            patrollSpeed = agent.GetSpeed();
            patrollDestination = agent.GetDestination();
        }
        else
            Debug.Log($"error :[{owner}] EnemyPatrollState //isn't patrollable object");
    }

    public override void Enter()
    {
        
    }
    public override void Update()
    {
        Patrolling();
    }

    private void Patrolling()
    {

    }
}
public class EnemyDetect : EnemyBaseState
{

}
public class EnemyDefence : EnemyBaseState
{
    public EnemyDefence(Enemy owner)
    {
        this.owner = owner;
    }
}
#endregion

#endregion