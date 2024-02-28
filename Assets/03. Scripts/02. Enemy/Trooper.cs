using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTrooper : EnemyShooter
{
    protected override void Awake()
    {
        base.Awake();

        fsm.AddState("Detect", new TrooperDetect(this));
        fsm.AddState("Grabbed", new TrooperGrabbed(this));
        fsm.AddState("Die", new TrooperDie(this));

        fsm.Init("Detect");
    }

    public override void Died()
    {
        Destroy(gameObject, 3f);
        fsm.ChangeState("Die");
    }
    public override void Grabbed(out float holdingYpoint)
    {
        holdingYpoint = grabbedYPos;
        fsm.ChangeState("Grabbed");
    }
}
