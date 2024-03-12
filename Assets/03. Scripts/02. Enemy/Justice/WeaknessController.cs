using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaknessController : MonoBehaviour
{
    [Header("Justice")]
    [Space(2)]
    [SerializeField]
    private Justice owner;

    [Space(3)]
    [Header("Weakness")]
    [Space(2)]
    [SerializeField]
    private Weakness[] weaknesses;

    [Space(3)]
    [Header("Specs")]
    [Space(2)]
    [SerializeField]
    private float rotSpeed;
    public float RotSpeed { get { return rotSpeed; } set { rotSpeed = value; } }

    [Space(3)]
    [Header("Balancing")]
    [Space(2)]
    [SerializeField]
    private bool isSpawnIdle = true;    // 모든 약점이 파괴되면 약점 생성 대기상태 진입

    [SerializeField]
    private bool isDisAppear = false;   // 약점이 꺼진 상태인지? 
    public bool IsDisAppear { get { return isDisAppear; } }

    [SerializeField]
    private int weaknessCount = 0;

    [SerializeField]
    private float activeRoutineTime;

    private Coroutine activeRoutine;

    private void Start()
    {
        // 약점 오브젝트 초기위치 지정
        for (int i = 0; i < weaknesses.Length; i++)
        {
            Vector3 rot = new Vector3(0, 0, 360 / 3 * i);
            weaknesses[i].transform.Rotate(rot);
            weaknesses[i].transform.Translate(weaknesses[i].transform.up * 5);
            weaknesses[i].OnDestroyed += OnDestroyedWeakness;
            weaknessCount++;
        }
    }
    private void Update()
    {
        Rotation();
        if (isSpawnIdle 
            && owner.FSM.CurState != "Teleport" 
            && owner.FSM.CurState != "Grabing")
            Spawn();
    }
    private void Rotation()
    {
        transform.Rotate(new Vector3 (0, 0, rotSpeed * Time.deltaTime));
    }

    public void OnDestroyedWeakness()
    {
        weaknessCount--;
        if (weaknessCount <= 0)
        {
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            isSpawnIdle = true;
        }
    }

    public void DisAppearAll()
    {
        foreach (Weakness weakness in weaknesses)
        {
            weakness.FSM.ChangeState("DisAppear");
        }
        isDisAppear = true;
    }
    public void AppearAll()
    {
        foreach (Weakness weakness in weaknesses)
        {
            weakness.FSM.ChangeState("Appear");
        }
        isDisAppear = false;
    }

    private void Spawn()
    {
        isSpawnIdle = false;
        AppearAll();

        activeRoutine = StartCoroutine(AcitveRoutine());
    }

    IEnumerator AcitveRoutine()
    {
        while(owner.FSM.CurState != "Die")
        {
            foreach (Weakness weakness in weaknesses)
            {
                if (!weakness.IsActive)
                {
                    Debug.Log("Active Weakness");
                    weakness.FSM.ChangeState("Active");
                    break;
                }
            }
            // 일정시간에 맞춰 약점을 하나씩 활성화
            yield return new WaitForSeconds(activeRoutineTime);
        }
    }
}
