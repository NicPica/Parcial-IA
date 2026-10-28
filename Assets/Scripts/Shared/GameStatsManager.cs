using UnityEngine;
using System;

public class GameStatsManager : MonoBehaviour
{
    public static GameStatsManager Instance { get; private set; }

    public int BoidsCaught { get; private set; }

    public event Action<int> OnBoidCaught;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterBoidCaught()
    {
        BoidsCaught++;
        OnBoidCaught?.Invoke(BoidsCaught);
    }
}