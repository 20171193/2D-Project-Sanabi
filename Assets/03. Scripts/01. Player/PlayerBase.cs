using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    [HideInInspector]
    public const float MoveForce_Threshold = 0.1f;
    [HideInInspector]
    public const float JumpForce_Threshold = 0.05f;

    // Linked Class
    protected PlayerFSM playerFSM;
    public PlayerFSM PrFSM { get { return playerFSM; }  }

    protected PlayerMover playerMover;
    public PlayerMover PrMover { get { return playerMover; } }

    protected PlayerHooker playerHooker;
    public PlayerHooker PrHooker { get { return playerHooker; } }

    protected PlayerSkill playerSkill;
    public PlayerSkill PrSkill { get { return playerSkill; } }

    protected PlayerObjectPool playerObjectPool;
    public PlayerObjectPool PrObjectPool { get { return playerObjectPool; } }

    [Header("Components")]
    [Space(2)]
    [SerializeField]
    protected Rigidbody2D rigid;
    public Rigidbody2D Rigid { get { return rigid; } }

    [SerializeField]
    protected Animator anim;
    public Animator Anim { get { return anim; } }
    [SerializeField]
    protected Camera cam;

    protected virtual void Awake()
    {
        playerFSM = GetComponent<PlayerFSM>();
        playerMover = GetComponent<PlayerMover>();
        playerHooker = GetComponent<PlayerHooker>();
        playerSkill= GetComponent<PlayerSkill>();   

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cam = Camera.main;
    }
}
