using UnityEngine;
using System.Collections.Generic;

namespace UniudSummerCamp2025_2nd
{
    public class GoblinSpawner : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private GoblinController goblinPrefab;

        private LevelSO.GoblinWaveData _currentWaveData;
        private float _timer;
        private bool _canSpawn;
        private int _totalSpawnedGoblins;

        public bool SpawnedAllGoblins => _totalSpawnedGoblins == _currentWaveData.totalGoblins;

        private void Start()
        {
            DisableSpawner();
        }

        public void EnableSpawner(LevelSO.GoblinWaveData currentGoblinWave)
        {
            _currentWaveData = currentGoblinWave;
            _timer = _currentWaveData.timeToSpawnGoblin; // to spawn a goblin right away
            _totalSpawnedGoblins = 0;
            _canSpawn = true;
        }

        public void DisableSpawner()
        {
            _canSpawn = false;
        }

        private void Update()
        {
            if (!_canSpawn) return;

            HandleGoblinSpawn();
        }

        private void HandleGoblinSpawn()
        {
            _timer += Time.deltaTime;
            if (SpawnedAllGoblins) return;
            if (_timer < _currentWaveData.timeToSpawnGoblin) return;

            SpawnGoblin();
            _totalSpawnedGoblins += 1;
            _timer = 0;
        }

        private void SpawnGoblin()
        {
            ElementsListSO.ElementType spawnElement = _currentWaveData.GetRandomAvailableElement();

            int index = Random.Range(0, spawnPoints.Count);
            Transform spawnTransformPoint = spawnPoints[index];

            Vector3 position = spawnTransformPoint.position;
            Quaternion rotation = Quaternion.identity;
            GoblinController spawnedGoblin = Instantiate(goblinPrefab, position, rotation, transform);

            spawnedGoblin.Init(spawnTransformPoint, spawnElement);
        }
    }
}