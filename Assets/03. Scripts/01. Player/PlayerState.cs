using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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

        // 캐릭터 회전
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);

        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.MovePower);
        // 이동속도 제한
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
        // 브레이크 적용
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

        owner.OnClimb?.Invoke();
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
                float moveYPos = Mathf.Lerp(owner.transform.position.y, owner.transform.position.y + owner.PrMover.SlidingPower * owner.PrMover.MoveVtc, Time.deltaTime);
                owner.transform.position = new Vector3(owner.transform.position.x, moveYPos, 0);
            }
            else if (owner.PrMover.MoveVtc > Player.MoveForce_Threshold)
            {
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
        // 캐릭터 회전
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    private void FlyMoveMent()
    {
        // 실제 이동
        //owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.FlyMovePower);

        if (owner.PrMover.MoveHzt == 0)
        {
            // 브레이크 적용
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);
        }
        else
        {
            // 실제 이동
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.MovePower);
            // 이동속도 제한
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
        // 캐릭터 회전
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    private void FlyMoveMent()
    {
        // 실제 이동
        //owner.Rigid.AddForce(Vector2.right * mover.MoveHzt * mover.FlyMovePower);

        if (owner.PrMover.MoveHzt == 0)
        {
            // 브레이크 적용
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);
        }
        else
        {
            // 실제 이동
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.MovePower);
            // 이동속도 제한
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
        // 캐릭터 회전
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }
    private void FlyMoveMent()
    {
        if (owner.PrMover.MoveHzt == 0)
        {
            // 브레이크 적용
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);
        }
        else
        {
            // 실제 이동
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.MovePower);
            // 이동속도 제한
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
        // 캐릭터 회전
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);
    }
    private void RopeMoveMent()
    {
        // 실제 이동
        owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.RopeMovePower);
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
        // 캐릭터 회전
        if (owner.PrMover.MoveHzt > 0)
            owner.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (owner.PrMover.MoveHzt < 0)
            owner.transform.rotation = Quaternion.Euler(0, -180, 0);

        if (owner.PrMover.MoveHzt == 0)
        {
            // 브레이크 적용
            if (owner.Rigid.velocity.x > Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.PrMover.HztBrakePower);
            else if (owner.Rigid.velocity.x < -Player.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.PrMover.HztBrakePower);

            // 브레이크 이후 딜레이 적용 -> 일정시간 이후 Idle 상태로 전환
            if (brakeRoutine == null)
            {
                owner.Anim.Play("CeilingMoveEnd");
                brakeRoutine = owner.StartCoroutine(Extension.DelayRoutine(0.3f, () => owner.PrFSM.ChangeState("CeilingStickIdle")));
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
            owner.Rigid.AddForce(Vector2.right * owner.PrMover.MoveHzt * owner.PrMover.CeilingMovePower);
            // 이동속도 제한
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
        owner.gameObject.layer = LayerMask.NameToLayer("PlayerGrabing");
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
        owner.gameObject.layer = LayerMask.NameToLayer("Player");
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

public class PlayerDeadZoneDie : PlayerBaseState
{
    public PlayerDeadZoneDie(Player owner){this.owner = owner;}
    private Coroutine dieRoutine;

    public override void Enter()
    {
        owner.OnDeathByDeadZone?.Invoke();

        owner.EventController.EnableDeathEvent(DeathType.DeadZone);
        owner.PrFSM.IsDamageable = false;

        Time.timeScale = 0.8f;

        Manager.Camera.SetBlendTime(0.5f);
        Manager.Camera.SetCameraPriority(CameraType.Zoom);
        dieRoutine = owner.StartCoroutine(Extension.DelayRoutine(1f, () => owner.Respawn()));
    }
    public override void Exit()
    {
        if (dieRoutine != null)
            owner.StopCoroutine(dieRoutine);

        owner.PrFSM.IsDamageable = true;

        Time.timeScale = 1f;
        Manager.Camera.SetDefaultBlendTime();
        Manager.Camera.SetCameraPriority(CameraType.Main);
    }

}
public class PlayerDamagedDie : PlayerBaseState
{
    public PlayerDamagedDie(Player owner) { this.owner = owner; }
    private Coroutine dieRoutine;

    public override void Enter()
    {
        owner.OnDeathByDamaged?.Invoke();

        owner.EventController.EnableDeathEvent(DeathType.Damaged);
        owner.PrFSM.IsDamageable = false;

        Time.timeScale = 0.4f;
        Manager.Camera.SetBlendTime(0.5f);
        Manager.Camera.SetCameraPriority(CameraType.Zoom);

        dieRoutine = owner.StartCoroutine(Extension.DelayRoutine(2f, () => owner.Respawn()));
    }
    public override void Exit()
    {
        if (dieRoutine != null)
            owner.StopCoroutine(dieRoutine);

        owner.PrFSM.IsDamageable = true;

        Time.timeScale = 1f;
        Manager.Camera.SetDefaultBlendTime();
        Manager.Camera.SetCameraPriority(CameraType.Main);
    }
}
public class PlayerRespawn : PlayerBaseState
{
    public PlayerRespawn(Player owner) { this.owner = owner; }

    public override void Enter()
    {
        Manager.Camera.SetBlendTime(0);
        Manager.Camera.SetCameraPriority(CameraType.Zoom);

        owner.PrFSM.IsDamageable = false;

        owner.Anim.Play("PlayerRespawn");
        owner.PrInput.enabled = false;
        owner.StartCoroutine(Extension.DelayRoutine(3f, () => owner.PrFSM.ChangeState("Idle")));
    }

    public override void Exit()
    {
        Manager.Camera.SetDefaultBlendTime();
        Manager.Camera.SetCameraPriority(CameraType.Main);

        owner.PrFSM.IsDamageable = true;

        owner.PrInput.enabled = true;
    }
}


public class PlayerCutSceneMode : PlayerBaseState
{
    public PlayerCutSceneMode(Player owner) { this.owner = owner; }

    public override void Enter()
    {
        owner.PrFSM.IsDamageable = false;

        owner.Rigid.velocity = Vector2.zero;
        owner.PrInput.enabled = false;

        if (owner.CutSceneAnim.Length > 0)
            owner.Anim.Play(owner.CutSceneAnim);
        else
            owner.Anim.Play("Idle");
    }

    public override void Exit()
    {
        owner.PrFSM.IsDamageable = true;

        owner.PrInput.enabled = true;
    }
}