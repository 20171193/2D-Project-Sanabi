using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : Platform
{
    [Header("Components")]
    [SerializeField]
    protected LineRenderer lr;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    protected Vector3 startPos;
    [SerializeField]
    protected Vector3 endPos;
    [SerializeField]
    protected float moveSpeed;

    private Coroutine trailRoutine;

    private void OnEnable()
    {
        startPos = transform.position;
        endPos = transform.position + endPos;
    }

    public virtual void LineRendering()
    {
        lr.positionCount = 2;

        lr.SetPosition(0, new Vector3(startPos.x, startPos.y - 1f, 0));
        lr.SetPosition(1, new Vector3(endPos.x, endPos.y-1f, 0));
    }

    private void Trail(bool isReturn)
    {
        if (trailRoutine != null)
            StopCoroutine(trailRoutine);

        // return to startPos
        if (isReturn)
            trailRoutine = StartCoroutine(TrailRoutine(transform.position, startPos));
        else
        {
            anim.SetTrigger("OnHitted");
            trailRoutine = StartCoroutine(TrailRoutine(transform.position, endPos));
        }    
    }

    IEnumerator TrailRoutine(Vector3 start, Vector3 end)
    {
        // Get arrival time
        float time = Vector3.Distance(start, end) / moveSpeed;
        // Trail rate
        float rate = 0f;

        while(rate < 1f)
        {
            rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(start, end, rate);
            yield return null;
        }

        transform.position = end;

        yield return null;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trail Enter");
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
        {
            Trail(false);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trail Exit");
        if (Manager.Layer.playerHookLM.Contain(collision.gameObject.layer))
        {            
            Trail(true);
        }
    }
}
