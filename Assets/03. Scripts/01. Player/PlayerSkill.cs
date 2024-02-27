using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : PlayerBase
{
    [Header("Specs")]
    [SerializeField]
    private float ropeSkillPower;
    public float RopeSkillPower { get { return ropeSkillPower; } }

    protected override void Awake()
    {
        base.Awake();
    }

    public void RopeForce()
    {
        // 강한 반동 적용
        // 잔상 등 이펙트 추가
        Vector2 forceDir = transform.rotation.y == 0 ? Vector2.right : Vector2.left;
        rigid.AddForce(ropeSkillPower * forceDir, ForceMode2D.Impulse);
    }
    public void Dash(GameObject target)
    {
        playerFSM.IsDash = true;
        rigid.velocity = Vector3.zero;
        rigid.gravityScale = 0;

        playerFSM.ChangeState("Dash");

        rigid.AddForce((target.transform.position - transform.position).normalized * 50f, ForceMode2D.Impulse);

        // add Player CCD
        // if player and enemy not collide,
        // Dash -> Idle 
    }
    public void Grab(GameObject target)
    {
        // Check Enemy
        playerHooker.GrabEnemy = target.GetComponent<Enemy>();
        if (playerHooker.GrabEnemy == null)
        {
            Debug.Log("Grabbed Object is not Enemy");
            playerFSM.ChangeState("Idle");
            return;
        }
        playerFSM.IsGrab = true;
        playerFSM.ChangeState("Grab");

        playerHooker.GrabEnemy.Grabbed(out float holdedYPos);

        rigid.velocity = Vector3.zero;
        Vector3 enemyPos = playerHooker.GrabEnemy.transform.position;
        transform.position = new Vector3(enemyPos.x, enemyPos.y + holdedYPos, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Dash Enemy
        if (playerFSM.IsDash && Manager.Layer.enemyLM.Contain(collision.gameObject.layer))
        {
            playerFSM.IsDash = false;
            Grab(collision.gameObject);
        }
    }
}
