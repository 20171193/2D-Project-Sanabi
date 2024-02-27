using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerBaseState : BaseState
{
    protected PlayerFSM owner;
}

#region Idle
public class PlayerIdle : PlayerBaseState
{
    public PlayerIdle(PlayerFSM owner)
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
    public PlayerRun(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
    }

    public override void Enter()
    {
        Debug.Log("Enter Run");
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
    public PlayerRunStop(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
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
        else if (owner.Rigid.velocity.x < -PlayerBase.MoveForce_Threshold)
            owner.Rigid.AddForce(Vector2.right * mover.HztBrakePower);
    }
}
#endregion

#region WallSliding
public class PlayerWallSlide : PlayerBaseState
{
    private PlayerMover mover;
    public PlayerWallSlide(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
    }

    public override void Enter()
    {
        Debug.Log("Enter WallSlide");
        owner.Rigid.gravityScale = 0;
        owner.Anim.Play("WallSlide");
    }

    public override void FixedUpdate()
    {
        WallMove();
    }
    private void WallMove()
    {
        owner.Anim.SetFloat("MovePower", Mathf.Abs(mover.MoveVtc));

        if (mover.MoveVtc == 0)
        {
            // brake
            if (owner.Rigid.velocity.y > PlayerBase.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.down * mover.VtcBrakePower);
            else if (owner.Rigid.velocity.y < -PlayerBase.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.up * mover.VtcBrakePower);
        }
        else
        {
            if (mover.MoveVtc < 0)
            {
                owner.Rigid.AddForce(Vector2.up * mover.MoveVtc * mover.MovePower, ForceMode2D.Force);
                owner.Rigid.velocity = new Vector2(owner.Rigid.velocity.x, Mathf.Clamp(owner.Rigid.velocity.y, -mover.MaxMoveSpeed, mover.MaxMoveSpeed));
            }
            else
            {
                owner.Rigid.AddForce(Vector2.up * mover.MoveVtc * mover.MovePower/2f, ForceMode2D.Force);
                owner.Rigid.velocity = new Vector2(owner.Rigid.velocity.x, Mathf.Clamp(owner.Rigid.velocity.y, -mover.MaxMoveSpeed/2f, mover.MaxMoveSpeed/2f));
            }
        }
    }

    public override void Exit()
    {
        owner.Rigid.gravityScale = 1;
        owner.Anim.SetFloat("MovePower", 0);
    }
}

#endregion

#region Jump / Fall
public class PlayerJump : PlayerBaseState
{
    private PlayerMover mover;

    public PlayerJump(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
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

    public PlayerFall(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
    }

    public override void Enter()
    {
        Debug.Log("Fall Enter");
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

    public PlayerRoping(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
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
        if (owner.transform.position.x <= owner.PrHooker.FiredHook.transform.position.x)
            dirX = Vector3.right;
        else
            dirX = Vector3.left;

        float distX = Mathf.Abs(owner.PrHooker.FiredHook.transform.position.x - owner.transform.position.x);
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
    public PlayerDash(PlayerFSM owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Dash");
    }
}
public class PlayerGrab : PlayerBaseState
{
    private PlayerMover mover;
    private PlayerHooker hooker;

    public PlayerGrab(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
        hooker = owner.PrHooker;
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
        hooker.GrabEnemy.Anim.SetFloat("MovePower", mover.MoveHzt);

        // follow enemy x position
        owner.transform.position = new Vector3(hooker.GrabEnemy.transform.position.x, owner.transform.position.y, owner.transform.position.z);
    }


    private void GrabMove()
    {
        // In the state of Grab an enemy,
        // the movement speed is halved compared to the player run movement speed.
        // also brake power is halved compared to the player runstop brake speed to.


        // Move Braking
        if (mover.MoveHzt == 0)
        {
            if (hooker.GrabEnemy.Rigid.velocity.x > PlayerBase.MoveForce_Threshold)
                hooker.GrabEnemy.Rigid.AddForce(Vector2.left * mover.HoldingHztBrakePower);
            else if (hooker.GrabEnemy.Rigid.velocity.x < PlayerBase.MoveForce_Threshold)
                hooker.GrabEnemy.Rigid.AddForce(Vector2.right * mover.HoldingHztBrakePower);
        }
        else
        {
            // Controll Grab Enemy
            hooker.GrabEnemy.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.HoldingMovePower);
            hooker.GrabEnemy.Rigid.velocity = new Vector2(Mathf.Clamp(hooker.GrabEnemy.Rigid.velocity.x, -mover.MaxHoldingMoveSpeed, mover.MaxHoldingMoveSpeed), hooker.GrabEnemy.Rigid.velocity.y);
        }
    }

    public override void Exit()
    {
        
    }

}
#endregion