using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class RailCar : Platform
{
    private BossRoomController pooler;
    public BossRoomController Pooler { get { return pooler; } set { pooler = value; } }

    [SerializeField]
    private BoxCollider2D groundCol;
    [SerializeField]
    private BoxCollider2D hookingCol;

    private void OnEnable()
    {
        groundCol.enabled = true;
        hookingCol.enabled = true;
    }

    private void Release()
    {
        groundCol.enabled = false;
        hookingCol.enabled = false;
        pooler.ReturnRailCar(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Manager.Layer.bossAttackLM.Contain(collision.gameObject.layer))
        {
            anim.SetTrigger("OnDestroy");
            Release();
        }
    }
}
