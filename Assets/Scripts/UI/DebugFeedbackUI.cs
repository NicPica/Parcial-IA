using UnityEngine;
using TMPro;

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

        Color color = hunter.Visuals != null ? hunter.Visuals.GetStateColor(hunter.StateMachine.CurrentID) : Color.white;
        string stateColor = "#" + ColorUtility.ToHtmlStringRGB(color);

        debugText.text =
            $"<b>Estado:</b> <color={stateColor}>{hunter.CurrentStateName}</color>\n" +
            $"<b>Objetivo (vivo):</b> {target}\n" +
            $"<b>Objetivo muerto:</b> {deadTarget}\n" +
            $"<b>POIs activos:</b> {poiCount}\n" +
            $"<b>Boids cazados:</b> {caughtCount}";
    }
}