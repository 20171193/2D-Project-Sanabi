using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : PlayerBase
{
    [Header("Specs")]
    [SerializeField]
    private float ropeSkillPower;
    public float RopeSkillPower { get { return ropeSkillPower; } }

    [Header("Ballaincing")]
    private float dashPower;
    public float DashPower { get { return dashPower; } }

    private Coroutine dashCoroutine;

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
    public void Dash(IGrabable grabed)
    {
        playerFSM.IsDash = true;
        rigid.velocity = Vector3.zero;
        rigid.gravityScale = 0;

        dashCoroutine = StartCoroutine(DashTrailRoutine(grabed));

        playerFSM.ChangeState("Dash");
    }
    IEnumerator DashTrailRoutine(IGrabable grabed)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = grabed.GetGrabPosition();

        float time = Vector3.Distance(startPos, endPos) / dashPower;
        float rate = 0f;
        while(rate < 1f)
        {
            if (rate >= 0.3f)
                Time.timeScale = 0.5f;
            rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(startPos, endPos, rate);
            yield return null;
        }

        transform.position = endPos;
        Time.timeScale = 1f;
        Grab(grabed);
        yield return null;
    }
    public void Grab(IGrabable target)
    {
        playerFSM.IsDash = false;
        target.Grabbed();

        // Check Enemy
        playerHooker.GrabedObject = target;
        playerFSM.IsGrab = true;

        playerFSM.ChangeState("Grab");

        rigid.velocity = Vector3.zero;
    }
}
