using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroller : Mover
{
    private Coroutine patrolRoutine;
    private bool isReturn = false;

    private Vector3 dirToStartPos;
    private Vector3 dirToEndPos;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnEnable()
    {
        dirToStartPos = (startPos - endPos).normalized;
        dirToEndPos = (endPos - startPos).normalized;
        base.LineRendering();
    }

    private void Update()
    {
        Patrolling();
    }
    private void Patrolling()
    {
        // 이동
        transform.Translate((isReturn? dirToStartPos : dirToEndPos) * moveSpeed * Time.deltaTime);

        // 도착지 확인
        if (isReturn && (startPos - transform.position).magnitude <= 0.1f)
            isReturn = false;
        else if (!isReturn && (endPos - transform.position).magnitude <= 0.1f)
            isReturn = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Patroller Trigger Enter");

        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
            anim.SetTrigger("OnHit");
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        
    }
}
