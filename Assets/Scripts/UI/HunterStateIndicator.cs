using UnityEngine;
using TMPro;

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