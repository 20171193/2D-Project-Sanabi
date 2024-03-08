using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : Platform
{
    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Animator anim;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    private Vector3 startPos;
    [SerializeField]
    private Vector3 endPos;
    [SerializeField]
    private float moveSpeed;

    private Coroutine trailRoutine;

    private void OnEnable()
    {
        startPos = transform.position;
        endPos = transform.position + endPos;

        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(startPos.x, startPos.y-1f, 0));
        lr.SetPosition(1, endPos);
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
