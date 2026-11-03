using UnityEngine;

// Mantiene legible la etiqueta del padre en cualquier modo de cámara.
public class AgentLabel : MonoBehaviour
{
    [SerializeField] private float offset = 2.5f;
    [Tooltip("Acerca la etiqueta a la cámara para que las paredes no la tapen.")]
    [SerializeField] private float lift = 3f;
    [SerializeField] private float scalePerDistance = 0.0012f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        Transform view = mainCamera.transform;
        Vector3 position = transform.parent.position + view.up * offset - view.forward * lift;

        // Con la cámara cenital, mirar hacia ella daría una rotación degenerada.
        transform.SetPositionAndRotation(position, view.rotation);
        transform.localScale = Vector3.one * (scalePerDistance * Vector3.Distance(view.position, position));
    }
}
