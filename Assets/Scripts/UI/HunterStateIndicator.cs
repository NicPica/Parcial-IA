using UnityEngine;
using TMPro;

/// <summary>
/// Texto flotante sobre el Hunter en el mundo 3D, mostrando su estado actual.
/// Siempre mira hacia la cámara (billboard).
/// </summary>
public class HunterStateIndicator : MonoBehaviour
{
    [SerializeField] private HunterNPC hunter;
    [SerializeField] private TextMeshProUGUI label;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (hunter == null || label == null) return;

        label.text = hunter.CurrentStateName;

        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
}