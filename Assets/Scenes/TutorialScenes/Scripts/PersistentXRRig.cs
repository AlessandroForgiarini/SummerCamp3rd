using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PersistentXRRig : MonoBehaviour
{
    public static PersistentXRRig Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("Start XR ");
        if (Instance != null && Instance != this)
        {
            Debug.Log("Destroy XR");
            // A persistent rig already exists (came from Main Menu).
            // Destroy this local development rig immediately.
            DestroyImmediate(gameObject);
            return;
        }

        // No rig exists yet (started from Main Menu, or testing directly in this scene)
        Instance = this;
    }
}