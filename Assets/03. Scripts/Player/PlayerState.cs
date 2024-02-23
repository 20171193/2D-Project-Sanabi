using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerBaseState : BaseState
{
    protected PlayerAction owner;
}

#region Idle
public class PlayerIdle : PlayerBaseState
{
    public PlayerIdle(PlayerAction owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Idle");
    }
}
#endregion

#region Run
public class PlayerRun : PlayerBaseState
{
    public PlayerRun(PlayerAction owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Run");
    }

    public override void FixedUpdate()
    {
        Move();
    }
    private void Move()
    {
        owner.Anim.SetFloat("MovePower", Mathf.Abs(owner.MoveHzt));

        // 캐릭터 회전
        if (owner.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);

        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * owner.MoveHzt * owner.MovePower);

        // 이동속도 제한
        owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.MaxMoveSpeed, owner.MaxMoveSpeed), owner.Rigid.velocity.y);
    }

    public override void Exit()
    {
        owner.Anim.SetFloat("MovePower", 0);
    }
}
public class PlayerRunStop : PlayerBaseState
{

    public PlayerRunStop(PlayerAction owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("RunStop");
    }

    public override void FixedUpdate()
    {
        Brake();
    }

    private void Brake()
    {
        // 브레이크 적용
        if (owner.Rigid.velocity.x > owner.MoveForce_Threshold)
            owner.Rigid.AddForce(Vector2.left * owner.HztBrakePower);
        else if (owner.Rigid.velocity.x < owner.MoveForce_Threshold)
            owner.Rigid.AddForce(Vector2.right * owner.HztBrakePower);
    }
}
#endregion

#region Jump / Fall
public class PlayerJump : PlayerBaseState
{
    public PlayerJump(PlayerAction owner)
    {
        this.owner = owner;
    }

    public override void FixedUpdate()
    {
        FlyMoveMent();
    }
    public override void Update()
    {
        Rotation();
    }
    private void Rotation()
    {
        // 캐릭터 회전
        if (owner.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    private void FlyMoveMent()
    {
        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * owner.MoveHzt * owner.FlyMovePower);
    }
}
public class PlayerFall : PlayerBaseState
{
    public PlayerFall(PlayerAction owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Fall");
    }

    public override void FixedUpdate()
    {
        FlyMoveMent();
    }

    public override void Update()
    {
        Rotation();
    }
    private void Rotation()
    {
        // 캐릭터 회전
        if (owner.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }
    private void FlyMoveMent()
    {
        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * owner.MoveHzt * owner.FlyMovePower);
    }
}
#endregion

#region RopeAction
public class PlayerRoping : PlayerBaseState
{
    public PlayerRoping(PlayerAction owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("RopeAction");
    }

    public override void FixedUpdate()
    {
        RopeMoveMent();
    }

    public override void Update()
    {
        Rotation();
        //owner.Anim.SetFloat("RopeMovingPower", Mathf.Abs(owner.Rigid.velocity.magnitude));
    }

    private void Rotation()
    {
        // 캐릭터 회전
        if (owner.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    private void RopeMoveMent()
    {
        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * owner.MoveHzt * owner.RopeMovePower);
    }
}

#endregion