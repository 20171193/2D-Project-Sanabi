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
        Debug.Log("Enter : Idle");
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
        //owner.Rigid.velocity = Vector3.zero;
        Debug.Log("Enter : Run");
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
        Debug.Log("Enter : Runstop");

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

        if (mover.MoveVtc != 0)
        {
            if (mover.MoveVtc < -PlayerBase.MoveForce_Threshold)
            {
                owner.OnWallSliding?.Invoke();

                float moveYPos = Mathf.Lerp(owner.transform.position.y, owner.transform.position.y + mover.SlidingPower * mover.MoveVtc, Time.deltaTime);
                owner.transform.position = new Vector3(owner.transform.position.x, moveYPos, 0);
            }
            else if (mover.MoveVtc > PlayerBase.MoveForce_Threshold)
            {
                owner.OnClimb?.Invoke();

                float moveYPos = Mathf.Lerp(owner.transform.position.y, owner.transform.position.y + mover.ClimbPower * mover.MoveVtc, Time.deltaTime);
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
    public override void Enter()
    {
        Debug.Log("Enter : Jump");
        owner.Anim.Play("Jump");
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
        //owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.FlyMovePower);

        if (mover.MoveHzt == 0)
        {
            // 브레이크 적용
            if (owner.Rigid.velocity.x > PlayerBase.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * mover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -PlayerBase.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * mover.HztBrakePower);
        }
        else
        {
            // 실제 이동
            owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.MovePower);
            // 이동속도 제한
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -mover.MaxMoveSpeed, mover.MaxMoveSpeed), owner.Rigid.velocity.y);
        }
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
        Debug.Log("Enter : Fall");
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
        if (mover.MoveHzt == 0)
        {
            // 브레이크 적용
            if (owner.Rigid.velocity.x > PlayerBase.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * mover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -PlayerBase.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * mover.HztBrakePower);
        }
        else
        {
            // 실제 이동
            owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.MovePower);
            // 이동속도 제한
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -mover.MaxMoveSpeed, mover.MaxMoveSpeed), owner.Rigid.velocity.y);
        }
    }
}
#endregion

#region RopeAction
public class PlayerHookShoot : PlayerBaseState
{
    public PlayerHookShoot(PlayerFSM owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
        owner.Rigid.velocity = Vector2.zero;
        owner.Anim.Play("HookShoot");
    }
}
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
        Debug.Log("Enter : Roping");

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
public class PlayerCeilingStickStart : PlayerBaseState
{
    private PlayerSkill skill;
    public PlayerCeilingStickStart(PlayerFSM owner)
    {
        this.owner = owner;
        skill = owner.PrSkill;
    }

    public override void Enter()
    {
        Debug.Log("Enter : StcikStart");

        owner.Anim.Play("CeilingStickStart");
    }
    public override void Exit()
    {
    }
}
public class PlayerCeilingStickIdle : PlayerBaseState
{
    private PlayerMover mover;

    public PlayerCeilingStickIdle(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
    }
    public override void Enter()
    {
        Debug.Log("Enter : StcikIdle");

        owner.Rigid.velocity = Vector3.zero;
        owner.Anim.Play("CeilingStickIdle");
    }
    public override void Exit()
    {
        //owner.Rigid.gravityScale = 1f;
    }
    public override void Update()
    {
        if (mover.MoveHzt != 0)
            owner.FSM.ChangeState("CeilingStickMove");
    }

}
public class PlayerCeilingStickMove : PlayerBaseState
{
    private PlayerMover mover;
    private Coroutine brakeRoutine;

    public PlayerCeilingStickMove(PlayerFSM owner)
    {
        this.owner = owner;
        mover = owner.PrMover;
    }

    public override void Enter()
    {
        Debug.Log("Enter : StcikMove");
        owner.Anim.Play("CeilingStickMove");
    }
    public override void Update()
    {
        owner.Anim.SetFloat("MovePower", owner.Rigid.velocity.magnitude);
    }
    public override void FixedUpdate()
    {
        CeilingMove();
    }
    public override void Exit()
    {
    }
    private void CeilingMove()
    {
        // 캐릭터 회전
        if (mover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (mover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);

        if (mover.MoveHzt == 0)
        {
            // 브레이크 적용
            if (owner.Rigid.velocity.x > PlayerBase.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * mover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -PlayerBase.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * mover.HztBrakePower);

            // 브레이크 이후 딜레이 적용 -> 일정시간 이후 Idle 상태로 전환
            if (brakeRoutine == null)
            {
                owner.Anim.Play("CeilingMoveEnd");
                brakeRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.FSM.ChangeState("CeilingStickIdle")));
            }
        }
        else
        {
            // 브레이크 이후 이동 시 이전에 진행중이던 브레이크 루틴 종료
            if (brakeRoutine != null)
            {
                // 이동 애니메이션 재실행
                owner.Anim.Play("CeilingStickMove");
                owner.StopCoroutine(brakeRoutine);
                brakeRoutine = null;
            }

            // 실제 이동
            owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.CeilingMovePower);
            // 이동속도 제한
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -mover.MaxCeilingMovePower, mover.MaxCeilingMovePower), owner.Rigid.velocity.y);
        }
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
        if (owner.IsEnableGrabMove)
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
        owner.IsEnableGrabMove = false;
        owner.OnGrabEnd?.Invoke();
    }
}
#endregion

public class PlayerDamaged : PlayerBaseState
{
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
    }
    public override void Exit()
    {
        Time.timeScale = 1f;
    }
}