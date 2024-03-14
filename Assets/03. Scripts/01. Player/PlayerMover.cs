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

    // 리팩토링


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
        if (Player.PrFSM.FSM.CurState == "Grab" || Player.PrFSM.FSM.CurState == "Dash" ||
            Player.PrFSM.FSM.CurState == "HookShoot" || Player.PrFSM.FSM.CurState == "Fall") return;

        #region 스킬
        // 로프액션 상태 -> 천장후킹
        if (Player.PrFSM.FSM.CurState == "Roping")
        {
            Debug.Log("CeilingStickSkill");
            Player.PrSkill.CeilingStick();
            return;
        }
        #endregion


        #region 점프
        // 피격상태 -> 히트 점프
        if (Player.PrFSM.FSM.CurState == "Damaged")
        {
            HitJump();
            return;
        }
        // 천장후킹 상태 -> 천장점프
        if (Player.PrFSM.IsCeilingStick)
        {
            CeilingJump();
            return;
        }
        // 벽타기 상태 -> 벽점프
        if (Player.PrFSM.FSM.CurState == "WallSlide")
        {
            WallJump();
            return;
        }
        // 지면 상태 -> 일반점프
        if (Player.PrFSM.IsGround)
        {
            Jump();
            return;
        }
        #endregion
    }

    // 기본 점프
    private void Jump()
    {
        Debug.Log("Normal Jumping");

        Player.OnJump?.Invoke();

        Player.PrFSM.ChangeState("Jump");
        Player.Rigid.velocity = new Vector2(Player.Rigid.velocity.x, Player.Rigid.velocity.y + jumpPower);
    }
    // 벽 점프
    private void WallJump()
    {
        Debug.Log("Wall Jumping");
        Player.OnWallJump?.Invoke();

        Player.Rigid.gravityScale = 1;

        Player.PrFSM.IsInWall = false;
        Player.PrFSM.ChangeState("Jump");

        Player.Rigid.velocity = new Vector2(-transform.right.x * 3f, jumpPower);
    }
    // 히트상태 점프
    private void HitJump()
    {
        Debug.Log("Hit Jumping");

        // 기존 데미지 루틴 비활성화
        Player.InitDamageRoutine();

        Player.PrFSM.ChangeState("Jump");

        Player.OnHitJump?.Invoke();
        Player.Rigid.velocity = Vector2.zero;

        Vector2 dir = (Player.PrHooker.Aim.transform.position - transform.position).normalized;
        Player.Rigid.AddForce(dir * hittedJumpPower, ForceMode2D.Impulse);

    }
    private void CeilingJump()
    {
        Debug.Log("Ceiling Jumping");

        Player.Rigid.gravityScale = 1;

        // 기존 상태 초기화
        Player.PrFSM.IsCeilingStick = false;
        Player.PrFSM.ChangeState("Fall");

        Player.Rigid.velocity = new Vector2(MoveHzt * 3f, -0.5f);
    }
    #endregion

    #region Skills
    private void OnRopeForce(InputValue value)
    {
        if (Player.PrFSM.IsJointed)
            Player.PrSkill.RopeForce();
    }
    #endregion
}
