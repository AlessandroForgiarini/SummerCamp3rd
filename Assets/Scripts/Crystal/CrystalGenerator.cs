using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    public class CrystalGenerator : MonoBehaviour
    {
        // Maximum attempts to find a valid spot for a single crystal
        private const int MaxPlacementTries = 100;

        [Header("Crystal Settings")]

        [Tooltip("The crystal prefab to spawn into the scene.")]
        [SerializeField] private GameObject crystalPrefab;

        [Tooltip("The minimum allowed distance between each spawned crystal.")]
        [SerializeField, Range(0, 0.5f)] private float minSpacing = 0.2f;

        [Header("Spawn Area")]

        [Tooltip("The Renderer of the object that defines the spawn area (e.g., a plane).")]
        [SerializeField] private Renderer spawnAreaRenderer;

        private void Start()
        {
            if (spawnAreaRenderer == null)
            {
                spawnAreaRenderer = GetComponent<Renderer>();
            }
        }
        /// <summary>
        /// Spawns crystals randomly within the spawn area.
        /// Returns the actual number of crystals successfully spawned.
        /// </summary>
        public int GenerateCrystals(int totalCrystals)
        {
            if (crystalPrefab == null)
            {
                Debug.LogWarning($"{nameof(crystalPrefab)} not set!");
                return 0;
            }

            if (spawnAreaRenderer == null)
            {
                Debug.LogWarning($"{nameof(spawnAreaRenderer)} not set!");
                return 0;
            }

            // List to remember where crystals are placed
            List<Vector3> placedPositions = new List<Vector3>();

            // Get the center and extents (half-size) of the area
            Vector3 center = spawnAreaRenderer.bounds.center;
            Vector3 extents = spawnAreaRenderer.bounds.extents;
            // We’ll place all crystals at the same Y as the spawn area’s center
            float y = center.y;

            for (int i = 0; i < totalCrystals; i++)
            {
                int attempts = 0;
                Vector3 randomPos;

                // Keep trying until we find a free spot, or we hit the max tries
                do
                {
                    randomPos = center + new Vector3(
                        Random.Range(-extents.x, extents.x),
                        y, // at the same height as the spawn area center
                        Random.Range(-extents.z, extents.z)
                    );

                    attempts++;

                } while (
                    // If any existing crystal is too close, try again
                    placedPositions.Any(pos => Vector3.Distance(pos, randomPos) < minSpacing)
                    && attempts < MaxPlacementTries
                );

                if (attempts < MaxPlacementTries)
                {
                    placedPositions.Add(randomPos);
                }
            }


            // Spawn all crystals with random Y rotation
            foreach (Vector3 pos in placedPositions)
            {
                float randomYRotation = Random.Range(0f, 360f);
                Quaternion rotation = Quaternion.Euler(0f, randomYRotation, 0f);
                Instantiate(crystalPrefab, pos, rotation, transform);
            }

            // Return how many were actually spawned
            return placedPositions.Count;
        }
    }
}