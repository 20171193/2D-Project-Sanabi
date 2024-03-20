using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    [SerializeField]
    protected Player player;
    public Player Player { get { return player; } }

    protected virtual void Awake()
    {
        player = GetComponent<Player>();
    }
}
