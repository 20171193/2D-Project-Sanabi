using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Holder : Platform
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rigid;

    [Header("Holding")]
    [SerializeField]
    private float holdingYpos;
    [SerializeField]
    private float holdingSpeed;

    private bool isHolding = false;

    private void Holding()
    {
        Debug.Log("Holding");
        anim.SetTrigger("OnHold");
        rigid.AddForce(Vector2.down * holdingSpeed, ForceMode2D.Impulse);
        StartCoroutine(HoldingRoutine());
    }

    IEnumerator HoldingRoutine()
    {
        float time = -holdingYpos / holdingSpeed;
        float rate = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position;
        endPos.y += holdingYpos;

        while (rate < 1f)
        {
            if (rate < 0.7f)
                rate += (Time.deltaTime / time) * 2f;
            else
                rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(startPos, endPos, rate);
            yield return null;
        }

        transform.position = endPos;
        yield return null;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHolding) return;

        // 플레이어가 부딪히거나, 플레이어 훅에 닿은 경우 홀딩
        if (collision.tag == "Player" || Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
        {
            isHolding = true;
            Holding();
        }
    }
}
