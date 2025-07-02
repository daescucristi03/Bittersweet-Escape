using UnityEngine;
using System;

public class EnemyDespawnNotifier : MonoBehaviour
{
    public Action OnDeath;

    void OnDestroy()
    {
        OnDeath?.Invoke();
    }
}