using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public const float MoveForce_Threshold = 0.1f;
    [HideInInspector]
    public const float JumpForce_Threshold = 0.05f;

    // Linked Class
    [SerializeField]
    private PlayerFSM playerFSM;
    public PlayerFSM PrFSM { get { return playerFSM; } }

    [SerializeField]
    private PlayerMover playerMover;
    public PlayerMover PrMover { get { return playerMover; } }

    [SerializeField]
    private PlayerHooker playerHooker;
    public PlayerHooker PrHooker { get { return playerHooker; } }

    [SerializeField]
    private PlayerSkill playerSkill;
    public PlayerSkill PrSkill { get { return playerSkill; } }

    [Header("Components")]
    [Space(2)]
    [SerializeField]
    private Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }

    [SerializeField]
    private Animator anim;
    public Animator Anim { get { return anim; } }

    [SerializeField]
    private Camera cam;
    public Camera Cam { get { return cam; } }

    [SerializeField]
    private CinemachineImpulseSource impulseSource;
    public CinemachineImpulseSource ImpulseSource { get { return impulseSource; } }

    [SerializeField]
    private PlayerVFXPooler playerVFXPooler;
    public PlayerVFXPooler PrVFX { get { return playerVFXPooler; } }

    [Space(3)]
    [Header("Player Action Events")]
    [Space(2)]
    public UnityEvent OnRun;            // invoke by state
    public UnityEvent OnJump;           // invoke by PlayerMover (Jump())
    public UnityEvent OnWallJump;       // invoke by PlayerMover (WallJump())
    public UnityEvent OnHitJump;        // invoke by PlayerMover (HitJump())
    public UnityEvent OnDash;           // invoke by state
    public UnityEvent OnRopeForceStart; // invoke by PlayerSkill (RopeForce())
    public UnityEvent OnRopeForceEnd;   // invoke by PlayerHooker (OnHookDestroyed())
    public UnityEvent OnGrabEnd;        // invoke by state
    public UnityEvent OnClimb;          // invoke by state
    public UnityEvent OnWallSliding;    // invoke by state
    public UnityEvent OnHookShoot;      // invoke by PlayerHooker (HookShoot)
    public UnityEvent OnTakeDamage;     // invoke by state
    public UnityEvent OnLanding;        // invoke by PlayerMover (TriggerEnter2D)

    private Coroutine takeDamageRoutine;
    public Coroutine TakeDamageCoroutine { get { return takeDamageRoutine; } }

    private void Awake()
    { 
        impulseSource = GetComponent<CinemachineImpulseSource>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cam = Camera.main;

        // Assign linked class
        playerMover = GetComponent<PlayerMover>();
        playerFSM = GetComponent<PlayerFSM>();
        playerHooker = GetComponent<PlayerHooker>();
        playerSkill = GetComponent<PlayerSkill>();
    }

    public void DoImpulse()
    {
        impulseSource.GenerateImpulse();
    }

    private void TakeDamage()
    {
        DoImpulse();

        takeDamageRoutine = StartCoroutine(TakeDamageRoutine());

        // �������� : ? -> Damaged
        PrFSM.ChangeState("Damaged");
        PrHooker.FiredHook?.DisConnecting();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            DoImpulse();
    }

    // ��Ʈ ������ ������ ��ƾ�� �������� ���
    // �۸�ġ ����Ʈ �ʱ�ȭ, �������� �ʱ�ȭ
    public void InitDamageRoutine()
    {
        if (takeDamageRoutine != null)
            StopCoroutine(takeDamageRoutine);
        // �۸�ġ ����Ʈ ��Ȱ��ȭ
        Camera.main.GetComponent<GlitchEffect>().enabled = false;
        // ���̾� ����
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    IEnumerator TakeDamageRoutine()
    {
        // �������·� ����
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        // ī�޶� �۸�ġ ����Ʈ
        Camera.main.GetComponent<GlitchEffect>().enabled = true;
        yield return new WaitForSeconds(0.3f);
        Camera.main.GetComponent<GlitchEffect>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        PrFSM.ChangeState("Idle");
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private bool CheckGround(GroundType groundType)
    {
        RaycastHit2D hit;
        Vector2 rayDir = Vector2.zero;
        LayerMask layerMask = 0;
        float rayLength = 2f;

        switch (groundType)
        {
            case GroundType.Ground:
                layerMask = Manager.Layer.groundLM;
                rayDir = Vector2.down;
                break;
            case GroundType.HookingGround:
                layerMask = Manager.Layer.hookingGroundLM;
                rayDir = Vector2.up;
                break;
            case GroundType.Wall:
                layerMask = Manager.Layer.wallLM;
                rayDir = transform.right;
                break;
            default:
                return false;
        }

        hit = Physics2D.Raycast(transform.position, rayDir, rayLength, layerMask);
        if (hit)
            Debug.Log($"Type Change {groundType}");
        return hit;
    }

    #region Collision / Trigger

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //fsm.ChangeState(fsm.CurState);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger enter");

        // �켱���� 
        // õ�� > �ٴ� > ��

        // õ�� üũ : õ����ŷ
        if (PrFSM.CeilingChecker.activeSelf &&
            Manager.Layer.hookingGroundLM.Contain(collision.gameObject.layer) &&
            CheckGround(GroundType.HookingGround))
        {
            PrHooker.FiredHook?.DisConnecting();
            PrFSM.IsCeilingStick = true;
            Rigid.velocity = Vector3.zero;

            PrFSM.ChangeState("CeilingStickIdle");
            return;
        }

        // �� üũ : ��Ÿ�� 
        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer) && CheckGround(GroundType.Wall))
        {
            Debug.Log("Trigger wall");
            PrFSM.IsCeilingStick = false;
            PrHooker.FiredHook?.DisConnecting();

            PrFSM.IsInWall = true;
            PrFSM.ChangeState("WallSlide");
            return;
        }
        // �ٴ� üũ 
        if (Manager.Layer.groundLM.Contain(collision.gameObject.layer) && CheckGround(GroundType.Ground))
        {
            Debug.Log("Check Ground");
            // �������� ���̱� ���� ����Ʈ ����
            Rigid.velocity = new Vector2(Rigid.velocity.x, -0.01f);
            // ground check
            OnLanding?.Invoke();

            if (PrFSM.IsJointed)
                PrHooker.FiredHook?.DisConnecting();
            if (PrFSM.IsCeilingStick)
                PrFSM.IsCeilingStick = false;

            PrFSM.IsGround = true;
            return;
        }


        if (Manager.Layer.damageGroundLM.Contain(collision.gameObject.layer))
        {
            // ��� ���� Ż��
            PrFSM.IsInWall = false;
            PrFSM.IsCeilingStick = false;

            Rigid.velocity = Vector3.zero;
            Rigid.AddForce(collision.gameObject.transform.right * 12f + Rigid.transform.right * -5f, ForceMode2D.Impulse);
            TakeDamage();
            return;
        }

        if (Manager.Layer.enemyBulletLM.Contain(collision.gameObject.layer) ||
            (Manager.Layer.bossAttackLM.Contain(collision.gameObject.layer)))
        {
            TakeDamage();
            return;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trigger exit");

        if (Manager.Layer.groundLM.Contain(collision.gameObject.layer))
        {
            PrFSM.IsGround = false;
        }

        if (PrFSM.CeilingChecker.activeSelf &&
            Manager.Layer.hookingGroundLM.Contain(collision.gameObject.layer))
        {
            Debug.Log("Change fall");
            PrFSM.IsCeilingStick = false;
            PrFSM.ChangeState("Fall");
            return;
        }

        if (Manager.Layer.wallLM.Contain(collision.gameObject.layer))
        {
            PrFSM.IsInWall = false;
            //PrHooker.FiredHook?.DisConnecting();

            // ���� ������ ���¿��� Ż������ ���
            if (PrMover.MoveVtc > 0 && PrFSM.FSM.CurState == "WallSlide")
            {
                PrFSM.ChangeState("Jump");
                // Wall Exit to Up Position
                OnWallJump?.Invoke();
                Rigid.AddForce(transform.up * 10f, ForceMode2D.Impulse);
            }
        }
    }
    #endregion
}
