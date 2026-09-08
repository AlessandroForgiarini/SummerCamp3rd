using UnityEngine;

public class ToggleMaterial : MonoBehaviour
{
    public Material materialA;
    public Material materialB;
    public Renderer targetRenderer;

    bool isUsingMaterialA;

    void Start()
    {
        isUsingMaterialA = false;
        Toggle();
    }

    public void Toggle()
    {
        isUsingMaterialA = !isUsingMaterialA;

        // Decido che colore usare
        if (isUsingMaterialA)
        {
            targetRenderer.material = materialA;
        }
        else
        {
            targetRenderer.material = materialB;
        }
    }
}