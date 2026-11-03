using UnityEngine;
using TMPro;

public class HunterStateIndicator : MonoBehaviour
{
    [SerializeField] private HunterNPC hunter;
    [SerializeField] private TextMeshProUGUI label;
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
        if (hunter == null || label == null) return;

        messageTimer -= Time.deltaTime;
        if (messageTimer > 0f)
        {
            label.text = message;
            return;
        }

        string state = hunter.ChosenSafeZone != null ? $"Huye a {hunter.ChosenSafeZone.name}" : hunter.CurrentStateName;
        label.text = $"{state} {hunter.Gathers}/{hunter.RequiredGathers}";
    }
}
