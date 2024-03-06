using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        owner.Rigid.velocity = Vector3.zero;

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

        owner.OnRun?.Invoke();

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
        owner.Rigid.velocity = Vector2.zero;

        owner.Anim.Play("WallSlide");
    }
    public override void Update()
    {
        WallMove();
    }
    public override void FixedUpdate()
    {
        
    }
    private void WallMove()
    {
        owner.Anim.SetFloat("MovePower", mover.MoveVtc);

        if(mover.MoveVtc != 0)
        {
            if(mover.MoveVtc < -PlayerBase.MoveForce_Threshold)
            {
                owner.OnWallSliding?.Invoke();

                float moveYPos = Mathf.Lerp(owner.transform.position.y, owner.transform.position.y + mover.SlidingPower * mover.MoveVtc, Time.deltaTime);
                owner.transform.position = new Vector3(owner.transform.position.x, moveYPos, 0);
            }
            else if(mover.MoveVtc > PlayerBase.MoveForce_Threshold)
            {
                owner.OnClimb?.Invoke();

                float moveYPos = Mathf.Lerp(owner.transform.position.y, owner.transform.position.y + mover.ClimbPower*mover.MoveVtc, Time.deltaTime);
                owner.transform.position = new Vector3(owner.transform.position.x, moveYPos, 0);
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
        // 이동속도 제한
        owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.PrMover.CurrentMaxRopingPower, owner.PrMover.CurrentMaxRopingPower), owner.Rigid.velocity.y);
    }

    public override void Exit()
    {
        owner.PrMover.CurrentMaxRopingPower = owner.PrMover.MaxRopingPower;
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
        owner.OnDash?.Invoke();

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
    }
    private void GrabMove()
    {
        // Move Braking
        if (mover.MoveHzt == 0)
        {
            if (mover.Rigid.velocity.x > PlayerBase.MoveForce_Threshold)
                mover.Rigid.AddForce(Vector2.left * mover.HoldingHztBrakePower);
            else if (mover.Rigid.velocity.x < PlayerBase.MoveForce_Threshold)
                mover.Rigid.AddForce(Vector2.right * mover.HoldingHztBrakePower);
        }
        else
        {
            // Character Rotation
            if (mover.MoveHzt > 0)
                owner.transform.rotation = Quaternion.Euler(0, -180, 0);
            else if (mover.MoveHzt < 0)
                owner.transform.rotation = Quaternion.Euler(0, 0, 0);

            // Controll Grab Enemy
            mover.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.HoldingMovePower);
            mover.Rigid.velocity = new Vector2(Mathf.Clamp(mover.Rigid.velocity.x, -mover.MaxHoldingMoveSpeed, mover.MaxHoldingMoveSpeed), mover.Rigid.velocity.y);
        }
    }

    public override void Exit()
    {
        owner.OnGrabEnd?.Invoke();
    }
}
#endregion

public class PlayerDamaged : PlayerBaseState
{
    private Coroutine damagedRoutine;

    public PlayerDamaged(PlayerFSM owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.OnTakeDamage?.Invoke();

        Time.timeScale = 0.5f;

        // init all state
        owner.PrFSM.IsJointed = false;
        owner.PrFSM.IsInWall = false;

        owner.Rigid.gravityScale = 1f;
        owner.Anim.Play("Damaged");
        damagedRoutine = owner.StartCoroutine(DamagedRoutine());
    }
    public override void Exit()
    {
        Time.timeScale = 1f;

        if (damagedRoutine != null)
            owner.StopCoroutine(damagedRoutine);
    }

    IEnumerator DamagedRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        owner.BeDamaged = false;
    }
}