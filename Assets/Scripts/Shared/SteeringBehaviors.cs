using UnityEngine;

public static class SteeringBehaviors
{
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

    public static Vector3 Alignment(Vector3[] neighborVelocities, int neighborCount)
    {
        if (neighborCount == 0) return Vector3.zero;

        Vector3 avgVelocity = Vector3.zero;
        for (int i = 0; i < neighborCount; i++)
            avgVelocity += neighborVelocities[i];

        avgVelocity /= neighborCount;
        return avgVelocity.normalized;
    }

    public static Vector3 Cohesion(Vector3 myPos, Vector3[] neighborPositions, int neighborCount)
    {
        if (neighborCount == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        for (int i = 0; i < neighborCount; i++)
            center += neighborPositions[i];

        center /= neighborCount;
        return (center - myPos).normalized;
    }

    public static Vector3 Arrive(Vector3 myPos, Vector3 targetPos, Vector3 currentVelocity, float maxSpeed, float slowRadius, float stopRadius, float brakeFactor)
    {
        Vector3 toTarget = targetPos - myPos;
        float distance = toTarget.magnitude;

        if (distance < stopRadius)
        {
            // Frenado activo: fuerza opuesta a la velocidad actual para detenerlo de verdad
            return -currentVelocity * brakeFactor;
        }

        float speed = maxSpeed;
        if (distance < slowRadius)
            speed = maxSpeed * (distance / slowRadius);

        Vector3 desiredVelocity = toTarget.normalized * speed;
        return desiredVelocity - currentVelocity; // fuerza de corrección, no solo dirección
    }

    public static Vector3 Evade(Vector3 myPos, Vector3 threatPos)
    {
        return (myPos - threatPos).normalized;
    }

    // Empuja hacia adentro más fuerte cuanto más se mete el agente en el margen del borde.
    public static Vector3 Containment(Vector3 myPos, float halfWidth, float halfHeight, float margin)
    {
        Vector3 force = Vector3.zero;

        if (myPos.x > halfWidth - margin) force.x -= (myPos.x - (halfWidth - margin)) / margin;
        if (myPos.x < -halfWidth + margin) force.x += (-halfWidth + margin - myPos.x) / margin;
        if (myPos.z > halfHeight - margin) force.z -= (myPos.z - (halfHeight - margin)) / margin;
        if (myPos.z < -halfHeight + margin) force.z += (-halfHeight + margin - myPos.z) / margin;

        return force;
    }

    // El ángulo se acumula entre frames para que el rumbo cambie de a poco y no tiemble.
    public static Vector3 Wander(ref float wanderAngle, float jitterDegrees, float deltaTime)
    {
        wanderAngle += Random.Range(-jitterDegrees, jitterDegrees) * deltaTime;
        return Quaternion.Euler(0f, wanderAngle, 0f) * Vector3.forward;
    }
}
