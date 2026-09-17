using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Registro central de los objetos de interés activos en la escena.
/// Los boids lo consultan para encontrar su objetivo más cercano vía Arrive.
/// </summary>
public class PointOfInterestManager : MonoBehaviour
{
    public static PointOfInterestManager Instance { get; private set; }

    private readonly List<PointOfInterest> activePOIs = new List<PointOfInterest>();

    public int ActiveCount => activePOIs.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(PointOfInterest poi)
    {
        if (!activePOIs.Contains(poi))
            activePOIs.Add(poi);
    }

    public void NotifyDestroyed(PointOfInterest poi)
    {
        activePOIs.Remove(poi);
    }

    public PointOfInterest GetClosestActivePOI(Vector3 fromPosition)
    {
        PointOfInterest closest = null;
        float closestDist = float.MaxValue;

        foreach (var poi in activePOIs)
        {
            if (poi == null || !poi.IsAlive) continue;

            float dist = (poi.transform.position - fromPosition).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = poi;
            }
        }

        return closest;
    }
}