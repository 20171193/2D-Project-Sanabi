using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DeathType
{
    DeadZone,
    Damaged
}

public class EventController : MonoBehaviour
{
    [SerializeField]
    private DeathEvent deathByDeadzone;
    [SerializeField]
    private DeathEvent deathByDamaged;

    public void ActiveDeathEvent(DeathType type, bool isActive)
    {
        switch(type)
        {
            case DeathType.DeadZone:
                deathByDeadzone.SetAnimator(isActive);
                break;
            case DeathType.Damaged:
                deathByDamaged.SetAnimator(isActive);
                break;
            default:
                Debug.Log("Error(ActiveDeathEvent) :  Death 타입이 설정되지 않았습니다.");
                return;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            ActiveDeathEvent(DeathType.DeadZone, true);
        if (Input.GetKeyDown(KeyCode.W))
            ActiveDeathEvent(DeathType.Damaged, true);
    }
}
