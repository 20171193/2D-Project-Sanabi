using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : PlayerBase
{
    [Header("Specs")]
    [SerializeField]
    private float ropeForceSkillPower;
    public float RopeForceSkillPower { get { return ropeForceSkillPower; } }

    public void RopeForce()
    {
        // 강한 반동 적용
        // 잔상 등 이펙트 추가
        Vector2 forceDir = transform.rotation.y == 0 ? Vector2.right : Vector2.left;
        rigid.AddForce(ropeForceSkillPower * forceDir, ForceMode2D.Impulse);
    }
    public void Dash(GameObject target)
    {
        isDash = true;
        rigid.velocity = Vector3.zero;
        rigid.gravityScale = 0;

        fsm.ChangeState("Dash");

        rigid.AddForce((target.transform.position - transform.position).normalized * 50f, ForceMode2D.Impulse);

        // add Player CCD
        // if player and enemy not collide,
        // Dash -> Idle 
    }
    public void Grab(GameObject target)
    {
        // Check Enemy
        grabEnemy = target.GetComponent<Enemy>();
        if (grabEnemy == null)
        {
            Debug.Log("Grabbed Object is not Enemy");
            fsm.ChangeState("Idle");
            return;
        }
        isGrab = true;
        fsm.ChangeState("Grab");

        grabEnemy.Grabbed(out float holdedYPos);

        rigid.velocity = Vector3.zero;
        Vector3 enemyPos = grabEnemy.transform.position;
        transform.position = new Vector3(enemyPos.x, enemyPos.y + holdedYPos, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Dash Enemy
        if (isDash && Manager.Layer.enemyLM.Contain(collision.gameObject.layer))
        {
            isDash = false;
            Grab(collision.gameObject);
        }
    }
}
