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

    // 저스티스와 상호작용 액션
    public UnityAction OnAllWeaknessDestroyed;


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

    private void Start()
    {
        // 약점 오브젝트 초기위치 지정
        for (int i = 0; i < weaknesses.Length; i++)
        {
            Vector3 rot = new Vector3(0, 0, 360 / 3 * i);
            weaknesses[i].transform.Rotate(rot);
            weaknesses[i].transform.position = transform.position + weaknesses[i].transform.up * 5;
            weaknesses[i].OnDestroyed += OnDestroyedWeakness;
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
            OnAllWeaknessDestroyed?.Invoke();
            IsSpawnIdle = true;
        }
    }

    public void DisAppearAll()
    {
        foreach (Weakness weakness in weaknesses)
        {
            if (weakness.FSM.CurState == "Destroy" || weakness.FSM.CurState == "Default") return;
            Debug.Log("Weakness DisAppear");
            weakness.FSM.ChangeState("DisAppear");
        }
        isDisAppear = true;
    }
    public void AppearAll()
    {
        foreach (Weakness weakness in weaknesses)
        {
            if (weakness.FSM.CurState == "Destroy" || weakness.FSM.CurState == "Default") return;
            Debug.Log("Weakness Appear");
            weakness.FSM.ChangeState("Appear");
            if (!weakness.IsActive)
                StartCoroutine(Extension.DelayRoutine(2f, () => weakness.FSM.ChangeState("Active")));
        }
        isDisAppear = false;
    }

    private void Spawn()
    {
        Debug.Log("Spawn");

        isSpawnIdle = false;
        foreach (Weakness weakness in weaknesses)
        {
            weaknessCount++;
            weakness.FSM.ChangeState("Appear");
            if (!weakness.IsActive)
                StartCoroutine(Extension.DelayRoutine(2f, () => weakness.FSM.ChangeState("Active")));
        }
        isDisAppear = false;
    }
}
