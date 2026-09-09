using UnityEngine;

// Sfrutta il pattern di programmazione Singleton
[DisallowMultipleComponent]
public class PersistentXRRig : MonoBehaviour
{
    public static PersistentXRRig Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
    }
}