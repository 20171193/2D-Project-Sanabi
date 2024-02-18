using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
public class PlayerAction : MonoBehaviour
{
    [Header("Components")]
    [Space(2)]

    [SerializeField]
    private Rigidbody2D rigid;

    [SerializeField]
    private SpriteRenderer spRenderer;

    [SerializeField]
    private Animator anim;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]

    [SerializeField]
    private float movePower;
    [SerializeField]
    private float maxMoveSpeed;

    [SerializeField]
    private float jumpPower;

    // �̵� �� �Ӱ�ġ
    const float MoveForce_Threshold = 0.1f;

    [Space(3)]
    [Header("Ballancing")]
    [Space(2)]
    // �¿� ������
    [SerializeField]
    private float hztBrakePower;
    // ���� ������
    [SerializeField]
    private float vtcBrakePower;

    [SerializeField]
    private float moveHzt;
    [SerializeField]
    private float moveVtc;
    [SerializeField]
    private float inputJumpPower;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {

        // �Է��� ���� ���
        if(moveHzt == 0)
        {
            // �극��ũ ����
            if (rigid.velocity.x > MoveForce_Threshold)
                rigid.AddForce(Vector2.left * hztBrakePower);
            else if (rigid.velocity.x < MoveForce_Threshold)
                rigid.AddForce(Vector2.right * hztBrakePower);
        }
        else
        {
            rigid.AddForce(Vector2.right * moveHzt * movePower);
            // �̵��ӵ� ����
            rigid.velocity = new Vector2(Mathf.Clamp(rigid.velocity.x, -maxMoveSpeed, maxMoveSpeed), rigid.velocity.y);
        }
    }

    private void Jump()
    {
        rigid.velocity = new Vector2(rigid.velocity.x, jumpPower);
    }

    #region Input Action
    private void OnMove(InputValue value)
    {
        moveHzt = value.Get<Vector2>().x;
        anim.SetFloat("MovePower", Mathf.Abs(moveHzt));
        moveVtc = value.Get<Vector2>().y;

        if (moveHzt > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (moveHzt < 0)
            transform.rotation = Quaternion.Euler(0, -180, 0);
    }

    private void OnJump(InputValue value)
    {
        Jump();
    }

    #endregion
}
