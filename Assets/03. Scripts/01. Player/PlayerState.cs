using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerBaseState : BaseState
{
    protected Player owner;
}

#region Idle
public class PlayerIdle : PlayerBaseState
{
    public PlayerIdle(Player owner)
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
    public PlayerRun(Player owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        //owner.Rigid.velocity = Vector3.zero;
        owner.Anim.Play("Run");
    }

    public override void FixedUpdate()
    {
        Move();
    }
    private void Move()
    {
        owner.Anim.SetFloat("MovePower", Mathf.Abs(owner.PrMover.MoveHzt));

        owner.OnRun?.Invoke();

        // ĳ���� ȸ��
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);

        // ���� �̵�
        owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.MovePower);
        // �̵��ӵ� ����
        owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.PrMover.MaxMoveSpeed, owner.PrMover.MaxMoveSpeed), owner.Rigid.velocity.y);
    }

    public override void Exit()
    {
        owner.Anim.SetFloat("MovePower", 0);
    }
}
public class PlayerRunStop : PlayerBaseState
{
    public PlayerRunStop(Player owner)
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
        // �극��ũ ����
        if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
            owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
        else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);
    }
}
#endregion

#region WallSliding
public class PlayerWallSlide : PlayerBaseState
{
    
    public PlayerWallSlide(Player owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
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
        owner.Anim.SetFloat("MovePower", owner.PrMover.MoveVtc);

        if (owner.PrMover.MoveVtc != 0)
        {
            if (owner.PrMover.MoveVtc < -Player.MoveForce_Threshold)
            {
                owner.OnWallSliding?.Invoke();

                float moveYPos = Mathf.Lerp(owner.transform.position.y, owner.transform.position.y + owner.PrMover.SlidingPower * owner.PrMover.MoveVtc, Time.deltaTime);
                owner.transform.position = new Vector3(owner.transform.position.x, moveYPos, 0);
            }
            else if (owner.PrMover.MoveVtc > Player.MoveForce_Threshold)
            {
                owner.OnClimb?.Invoke();

                float moveYPos = Mathf.Lerp(owner.transform.position.y, owner.transform.position.y + owner.PrMover.ClimbPower * owner.PrMover.MoveVtc, Time.deltaTime);
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
    public PlayerJump(Player owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
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
        // ĳ���� ȸ��
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    private void FlyMoveMent()
    {
        // ���� �̵�
        //owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.FlyMovePower);

        if (owner.PrMover.MoveHzt == 0)
        {
            // �극��ũ ����
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);
        }
        else
        {
            // ���� �̵�
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.MovePower);
            // �̵��ӵ� ����
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.PrMover.MaxMoveSpeed, owner.PrMover.MaxMoveSpeed), owner.Rigid.velocity.y);
        }
    }
}
public class PlayerHookingJump : PlayerBaseState
{
    
    public PlayerHookingJump(Player owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
        owner.Anim.Play("RopeJump");
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
        // ĳ���� ȸ��
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    private void FlyMoveMent()
    {
        // ���� �̵�
        //owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.FlyMovePower);

        if (owner.PrMover.MoveHzt == 0)
        {
            // �극��ũ ����
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);
        }
        else
        {
            // ���� �̵�
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.MovePower);
            // �̵��ӵ� ����
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.PrMover.MaxMoveSpeed, owner.PrMover.MaxMoveSpeed), owner.Rigid.velocity.y);
        }
    }
}
public class PlayerFall : PlayerBaseState
{

    public PlayerFall(Player owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.PrHooker.FiredHook?.DisConnecting();
        owner.Rigid.gravityScale = 1;
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
        // ĳ���� ȸ��
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }
    private void FlyMoveMent()
    {
        if (owner.PrMover.MoveHzt == 0)
        {
            // �극��ũ ����
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);
        }
        else
        {
            // ���� �̵�
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.MovePower);
            // �̵��ӵ� ����
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.PrMover.MaxMoveSpeed, owner.PrMover.MaxMoveSpeed), owner.Rigid.velocity.y);
        }
    }
}
#endregion

#region RopeAction
public class PlayerHookShoot : PlayerBaseState
{
    public PlayerHookShoot(Player owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
        owner.Rigid.velocity = Vector2.zero;
        owner.Anim.Play("HookShoot");
    }
    public override void Update()
    {
        if (owner.PrHooker.HookObject.activeSelf == false)
        {
            owner.PrFSM.ChangeState("Idle");
        }
    }
}
public class PlayerRoping : PlayerBaseState
{
    public PlayerRoping(Player owner)
    {
        this.owner = owner;
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
        // ĳ���� ȸ��
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }
    private void RopeMoveMent()
    {
        // ���� �̵�
        owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.RopeMovePower);
        // �̵��ӵ� ����
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
    private Coroutine ceilingStickRoutine;
    public PlayerCeilingStickStart(Player owner)
    {
        this.owner = owner;
        skill = owner.PrSkill;
    }

    public override void Enter()
    {
        owner.Anim.Play("CeilingStickStart");
    }
    public override void Exit()
    {

    }
}
public class PlayerCeilingStickIdle : PlayerBaseState
{
    

    public PlayerCeilingStickIdle(Player owner)
    {
        this.owner = owner;
    }
    public override void Enter()
    {
        owner.Rigid.velocity = Vector3.zero;
        owner.Anim.Play("CeilingStickIdle");
    }
    public override void Exit()
    {
        //owner.Rigid.gravityScale = 1f;
    }
    public override void Update()
    {
        if (owner.PrMover.MoveHzt != 0)
            owner.PrFSM.FSM.ChangeState("CeilingStickMove");
    }

}
public class PlayerCeilingStickMove : PlayerBaseState
{
    
    private Coroutine brakeRoutine;

    public PlayerCeilingStickMove(Player owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
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
        // ĳ���� ȸ��
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);

        if (owner.PrMover.MoveHzt == 0)
        {
            // �극��ũ ����
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);

            // �극��ũ ���� ������ ���� -> �����ð� ���� Idle ���·� ��ȯ
            if (brakeRoutine == null)
            {
                owner.Anim.Play("CeilingMoveEnd");
                brakeRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.PrFSM.ChangeState("CeilingStickIdle")));
            }
        }
        else
        {
            // �극��ũ ���� �̵� �� ������ �������̴� �극��ũ ��ƾ ����
            if (brakeRoutine != null)
            {
                // �̵� �ִϸ��̼� �����
                owner.Anim.Play("CeilingStickMove");
                owner.StopCoroutine(brakeRoutine);
                brakeRoutine = null;
            }

            // ���� �̵�
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.CeilingMovePower);
            // �̵��ӵ� ����
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.PrMover.MaxCeilingMovePower, owner.PrMover.MaxCeilingMovePower), owner.Rigid.velocity.y);
        }
    }
}

#endregion

#region Dash and Grab

public class PlayerDash : PlayerBaseState
{
    public PlayerDash(Player owner)
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

    public PlayerGrab(Player owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
        owner.Anim.Play("Grab");
    }

    public override void FixedUpdate()
    {
        // Grab Moving
        if (owner.PrFSM.IsEnableGrabMove)
            GrabMove();
    }
    public override void Update()
    {
        owner.Anim.SetFloat("MovePower", owner.PrMover.MoveHzt);
    }
    private void GrabMove()
    {
        // Move Braking
        if (owner.PrMover.MoveHzt == 0)
        {
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HoldingHztBrakePower);
            else if (owner.Rigid.velocity.x < Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HoldingHztBrakePower);
        }
        else
        {
            // Character Rotation
            if (owner.PrMover.MoveHzt > 0)
                owner.transform.rotation = Quaternion.Euler(0, -180, 0);
            else if (owner.PrMover.MoveHzt < 0)
                owner.transform.rotation = Quaternion.Euler(0, 0, 0);

            // Controll Grab Enemy
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.HoldingMovePower);
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.PrMover.MaxHoldingMoveSpeed, owner.PrMover.MaxHoldingMoveSpeed), owner.Rigid.velocity.y);
        }
    }

    public override void Exit()
    {
        owner.PrFSM.IsEnableGrabMove = false;
        owner.OnGrabEnd?.Invoke();
    }
}
#endregion

public class PlayerDamaged : PlayerBaseState
{
    public PlayerDamaged(Player owner)
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