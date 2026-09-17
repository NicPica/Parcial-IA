using UnityEngine;
using TMPro;

/// <summary>
/// Muestra en pantalla información de debug sobre el estado del Hunter,
/// para verificar visualmente el funcionamiento de la FSM durante la ejecución.
/// </summary>
public class DebugFeedbackUI : MonoBehaviour
{
    [SerializeField] private HunterNPC hunter;
    [SerializeField] private TextMeshProUGUI debugText;

    private void Update()
    {
        if (hunter == null || debugText == null) return;

        string target = hunter.CurrentTarget != null ? hunter.CurrentTarget.name : "ninguno";
        string deadTarget = hunter.DeadBoidTarget != null ? hunter.DeadBoidTarget.name : "ninguno";
        int poiCount = PointOfInterestManager.Instance != null ? PointOfInterestManager.Instance.ActiveCount : 0;
        int caughtCount = GameStatsManager.Instance != null ? GameStatsManager.Instance.BoidsCaught : 0;

        string stateColor = GetColorForState(hunter.CurrentStateName);

        debugText.text =
            $"<b>Estado:</b> <color={stateColor}>{hunter.CurrentStateName}</color>\n" +
            $"<b>Objetivo (vivo):</b> {target}\n" +
            $"<b>Objetivo muerto:</b> {deadTarget}\n" +
            $"<b>POIs activos:</b> {poiCount}\n" +
            $"<b>Boids cazados:</b> {caughtCount}";
    }

    private string GetColorForState(string stateName)
    {
        switch (stateName)
        {
            case "Patrol": return "#4CD964"; // verde
            case "Attack": return "#FF3B30"; // rojo
            case "Gather": return "#FFCC00"; // amarillo
            default: return "#FFFFFF";
        }
    }
}