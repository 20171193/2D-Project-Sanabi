using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestManager : MonoBehaviour
{
    public UnityEvent OnTestEvent;
    public Vector3 startPos;

    [Header("Collider Parent")]
    [SerializeField]
    private Transform topCollider;
    private List<SpriteRenderer> sprList = new List<SpriteRenderer>();

    public void Start()
    {
        for (int i = 0; i < topCollider.childCount; i++)
        {
            Transform child = topCollider.GetChild(i);
            SpriteRenderer childSpr = child.GetComponent<SpriteRenderer>();
            if (childSpr != null)
                sprList.Add(childSpr);

            for (int j = 0; j < child.childCount; j++)
            {
                childSpr = child.GetChild(j).GetComponent<SpriteRenderer>();
                if (childSpr == null) continue;
                sprList.Add(childSpr);
            }
        }
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().transform.position = startPos;
        }
        if (Input.GetKey(KeyCode.Y))
        {
            ColliderDebugerSetting();
        }
    }

    public void ColliderDebugerSetting()
    {
        foreach (SpriteRenderer spr in sprList)
        {
            if (spr.enabled)
                spr.enabled = false;
            else
                spr.enabled = true;
        }
    }
}
