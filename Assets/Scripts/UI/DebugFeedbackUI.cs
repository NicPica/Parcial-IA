using UnityEngine;
using TMPro;

public class DebugFeedbackUI : MonoBehaviour
{
    [SerializeField] private HunterNPC hunter;
    [SerializeField] private TextMeshProUGUI debugText;
    [Tooltip("Armar el texto genera basura: no hace falta hacerlo por frame.")]
    [SerializeField] private float refreshInterval = 0.2f;

    private float refreshTimer;

    private void Update()
    {
        if (hunter == null || debugText == null) return;

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f) return;
        refreshTimer = refreshInterval;

        string safeZone = hunter.ChosenSafeZone != null ? hunter.ChosenSafeZone.name : "ninguna";
        string target = hunter.CurrentTarget != null ? hunter.CurrentTarget.name : "ninguno";
        string deadTarget = hunter.DeadBoidTarget != null ? hunter.DeadBoidTarget.name : "ninguno";
        int poiCount = PointOfInterestManager.Instance != null ? PointOfInterestManager.Instance.ActiveCount : 0;

        Color color = hunter.Visuals != null ? hunter.Visuals.GetStateColor(hunter.StateMachine.CurrentID) : Color.white;
        string stateColor = "#" + ColorUtility.ToHtmlStringRGB(color);

        debugText.text =
            $"<b>Estado:</b> <color={stateColor}>{hunter.CurrentStateName}</color>\n" +
            $"<b>Objetivo (vivo):</b> {target}\n" +
            $"<b>Objetivo muerto:</b> {deadTarget}\n" +
            $"<b>POIs activos:</b> {poiCount}\n" +
            $"<b>Recolecciones:</b> {hunter.Gathers}/{hunter.RequiredGathers}\n" +
            $"<b>Lobo a la vista:</b> {hunter.SeenWolfName}\n" +
            $"<b>Zona segura elegida:</b> {safeZone}\n" +
            "<b>Disco de color:</b> ultima posicion de ese lobo";
    }
}