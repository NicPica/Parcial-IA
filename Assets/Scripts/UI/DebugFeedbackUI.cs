using UnityEngine;

/// <summary>
/// Muestra en pantalla información de debug sobre el estado del Hunter,
/// para verificar visualmente el funcionamiento de la FSM durante la ejecución.
/// </summary>
public class DebugFeedbackUI : MonoBehaviour
{
    [SerializeField] private HunterNPC hunter;

    private GUIStyle style;

    private void OnGUI()
    {
        if (hunter == null) return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.box);
            style.fontSize = 18;
            style.alignment = TextAnchor.UpperLeft;
            style.normal.textColor = Color.white;
        }

        string target = hunter.CurrentTarget != null ? hunter.CurrentTarget.name : "ninguno";
        string deadTarget = hunter.DeadBoidTarget != null ? hunter.DeadBoidTarget.name : "ninguno";
        int poiCount = PointOfInterestManager.Instance != null ? PointOfInterestManager.Instance.ActiveCount : 0;

        string text =
            $"Estado: {hunter.CurrentStateName}\n" +
            $"Objetivo (vivo): {target}\n" +
            $"Objetivo (muerto/gather): {deadTarget}\n" +
            $"POIs activos: {poiCount}\n" +
            $"Posición Hunter: {hunter.transform.position:F1}";

        GUI.Box(new Rect(10, 10, 350, 120), text, style);
    }
}