using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerMover : PlayerBase
{
    [Header("Specs")]
    [Space(2)]
    #region Normal Movement
    [Header("Ground Move")]
    [SerializeField]
    private float movePower;
    public float MovePower { get { return movePower; } }

    [SerializeField]
    private float maxMoveSpeed;
    public float MaxMoveSpeed { get { return maxMoveSpeed; } }

    [SerializeField]
    private float hztBrakePower;    // horizontal movement brake force
    public float HztBrakePower { get { return hztBrakePower; } }

    [SerializeField]
    private float vtcBrakePower;    // vertical movement brake force
    public float VtcBrakePower { get { return vtcBrakePower; } }

    [SerializeField]
    private float holdingMovePower;
    public float HoldingMovePower { get { return holdingMovePower; } }

    [SerializeField]
    private float maxHoldingMoveSpeed;
    public float MaxHoldingMoveSpeed { get { return maxHoldingMoveSpeed; } }

    [SerializeField]
    private float holdingHztBrakePower;    // horizontal movement brake force
    public float HoldingHztBrakePower { get { return hztBrakePower; } }

    [Header("Wall Move")]
    [SerializeField]
    private float climbPower;
    public float ClimbPower { get { return climbPower; } }

    [SerializeField]
    private float slidingPower;
    public float SlidingPower { get { return slidingPower; } }

    [Header("In Air Move")]
    [SerializeField]
    private float jumpPower;
    public float JumpPower { get { return jumpPower; } }

    [SerializeField]
    private float flyMovePower;
    public float FlyMovePower { get { return flyMovePower; } }

    [Header("Be Damaged")]
    [SerializeField]
    private float hittedJumpPower;
    public float HittedJumpPower { get { return hittedJumpPower; } }

    #endregion

    #region Rope Movement
    [SerializeField]
    private float ropeMovePower;
    public float RopeMovePower { get { return ropeMovePower; } }

    [SerializeField]
    private float currentMaxRopingPower;
    public float CurrentMaxRopingPower { get { return currentMaxRopingPower; } set { currentMaxRopingPower = value; } }

    [SerializeField]
    private float maxRopingPower;
    public float MaxRopingPower { get { return maxRopingPower; } }

    [SerializeField]
    private float ceilingMovePower;
    public float CeilingMovePower { get { return ceilingMovePower; } }

    [SerializeField]
    private float maxCeilingMovePower;
    public float MaxCeilingMovePower { get { return maxCeilingMovePower; } }
    #endregion

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    // input value
    [SerializeField]
    protected float moveHzt;  // Keyboard input - 'A', 'D' *Ground Movement
    public float MoveHzt { get { return moveHzt; } }

    [SerializeField]
    protected float moveVtc;  // Keyboard input - 'W', 'S' *Wall Movement 
    public float MoveVtc { get { return moveVtc; } }

    protected override void Awake()
    {
        base.Awake();
    }

    #region Normal Movement
    // Keyboard Acition
    // move horizontally with keyboard "a" key and "d" key
    // jump with keyboard "space" key
    // +rope jump 
    private void OnMove(InputValue value)
    {
        moveHzt = value.Get<Vector2>().x;
        moveVtc = value.Get<Vector2>().y;
    }
    private void OnJump(InputValue value)
    {
        if (PrFSM.BeDamaged)
        {
            HitJump();
            return;
        }
        if (PrFSM.IsDash || PrFSM.IsGrab || PrFSM.IsCeilingStick) return;



        if (PrFSM.IsJointed)
        {
            PrSkill.CeilingStick();
            return;
        }
        if (PrFSM.IsCeilingStick)
        {
            PrFSM.IsCeilingStick = false;
            PrFSM.FSM.ChangeState("Jump");
            Jump();
            return;
        }

        if (PrFSM.IsInWall)
        {
            WallJump();
            return;
        }
        if (PrFSM.IsGround)
        {
            Jump();
            return;
        }
    }

    // 기본 점프
    private void Jump()
    {
        PrFSM.OnJump?.Invoke();

        anim.Play("Jump");
        rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y + jumpPower);
    }

    // 벽 점프
    private void WallJump()
    {
        PrFSM.OnWallJump?.Invoke();

        rigid.gravityScale = 1;

        PrFSM.IsInWall = false;
        PrFSM.ChangeState("Jump");
        anim.Play("Jump");

        rigid.velocity = Vector2.zero;
        rigid.velocity = new Vector2(-transform.right.x * 3f, jumpPower);
    }

    // 히트상태 점프
    private void HitJump()
    {
        PrFSM.BeDamaged = false;
        PrFSM.ChangeState("Jump");

        PrFSM.OnHitJump?.Invoke();
        anim.Play("Jump");

        rigid.velocity = Vector2.zero;

        Vector2 dir = (PrHooker.Aim.transform.position - transform.position).normalized;
        rigid.AddForce(dir * hittedJumpPower, ForceMode2D.Impulse);

    }
    #endregion

    #region Skills
    private void OnRopeForce(InputValue value)
    {
        if (playerFSM.IsJointed)
            playerSkill.RopeForce();
    }
    #endregion
}
