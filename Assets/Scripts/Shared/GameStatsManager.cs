using UnityEngine;
using System;

/// <summary>
/// Lleva estadísticas globales de la simulación, como la cantidad de boids
/// cazados por el Hunter. Otros sistemas se suscriben a OnBoidCaught para reaccionar
/// (por ejemplo, actualizar la UI) sin acoplarse directamente entre sí.
/// </summary>
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