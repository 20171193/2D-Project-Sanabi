using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerBaseState : BaseState
{
    protected PlayerBase owner;
}

#region Idle
public class PlayerIdle : PlayerBaseState
{
    public PlayerIdle(PlayerBase owner)
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
    private PlayerMover mover;
    public PlayerRun(PlayerBase owner)
    {
        this.owner = owner;
        mover = owner.GetComponent<PlayerMover>();
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
        owner.Anim.SetFloat("MovePower", Mathf.Abs(mover.MoveHzt));

        // 캐릭터 회전
        if (mover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (mover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);

        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.MovePower);

        // 이동속도 제한
        owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -mover.MaxMoveSpeed, mover.MaxMoveSpeed), owner.Rigid.velocity.y);
    }

    public override void Exit()
    {
        owner.Anim.SetFloat("MovePower", 0);
    }
}
public class PlayerRunStop : PlayerBaseState
{
    private PlayerMover mover;
    public PlayerRunStop(PlayerBase owner)
    {
        this.owner = owner;
        mover = owner.GetComponent<PlayerMover>();
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
        if (owner.Rigid.velocity.x > PlayerBase.MoveForce_Threshold)
            owner.Rigid.AddForce(Vector2.left * mover.HztBrakePower);
        else if (owner.Rigid.velocity.x < PlayerBase.MoveForce_Threshold)
            owner.Rigid.AddForce(Vector2.right * mover.HztBrakePower);
    }
}
#endregion

#region Jump / Fall
public class PlayerJump : PlayerBaseState
{
    private PlayerMover mover;

    public PlayerJump(PlayerBase owner)
    {
        this.owner = owner;
        mover = owner.GetComponent<PlayerMover>();  
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
        if (mover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (mover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    private void FlyMoveMent()
    {
        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.FlyMovePower);
    }
}
public class PlayerFall : PlayerBaseState
{
    private PlayerMover mover;

    public PlayerFall(PlayerBase owner)
    {
        this.owner = owner;
        mover = owner.GetComponent<PlayerMover>();
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
        if (mover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (mover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }
    private void FlyMoveMent()
    {
        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.FlyMovePower);
    }
}
#endregion

#region RopeAction
public class PlayerRoping : PlayerBaseState
{
    private PlayerMover mover;

    public PlayerRoping(PlayerBase owner)
    {
        this.owner = owner;
        mover = owner.GetComponent<PlayerMover>();
    }

    public override void Enter()
    {
        // Calculate the distance to fired hook and AddForce
        owner.Anim.Play("RopeAction");
        //StartRecoil();
    }

    public override void FixedUpdate()
    {
        RopeMoveMent();
    }

    public override void Update()
    {
        Rotation();
    }

    private void StartRecoil()
    {
        Vector3 dirX = Vector3.zero;
        if (owner.transform.position.x <= owner.FiredHook.transform.position.x)
            dirX = Vector3.right;
        else
            dirX = Vector3.left;

        float distX = Mathf.Abs(owner.FiredHook.transform.position.x - owner.transform.position.x);
        Debug.Log($"StartRecoil : {dirX}, {distX}");
        owner.Rigid.AddForce(dirX * distX, ForceMode2D.Impulse);
    }
    private void Rotation()
    {
        // 캐릭터 회전
        if (mover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (mover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }
    private void RopeMoveMent()
    {
        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.RopeMovePower);
    }
}

#endregion

#region Dash and Grab

public class PlayerDash : PlayerBaseState
{
    public PlayerDash(PlayerBase owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Dash");
    }

    //public IEnumerator DashCCD()
    //{
    //    //yield return new WaitForSeconds();
    //}
}
public class PlayerGrab : PlayerBaseState
{
    private PlayerMover mover;

    public PlayerGrab(PlayerBase owner)
    {
        this.owner = owner;
        mover = owner.GetComponent<PlayerMover>();  
    }

    public override void Enter()
    {
        owner.Anim.Play("Grab");
    }

    public override void FixedUpdate()
    {
        // Grab Moving
        GrabMove();
    }

    public override void Update()
    {
        owner.Anim.SetFloat("MovePower", mover.MoveHzt);
        owner.GrabEnemy.Anim.SetFloat("MovePower", mover.MoveHzt);

        // follow enemy x position
        owner.transform.position = new Vector3(owner.GrabEnemy.transform.position.x, owner.transform.position.y, owner.transform.position.z);
    }


    private void GrabMove()
    {
        // In the state of Grab an enemy,
        // the movement speed is halved compared to the player run movement speed.
        // also brake power is halved compared to the player runstop brake speed to.


        // Move Braking
        if (mover.MoveHzt == 0)
        {
            if (owner.GrabEnemy.Rigid.velocity.x > PlayerBase.MoveForce_Threshold)
                owner.GrabEnemy.Rigid.AddForce(Vector2.left * mover.HoldingHztBrakePower);
            else if (owner.GrabEnemy.Rigid.velocity.x < PlayerBase.MoveForce_Threshold)
                owner.GrabEnemy.Rigid.AddForce(Vector2.right * mover.HoldingHztBrakePower);
        }
        else
        {
            // Controll Grab Enemy
            owner.GrabEnemy.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.HoldingMovePower);
            owner.GrabEnemy.Rigid.velocity = new Vector2(Mathf.Clamp(owner.GrabEnemy.Rigid.velocity.x, -mover.MaxHoldingMoveSpeed, mover.MaxHoldingMoveSpeed), owner.GrabEnemy.Rigid.velocity.y);
        }
    }

    public override void Exit()
    {
        
    }

}
#endregion