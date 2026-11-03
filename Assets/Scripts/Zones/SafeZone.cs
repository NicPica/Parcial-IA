using UnityEngine;

// El collider sólido de la zona solo choca con lobos: el Cazador lo atraviesa por sus Exclude Layers.
public class SafeZone : MonoBehaviour
{
    [SerializeField] private WolfPack pack;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out HunterNPC hunter)) return;

        hunter.IsInSafeZone = true;
        pack.LoseTarget();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out HunterNPC hunter)) hunter.IsInSafeZone = false;
    }
}
