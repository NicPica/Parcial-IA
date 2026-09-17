using UnityEngine;
using System.Collections.Generic;

public class PointOfInterest : MonoBehaviour
{
    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float damagePerTick = 1f;

    private float currentHealth;
    private readonly List<Boid> attackingBoids = new List<Boid>();
    private float damageTimer;

    public bool IsAlive { get; private set; } = true;

    private void Awake()
    {
        currentHealth = maxHealth;
        PointOfInterestManager.Instance?.Register(this);
    }

    private void Update()
    {
        if (!IsAlive || attackingBoids.Count == 0) return;

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            currentHealth -= damagePerTick;

            if (currentHealth <= 0f)
                Destroy();
        }
    }

    public void BeginDamageOverTime(Boid boid)
    {
        if (!attackingBoids.Contains(boid))
            attackingBoids.Add(boid);
    }

    private void Destroy()
    {
        IsAlive = false;
        PointOfInterestManager.Instance?.NotifyDestroyed(this);
        Destroy(gameObject);
    }
}