using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

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
    #endregion

    #region Rope Movement
    [SerializeField]
    private float ropeMovePower;
    public float RopeMovePower { get { return ropeMovePower; } }

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
        if (playerFSM.IsJointed || playerFSM.IsDash || playerFSM.IsGrab) return;

        if (playerFSM.IsGround)
            Jump();
        if (playerFSM.IsInWall)
            WallJump();
    }

    private void WallJump()
    {
        anim.Play("Jump");

        rigid.gravityScale = 1;
        rigid.velocity = new Vector2(moveHzt * 3f, rigid.velocity.y + jumpPower);
    }

    // normal jumpping
    private void Jump()
    {
        anim.Play("Jump");
        rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y + jumpPower);
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
