using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    [Header("IsGround")]
    public LayerMask groundLM;
    [Header("IsWall")]
    public LayerMask wallLM;
    [Header("HookInteractable")]
    public LayerMask hookInteractableLM;
    [Header("Player Hook")]
    public LayerMask playerHookLM;
    [Header("Enemy")]
    public LayerMask enemyLM;
    [Header("Enemy Bullet")]
    public LayerMask enemyBulletLM;
}
