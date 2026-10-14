using UnityEngine;

public class BoidVisuals : MonoBehaviour
{
    [SerializeField] private Renderer boidRenderer;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Color flockingColor = Color.cyan;
    [SerializeField] private Color evadingColor = Color.red;
    [SerializeField] private Color deadColor = Color.gray;

    private MaterialPropertyBlock propBlock;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    public void ShowFlocking() => SetColor(flockingColor);

    public void ShowEvading() => SetColor(evadingColor);

    public void ShowDead() => SetColor(deadColor);

    public void SetTrailEmitting(bool emitting)
    {
        if (trailRenderer != null) trailRenderer.emitting = emitting;
    }

    public void ResetToFlocking()
    {
        SetTrailEmitting(true);
        ShowFlocking();
    }

    private void SetColor(Color color)
    {
        if (boidRenderer != null)
        {
            boidRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", color); // URP Lit expone el color como _BaseColor

            boidRenderer.SetPropertyBlock(propBlock);
        }

        if (trailRenderer != null)
        {
            trailRenderer.startColor = color;
            trailRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
        }
    }
}
