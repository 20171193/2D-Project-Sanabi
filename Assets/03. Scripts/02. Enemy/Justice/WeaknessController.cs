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
    private bool isSpawnIdle = false;    // 모든 약점이 파괴되면 약점 생성 대기상태 진입
    public bool IsSpawnIdle 
    { 
        get 
        { 
            return isSpawnIdle; 
        } 
        set
        {
            isSpawnIdle = value;
        }
    }


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
            weaknesses[i].transform.position = transform.position + weaknesses[i].transform.up * 5;
            weaknesses[i].OnDestroyed += OnDestroyedWeakness;
            weaknessCount++;
        }
    }
    private void Update()
    {
        transform.position = owner.transform.position;

        Rotation();
        if (IsSpawnIdle &&
            owner.FSM.CurState != "Init" &&
            owner.FSM.CurState != "Teleport" &&
            owner.FSM.CurState != "Counter" &&
            owner.FSM.CurState != "Groggy")
        {
            Spawn();
        }
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

            IsSpawnIdle = true;
        }
    }

    public void DisAppearAll()
    {
        foreach (Weakness weakness in weaknesses)
        {
            if (weakness.FSM.CurState == "Destroy" || weakness.FSM.CurState == "Default") return;

            weakness.FSM.ChangeState("DisAppear");
        }
        isDisAppear = true;
    }
    public void AppearAll()
    {
        foreach (Weakness weakness in weaknesses)
        {
            if (weakness.FSM.CurState == "Destroy" || weakness.FSM.CurState == "Default") return;

            weakness.FSM.ChangeState("Appear");
        }
        isDisAppear = false;
    }

    private void Spawn()
    {
        Debug.Log("Spawn");

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
                if (owner.FSM.CurState != "Teleport" && 
                    (weakness.FSM.CurState == "Default" ||
                    weakness.FSM.CurState == "InActive"))
                {
                    weakness.FSM.ChangeState("Active");
                    break;
                }
            }
            // 일정시간에 맞춰 약점을 하나씩 활성화
            yield return new WaitForSeconds(activeRoutineTime);
        }
    }
}
