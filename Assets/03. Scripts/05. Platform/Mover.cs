using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : Platform
{
    [Header("Components")]
    [SerializeField]
    protected LineRenderer lr;

    [SerializeField]
    protected SpriteRenderer spr;

    [SerializeField]
    private BoxCollider2D groundCol;
    [SerializeField]
    private BoxCollider2D hookCol;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    protected Transform startTr;
    [SerializeField]
    protected Transform chainEndTr;
    [SerializeField]
    protected Transform endTr;

    protected Vector3 startPos;
    protected Vector3 chainEndPos;
    protected Vector3 endPos;

    [SerializeField]
    private float chainHeight; 
    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected float respawnTime;

    private Coroutine trailRoutine;

    private void Awake()
    {
        startPos = startTr.position;
        chainEndPos = chainEndTr.position;
        endPos = endTr.position;
    }

    private void OnEnable()
    {
        LineRendering();
    }

    public virtual void LineRendering()
    {
        lr.positionCount = 2;

        lr.SetPosition(0, new Vector3(startPos.x, startPos.y + chainHeight, 0));
        lr.SetPosition(1, new Vector3(chainEndPos.x, chainEndPos.y + chainHeight, 0));
    }

    protected virtual void Trail()
    {
        trailRoutine = StartCoroutine(TrailRoutine());
        anim.SetBool("IsMoving",true);
    }

    IEnumerator TrailRoutine()
    {
        // Get arrival time
        float time = Vector3.Distance(startPos, endPos) / moveSpeed;
        // Trail rate
        float rate = 0f;

        while(rate < 1f)
        {
            rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(startPos, endPos, rate);
            yield return null;
        }

        anim.SetBool("IsMoving", false);
        transform.position = endPos;
        yield return null;
    }

    IEnumerator RespawnRoutine()
    {
        if (trailRoutine != null)
            StopCoroutine(trailRoutine);

        anim.SetBool("IsEnable", false); 
        anim.SetBool("IsMoving", false);
        yield return new WaitForSeconds(0.2f);
        spr.enabled = false;
        groundCol.enabled = false;
        hookCol.enabled = false;
        transform.position = startPos;
        yield return new WaitForSeconds(respawnTime);
        Respawn();
    }
    private void Respawn()
    {
        anim.SetBool("IsEnable", true);
        spr.enabled = true;
        groundCol.enabled = true;
        hookCol.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
            Trail();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
            StartCoroutine(RespawnRoutine());
    }
}
