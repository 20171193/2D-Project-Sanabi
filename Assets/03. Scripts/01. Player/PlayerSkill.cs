using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : PlayerBase
{
    [Header("Component")]
    [SerializeField]
    private GameObject ropeForceParticle;

    [Header("Specs")]
    [SerializeField]
    private float ropeSkillPower;
    public float RopeSkillPower { get { return ropeSkillPower; } }

    [Header("Ballaincing")]
    [SerializeField]
    private float dashPower;
    public float DashPower { get { return dashPower; } }

    [SerializeField]
    private float slowMotionTime;

    private Coroutine dashCoroutine;
    private Coroutine ropeForceRoutine;

    protected override void Awake()
    {
        base.Awake();
    }

    public void RopeForce()
    {
        // Shift Key : Rope Skill

        // limit move power + skill power
        // reset when rope disconnection
        if (ropeForceRoutine != null)
            StopCoroutine(ropeForceRoutine);

        StartCoroutine(RopeForceRoutine());
        PrMover.CurrentMaxRopingPower += ropeSkillPower;

        // Add Force in the current direction
        rigid.AddForce(ropeSkillPower * rigid.velocity.normalized, ForceMode2D.Impulse);
    }
    IEnumerator RopeForceRoutine()
    {
        ropeForceParticle.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        ropeForceParticle.SetActive(false);
    }


    public void Dash(IGrabable grabed)
    {
        playerFSM.IsDash = true;
        rigid.velocity = Vector3.zero;
        rigid.gravityScale = 0;

        Vector3 grabedPos = grabed.GetGrabPosition();

        if(transform.position.x < grabedPos.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, -180, 0);

        playerFSM.ChangeState("Dash");

        dashCoroutine = StartCoroutine(DashTrailRoutine(grabed));
    }
    IEnumerator DashTrailRoutine(IGrabable grabed)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = grabed.GetGrabPosition();

        float time = Vector3.Distance(startPos, endPos) / dashPower;
        float rate = 0f;

        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        Time.timeScale = 0.5f;
        while(rate < 1f)
        {
            if (rate >= 0.6f && rate <= 0.7f)
                Manager.Camera.SetEventCamera();

            rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(startPos, endPos, rate);
            yield return null;
        }

        Manager.Camera.SetMainCamera();

        gameObject.layer = LayerMask.NameToLayer("Player");
        transform.position = endPos;
        Time.timeScale = 1f;
        Grab(grabed);
        yield return null;
    }
    public void Grab(IGrabable target)
    {
        playerFSM.IsDash = false;
        target.Grabbed(rigid);

        // Check Enemy
        playerHooker.GrabedObject = target;
        playerFSM.IsGrab = true;

        playerFSM.ChangeState("Grab");
    }
}
