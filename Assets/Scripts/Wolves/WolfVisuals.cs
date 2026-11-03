using UnityEngine;

public class WolfVisuals : MonoBehaviour
{
    [SerializeField] private Renderer[] wolfRenderers;
    [SerializeField] private Color patrolColor = Color.gray;
    [SerializeField] private Color investigateColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color chaseColor = Color.red;
    [SerializeField] private Color searchColor = Color.yellow;

    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    public void SetStateColor(WolfStateID stateID)
    {
        propBlock.SetColor("_BaseColor", GetStateColor(stateID)); // URP Lit expone el color como _BaseColor

        foreach (Renderer wolfRenderer in wolfRenderers) wolfRenderer.SetPropertyBlock(propBlock);
    }

    private Color GetStateColor(WolfStateID stateID)
    {
        switch (stateID)
        {
            case WolfStateID.Investigate: return investigateColor;
            case WolfStateID.Chase: return chaseColor;
            case WolfStateID.Search: return searchColor;
            default: return patrolColor;
        }
    }
}
