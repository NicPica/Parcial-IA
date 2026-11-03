using UnityEngine;

public class HunterVisuals : MonoBehaviour
{
    [SerializeField] private Renderer hunterRenderer;
    [SerializeField] private Color searchColor = Color.green;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color gatherColor = Color.yellow;
    [SerializeField] private Color exitColor = Color.cyan;
    [SerializeField] private Color deadColor = Color.gray;
    [SerializeField] private Color fleeColor = Color.magenta;

    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    public void SetStateColor(HunterStateID stateID) => SetColor(GetStateColor(stateID));

    public Color GetStateColor(HunterStateID stateID)
    {
        switch (stateID)
        {
            case HunterStateID.Attack: return attackColor;
            case HunterStateID.Gather: return gatherColor;
            case HunterStateID.Exit: return exitColor;
            case HunterStateID.Dead: return deadColor;
            case HunterStateID.Flee: return fleeColor;
            default: return searchColor;
        }
    }

    private void SetColor(Color color)
    {
        if (hunterRenderer == null) return;

        hunterRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", color); // URP Lit expone el color como _BaseColor

        hunterRenderer.SetPropertyBlock(propBlock);
    }
}
