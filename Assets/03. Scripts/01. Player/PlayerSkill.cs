using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSkill : PlayerBase
{
    [Header("Component")]
    [SerializeField]
    private ParticleSystem ropeForceParticle;

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

    protected override void Awake()
    {
        base.Awake();
    }

    public void RopeForce()
    {
        // Shift Key : RopeForceSkill
        if (!isEnableRopeForce) return;

        // 현재 로프 연결 중 로프파워스킬은 한번 만 사용가능.
        // 다음 로프 연결에서 다시 사용가능.
        isEnableRopeForce = true; // PlayerHooker의 OnHookDestroyed 호출 시 false로 변경

        // 로프 최대 속도에 로프파워속도만큼 더해 스킬 사용중 최대 속도를 증가시킴
        // 로프 루틴(코루틴) 종료 시 초기화
        if (ropeForceRoutine != null)
            StopCoroutine(ropeForceRoutine);

        ropeForceRoutine = StartCoroutine(RopeForceRoutine());

        // 잔상 파티클 렌더러 플립
        if (transform.rotation.y == 0)
            ropeForceParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(0, 0, 0);
        else
            ropeForceParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(1, 0, 0);

        PrFSM.OnRopeForceStart?.Invoke();
        PrMover.CurrentMaxRopingPower += ropeSkillPower;

        // 현재 플레이어가 바라보고있는 방향으로 로프파워를 가해줌
        rigid.AddForce(ropeSkillPower * transform.right, ForceMode2D.Impulse);
    }
    IEnumerator RopeForceRoutine()
    {

        ropeForceParticle.Play();
        yield return new WaitForSeconds(0.5f);
        PrMover.CurrentMaxRopingPower -= ropeSkillPower;
        ropeForceParticle.Stop();
    }


    public void Dash(IGrabable grabed)
    {
        // 훅이 벽이 아닌 상호작용 가능한 오브젝트에 닿은 경우 대쉬스킬 발동
        playerFSM.IsDash = true;

        // 현재 속력과 중력을 0으로 변경
        rigid.velocity = Vector3.zero;
        rigid.gravityScale = 0;

        Vector3 grabedPos = grabed.GetGrabPosition();
        
        // 닿은 오브젝트의 위치와 플레이어 위치를 계산해 플레이어 회전
        if(transform.position.x < grabedPos.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, -180, 0);

        playerFSM.ChangeState("Dash");
        
        // 닿은 오브젝트까지 트레일링 (물리 이동이 아닌 선형보간 이동방식) 
        dashCoroutine = StartCoroutine(DashTrailRoutine(grabed));
    }
    IEnumerator DashTrailRoutine(IGrabable grabed)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = grabed.GetGrabPosition();

        float time = Vector3.Distance(startPos, endPos) / dashPower;
        float rate = 0f;

        // 대쉬 중 플레이어 무적상태 적용(레이어 변경)
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        
        // 슬로우 연출
        Time.timeScale = 0.5f;
        while(rate < 1f)
        {
            // 카메라 시점 변경
            if (rate >= 0.6f && rate <= 0.7f)
                Manager.Camera.SetEventCamera();

            rate += Time.deltaTime / time;
            transform.position = Vector3.Lerp(startPos, endPos, rate);
            yield return null;
        }

        // 카메라 시점 원상복구
        Manager.Camera.SetMainCamera();

        gameObject.layer = LayerMask.NameToLayer("Player");
        transform.position = endPos;
        Time.timeScale = 1f;
        
        // 목표지점에 도착한 뒤 닿은 오브젝트를 그랩
        Grab(grabed);
        yield return null;
    }
    public void Grab(IGrabable target)
    {
        // 대쉬에서 전환된 그랩
        playerFSM.IsDash = false;

        target.Grabbed(rigid);

        // 잡은 오브젝트를 Hooker에 할당.
        playerHooker.GrabedObject = target;
        playerFSM.IsGrab = true;

        playerFSM.ChangeState("Grab");
    }
}
