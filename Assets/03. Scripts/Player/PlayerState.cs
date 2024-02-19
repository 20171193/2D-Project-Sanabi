using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerBaseState : BaseState
{
    protected PlayerAction owner;
    
}

public class PlayerIdle : PlayerBaseState
{
    public PlayerIdle(PlayerAction owner)
    {
        this.owner = owner;
    }

    public override void Enter()
    {
    }
}

public class PlayerRun : PlayerBaseState
{
    public PlayerRun(PlayerAction owner)
    {
        this.owner = owner;
    }
    public override void FixedUpdate()
    {
        Move();
    }
    private void Move()
    {
        owner.Anim.SetFloat("MovePower", Mathf.Abs(owner.MoveHzt));

        // 입력이 없는 경우
        if (owner.MoveHzt == 0)
        {
            // 브레이크 적용
            if (owner.Rigid.velocity.x > owner.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.left * owner.HztBrakePower);
            else if (owner.Rigid.velocity.x < owner.MoveForce_Threshold)
                owner.Rigid.AddForce(Vector2.right * owner.HztBrakePower);
        }
        else
        {
            // 캐릭터 회전
            if (owner.MoveHzt > 0)
                owner.transform.rotation = Quaternion.Euler(0, 0, 0);
            else if (owner.MoveHzt < 0)
                owner.transform.rotation = Quaternion.Euler(0, -180, 0);


            owner.Rigid.AddForce(Vector2.right * owner.MoveHzt * owner.MovePower);
            // 이동속도 제한
            owner.Rigid.velocity = new Vector2(Mathf.Clamp(owner.Rigid.velocity.x, -owner.MaxMoveSpeed, owner.MaxMoveSpeed), owner.Rigid.velocity.y);
        }

        owner.Anim.SetFloat("ySpeed", owner.Rigid.velocity.y);

    }

    public override void Exit()
    {
        owner.Anim.SetFloat("MovePower", 0);
    }
}

public class PlayerJump : PlayerBaseState
{
    public PlayerJump(PlayerAction owner)
    {
        this.owner = owner;
    }
    private void Jump()
    {
        owner.Rigid.velocity = new Vector2(owner.Rigid.velocity.x, owner.Rigid.velocity.y + owner.JumpPower);
    }
    public override void Enter()
    {
        // 로프액션 추가 시 수정
        Jump();
    }

    public override void Update()
    {
        
    }

}

public class PlayerFall : PlayerBaseState
{
    public PlayerFall(PlayerAction owner)
    {
        this.owner = owner;
    }
}
