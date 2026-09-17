using UnityEngine;

/// <summary>
/// Conjunto de comportamientos de steering puros y reutilizables.
/// No dependen de ningún agente en particular: reciben datos y devuelven una fuerza.
/// </summary>
public static class SteeringBehaviors
{
    /// <summary>
    /// Aleja al agente de sus vecinos cercanos (rango de percepción menor que Alignment/Cohesion).
    /// </summary>
    public static Vector3 Separation(Vector3 myPos, Transform[] neighbors, int neighborCount, float separationRadius)
    {
        Vector3 force = Vector3.zero;
        int count = 0;

        for (int i = 0; i < neighborCount; i++)
        {
            Transform other = neighbors[i];
            if (other == null) continue;

            Vector3 toMe = myPos - other.position;
            float dist = toMe.magnitude;

            if (dist > 0f && dist < separationRadius)
            {
                // Cuanto más cerca, más fuerte empuja (inversamente proporcional a la distancia)
                force += toMe.normalized / dist;
                count++;
            }
        }

        if (count > 0) force /= count;
        return force;
    }

    /// <summary>
    /// Alinea la dirección de movimiento con el promedio de velocidades de los vecinos.
    /// </summary>
    public static Vector3 Alignment(Vector3[] neighborVelocities, int neighborCount)
    {
        if (neighborCount == 0) return Vector3.zero;

        Vector3 avgVelocity = Vector3.zero;
        for (int i = 0; i < neighborCount; i++)
            avgVelocity += neighborVelocities[i];

        avgVelocity /= neighborCount;
        return avgVelocity.normalized;
    }

    /// <summary>
    /// Dirige al agente hacia el centro de masa de sus vecinos.
    /// </summary>
    public static Vector3 Cohesion(Vector3 myPos, Vector3[] neighborPositions, int neighborCount)
    {
        if (neighborCount == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        for (int i = 0; i < neighborCount; i++)
            center += neighborPositions[i];

        center /= neighborCount;
        return (center - myPos).normalized;
    }

    /// <summary>
    /// Se acerca a un objetivo desacelerando a medida que se aproxima, para no superponerse.
    /// </summary>
    public static Vector3 Arrive(Vector3 myPos, Vector3 targetPos, Vector3 currentVelocity, float maxSpeed, float slowRadius, float stopRadius)
    {
        Vector3 toTarget = targetPos - myPos;
        float distance = toTarget.magnitude;

        if (distance < stopRadius)
        {
            // Frenado activo: fuerza opuesta a la velocidad actual para detenerlo de verdad
            return -currentVelocity * 2f;
        }

        float speed = maxSpeed;
        if (distance < slowRadius)
            speed = maxSpeed * (distance / slowRadius);

        Vector3 desiredVelocity = toTarget.normalized * speed;
        return desiredVelocity - currentVelocity; // fuerza de corrección, no solo dirección
    }

    /// <summary>
    /// Huye de una amenaza (por ejemplo, el NPC Cazador).
    /// </summary>
    public static Vector3 Evade(Vector3 myPos, Vector3 threatPos)
    {
        return (myPos - threatPos).normalized;
    }
}