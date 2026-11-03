using System;
using UnityEngine;

// Solo entrega el aullido: cada lobo decide su transición (1.11).
public class WolfPack : MonoBehaviour
{
    public event Action<Wolf, Vector3> Howled;
    public event Action TargetLost;

    public void Howl(Wolf source, Vector3 position) => Howled?.Invoke(source, position);

    public void LoseTarget() => TargetLost?.Invoke();
}
