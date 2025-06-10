using System;
using UnityEngine;

public static class Events
{
    public static event Action OnPlayerDeath;

    public static void TriggerPlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }
}
