using UnityEngine;
using TMPro;

public class WolfStateIndicator : MonoBehaviour
{
    [SerializeField] private Wolf wolf;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GameObject knownPositionMarker;
    [SerializeField] private float messageDuration = 1.5f;

    private string message;
    private float messageTimer;

    public void ShowMessage(string text)
    {
        message = text;
        messageTimer = messageDuration;
    }

    private void LateUpdate()
    {
        messageTimer -= Time.deltaTime;
        label.text = messageTimer > 0f ? message : wolf.CurrentStateName;

        // En Patrol no usa ninguna posición del Cazador.
        bool usesKnownPosition = wolf.CurrentState != WolfStateID.Patrol;
        knownPositionMarker.SetActive(usesKnownPosition);
        if (usesKnownPosition) knownPositionMarker.transform.position = wolf.LastKnownPosition;
    }
}
