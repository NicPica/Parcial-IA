using UnityEngine;

public class HunterVisuals : MonoBehaviour
{
    [SerializeField] private Renderer hunterRenderer;
    [SerializeField] private Color patrolColor = Color.green;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color gatherColor = Color.yellow;

    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    public void SetStateColor(HunterStateID stateID)
    {
        switch (stateID)
        {
            case HunterStateID.Attack:
                SetColor(attackColor);
                break;
            case HunterStateID.Gather:
                SetColor(gatherColor);
                break;
            default:
                SetColor(patrolColor);
                break;
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
