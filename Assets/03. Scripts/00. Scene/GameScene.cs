using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GameScene : MonoBehaviour
{
    [SerializeField]
    private PooledObject eneTrooperPrefab;
    [SerializeField]
    private PooledObject eneTurretPrefab;
    [SerializeField]
    private PooledObject enemyBullet;

    [SerializeField]
    private string[] loopAMB;
    [SerializeField]
    private string introBGM;
    [SerializeField]
    private string loopBGM;

    [SerializeField]
    private GameChapter curChapter;
    public GameChapter CurChapter { get { return curChapter; }  set { curChapter = value; } }

    public UnityEvent OnEnableScene;
    public UnityEvent OnDisableScene;

    private void Awake()
    {
        if(enemyBullet != null)
            Manager.Pool.CreatePool(enemyBullet, 15, 30);
        if(eneTurretPrefab != null)
            Manager.Pool.CreatePool(eneTrooperPrefab, 5, 10);
        if (enemyBullet != null)
            Manager.Pool.CreatePool(eneTurretPrefab, 10, 15);
    }
    private void OnEnable()
    {
        OnEnableScene?.Invoke();
    }
    private void OnDisable()
    {
        OnDisableScene?.Invoke();
    }

    private void Start()
    {
        foreach (string amb in loopAMB)
            Manager.Sound.PlaySound(SoundType.AMB, amb);

        Manager.Sound.PlaySound(SoundType.BGM, introBGM);
        StartCoroutine(
            Extension.DelayRoutine(Manager.Sound.BGMDic[introBGM].length,
            () => Manager.Sound.PlaySound(SoundType.BGM, loopBGM)));
    }
}
