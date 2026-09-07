using UnityEngine;

[DefaultExecutionOrder(-100)] // Ensures this runs before other XR scripts initialize
public class PersistentXRRig : MonoBehaviour
{
    public static PersistentXRRig Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A persistent rig already exists (came from Main Menu).
            // Destroy this local development rig immediately.
            DestroyImmediate(gameObject);
            return;
        }

        // No rig exists yet (started from Main Menu, or testing directly in this scene)
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}