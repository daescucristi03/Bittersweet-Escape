using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    protected Transform player;

    public virtual void SetPlayer(Transform p)
    {
        player = p;
    }
}