using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    [CreateAssetMenu]
    public class LevelSO : ScriptableObject
    {
        [Serializable]
        public struct GoblinWaveData
        {
            [Range(0, 15)]
            public int totalGoblins;
            [Range(0, 5)]
            public float timeToSpawnGoblin;
            public ElementsListSO.ElementType[] availableElements;

            public ElementsListSO.ElementType GetRandomAvailableElement()
            {
                int index = UnityEngine.Random.Range(0, availableElements.Length);
                return availableElements[index];
            }
        }

        public List<GoblinWaveData> goblinWaves;
        [Range(0, 20)] public float timeBetweenWaves;
        [Range(1, 30)] public int totalCrystals;
    }
}