using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    [Header("Ground / Wall")]
    [Space(3)]
    #region Ground / Wall
    [Header("IsGround")]
    public LayerMask groundLM;
    [Header("IsDamageGround")]
    public LayerMask damageGroundLM;
    [Header("IsWall")]
    public LayerMask wallLM;
    #endregion

    [Space(3)]
    [Header("Hook")]
    [Space(3)]
    #region Hook
    public LayerMask hookInteractableLM;
    public LayerMask hookingGroundLM;
    public LayerMask hookingPlatformLM;
    public LayerMask playerHookLM;
    public LayerMask rayBlockObjectLM;
    #endregion

    [Space(3)]
    [Header("Enemy")]
    [Space(3)]
    #region Enemy
    public LayerMask enemyLM;
    public LayerMask enemyBulletLM;
    #endregion

    [Space(3)]
    [Header("Boss")]
    [Space(3)]
    #region Boss
    public LayerMask bossLM;
    public LayerMask bossAttackLM;
    public LayerMask bossWeaknessLM;
    #endregion
}
