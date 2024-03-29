using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSkill : PlayerBase
{
    [Header("Component")]
    [SerializeField]
    private ParticleSystem ropeForceParticle;
    [SerializeField]
    private ParticleSystem ceilingStickParticle;

    [Header("Specs")]
    [SerializeField]
    private float ropeSkillPower;
    public float RopeSkillPower { get { return ropeSkillPower; } }

    [Header("Ballaincing")]
    private bool isEnableRopeForce = true;
    public bool IsEnableRopeForce { get { return isEnableRopeForce; } set { isEnableRopeForce = value; } }

    [SerializeField]
    private float dashPower;
    public float DashPower { get { return dashPower; } }

    [SerializeField]
    private float slowMotionTime;

    private Coroutine dashCoroutine;
    private Coroutine ropeForceRoutine;
    private Coroutine ghostTrailRoutine;    // 잔상효과
    private Coroutine ceilingStickRoutine;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        ropeForceParticle.gameObject.transform.position = transform.position - transform.right;
        ceilingStickParticle.gameObject.transform.position = transform.position - transform.up * 1f;
    }

    // 로프파워 스킬
    public void RopeForce()
    {
        // Shift Key : RopeForceSkill
        if (!isEnableRopeForce) return;

        // 현재 로프 연결 중 로프파워스킬은 한번 만 사용가능.
        // 다음 로프 연결에서 다시 사용가능.
        isEnableRopeForce = false; // PlayerHooker의 OnHookDestroyed 호출 시 true

        // 로프 최대 속도에 로프파워속도만큼 더해 스킬 사용중 최대 속도를 증가시킴
        // 로프 루틴(코루틴) 종료 시 초기화
        if (ghostTrailRoutine != null)
            StopCoroutine(ghostTrailRoutine);

        // 잔상 파티클 출력
        ghostTrailRoutine = StartCoroutine(GhostTrailRoutine(0.5f, ropeForceParticle, () => Player.PrMover.CurrentMaxRopingPower -= ropeSkillPower));

        // 잔상 파티클 렌더러 플립
        if (transform.rotation.y == 0)
            ropeForceParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(0, 0, 0);
        else
            ropeForceParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(1, 0, 0);

        Player.OnRopeForceStart?.Invoke();
        Player.PrMover.CurrentMaxRopingPower += ropeSkillPower;

        // 현재 플레이어가 바라보고있는 방향으로 로프파워를 가해줌
        Player.Rigid.AddForce(ropeSkillPower * transform.right, ForceMode2D.Impulse);
    }

    // 대쉬 스킬
    public void Dash(IGrabable grabed)
    {
        // 현재 속력과 중력을 0으로 변경
        Player.Rigid.velocity = Vector3.zero;
        Player.Rigid.gravityScale = 0;

        Vector3 grabedPos = grabed.GetGrabPosition();

        // 플레이어 대시 vfx 설정
        GameObject vfx = Player.PrVFX.GetVFX("PlayerDash");
        Vector3 vec = grabedPos - transform.position;
        vfx.transform.right = vec.normalized;
        vfx.transform.position = transform.position + vec.normalized * 2f;


        // 닿은 오브젝트의 위치와 플레이어 위치를 계산해 플레이어 회전
        if (transform.position.x < grabedPos.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, -180, 0);

        Player.PrFSM.ChangeState("Dash");

        // 닿은 오브젝트까지 트레일링(물리 이동이 아닌 선형보간 이동방식)
        //dashCoroutine = StartCoroutine(DashTrailRoutine(grabed));


        // 훅 DistanceJoint2D의 distance를 줄이며 이동하는 방식
        //Vector3 dir = (grabedPos - transform.position).normalized;
        //rigid.AddForce(dir * 50f, ForceMode2D.Impulse);
        dashCoroutine = StartCoroutine(DashTrail(grabed));
    }
    // fix
    IEnumerator DashTrail(IGrabable grabed)
    {
        DistanceJoint2D distJoint = Player.PrHooker.FiredHook.DistJoint;

        // 대쉬 중 플레이어 무적상태 적용(레이어 변경)
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        while (distJoint.distance > 0.1f)
        {
            distJoint.distance -= dashPower*Time.deltaTime;
            yield return null;
        }
        // 발사한 훅 비활성화
        Player.PrHooker.FiredHook.DisConnecting();
        transform.position = grabed.GetGrabPosition();
        gameObject.layer = LayerMask.NameToLayer("Player");
        Debug.Log("object Grabstart");
        Grab(grabed);
        yield return null;
    }
    //// 대쉬 트레일링
    //IEnumerator DashTrailRoutine(IGrabable grabed)
    //{
    //    Vector3 startPos = transform.position;
    //    Vector3 endPos = grabed.GetGrabPosition();

    //    float time = Vector3.Distance(startPos, endPos) / dashPower;
    //    float rate = 0f;

    //    // 대쉬 중 플레이어 무적상태 적용(레이어 변경)
    //    gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");

    //    // 슬로우 연출
    //    Time.timeScale = 0.5f;
    //    while (rate < 1f)
    //    {
    //        // 카메라 시점 변경
    //        if (rate >= 0.6f && rate <= 0.7f)
    //            Manager.Camera.SetEventCamera();

    //        rate += Time.deltaTime / time;
    //        transform.position = Vector3.Lerp(startPos, endPos, rate);
    //        yield return null;
    //    }

    //    // 카메라 시점 원상복구
    //    Manager.Camera.SetMainCamera();

    //    gameObject.layer = LayerMask.NameToLayer("Player");
    //    transform.position = endPos;
    //    Time.timeScale = 1f;

    //    // 목표지점에 도착한 뒤 닿은 오브젝트를 그랩
    //    Grab(grabed);
    //    yield return null;
    //}

    // 그랩 스킬
    public void Grab(IGrabable target)
    {
        target.Grabbed(Player.Rigid);

        // 잡은 오브젝트를 Hooker에 할당.
        Player.PrHooker.GrabedObject = target;
        Debug.Log(target);
        Player.PrFSM.ChangeState("Grab");
    }

    // 그랩대쉬 스킬 
    public void CeilingStick()
    {
        Player.OnCeilingStickStart?.Invoke();

        Player.PrFSM.IsCeilingStick = true;
        Player.PrFSM.IsGround = false;
        Player.PrFSM.ChangeState("CeilingStickStart");

        // 닿은 오브젝트의 위치와 플레이어 위치를 계산해 플레이어 회전
        if (transform.position.x < Player.PrHooker.FiredHook.transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, -180, 0);

        ghostTrailRoutine = StartCoroutine(GhostTrailRoutine(0.2f, ceilingStickParticle,  null));
        Player.PrHooker.FiredHook.DistJoint.distance = 0.8f;
        // 상태 전이는 PlayerFSM의 OnTriggerEnter2D에서 실행
        // CeilingCheck와 충돌 시 -> CeilingStickIdle로 전환
    }

    // 잔상 루틴
    IEnumerator GhostTrailRoutine(float activeTime, ParticleSystem particle, UnityAction afterAction)
    {
        particle.Play();
        yield return new WaitForSeconds(activeTime);
        afterAction?.Invoke();
        particle.Stop();
    }

}
