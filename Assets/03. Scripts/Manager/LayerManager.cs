using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    [Header("IsGround Check")]
    public LayerMask groundLM;
    [Header("HookGround Check")]
    public LayerMask hookGroundLM;
    [Header("Enemy Check")]
    public LayerMask enemyLM;
}
