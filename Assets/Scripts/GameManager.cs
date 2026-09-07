using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    public class GameManager : MonoBehaviour
    {
        private const string HighscoreLabelPlayerPrefs = "playerHighscore";

        public enum GameState
        {
            Loading,
            WaitToPlay,
            Playing,
            WaitToSpawn,
            EndGame
        }

        public static GameManager Instance { get; private set; }

        [Header("Scriptable Objects")]
        [SerializeField] private LevelSO currentLevelSo;
        [SerializeField] private ElementsListSO elementsListSo;

        private List<LevelSO.GoblinWaveData> _goblinWaves;
        private float _timeBetweenWaves;

        private GoblinSpawner _goblinSpawner;
        private UserInterfaceManager _interfaceManager;
        private CrystalGenerator _crystalGenerator;

        private GameState _currentState = GameState.Loading;
        private int _currentGoblinWaveIndex;
        private int _totalGoblinToBanish;
        private int _goblinBanishedCount;
        private float _waitToSpawnTimer;
        private int _stolenCrystals;
        private int _totalSpawnedCrystals;
        private bool _firstGame;

        private void Awake()
        {
            Instance = this;
            _goblinWaves = currentLevelSo.goblinWaves;
            _firstGame = true;
        }

        private void Start()
        {
            _goblinSpawner = GameObject.Find("GoblinSpawner").GetComponent<GoblinSpawner>();
            _interfaceManager = GameObject.Find("GameUserInterface").GetComponent<UserInterfaceManager>();
            _crystalGenerator = GameObject.Find("CrystalGenerator").GetComponent<CrystalGenerator>();
            ResetCrystals();

            LoadMainMenu();
        }

        private void Update()
        {
            switch (_currentState)
            {
                case GameState.Loading:
                    break;
                case GameState.WaitToPlay:
                    break;
                case GameState.Playing:
                    if (_totalSpawnedCrystals == _stolenCrystals)
                    {
                        GameOverLost();
                    }
                    else if (_goblinBanishedCount == _totalGoblinToBanish)
                    {
                        GameOverWin();
                    }
                    else if (_goblinSpawner.SpawnedAllGoblins && _currentGoblinWaveIndex < _goblinWaves.Count - 1)
                    {
                        _waitToSpawnTimer = 0;
                        UpdateState(GameState.WaitToSpawn);
                    }
                    break;
                case GameState.WaitToSpawn:
                    if (_totalSpawnedCrystals == _stolenCrystals)
                    {
                        GameOverLost();
                    }
                    else if (_goblinBanishedCount == _totalGoblinToBanish)
                    {
                        GameOverWin();
                    }
                    else
                    {
                        _waitToSpawnTimer += Time.deltaTime;
                        if (_waitToSpawnTimer > _timeBetweenWaves)
                        {
                            LoadGoblinWave(_currentGoblinWaveIndex + 1);
                            UpdateState(GameState.Playing);
                        }
                    }
                    break;
                case GameState.EndGame:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateState(GameState newState)
        {
            if (_currentState == newState)
            {
                Debug.LogWarning($"Updating game state with same state: [{newState}]");
                return;
            }

            _currentState = newState;
            _interfaceManager.ShowPanel(newState);
        }

        public void LoadMainMenu()
        {
            // Loading saved HighScore
            int currentHighScore = PlayerPrefs.GetInt(HighscoreLabelPlayerPrefs, 0);
            _interfaceManager.UpdateMainMenuHighScore(currentHighScore);

            // Enables spawners for all elements. If we want to support only elements in level need to change logic
            PrepareSpawners();

            UpdateState(GameState.WaitToPlay);
        }

        private void PrepareSpawners()
        {
            GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
            List<ElementsListSO.ElementData> elements = elementsListSo.elements;
            List<ElementsListSO.ElementData> validElements =
                elements.Where(el => el.type != ElementsListSO.ElementType.INVALID).ToList();

            foreach (GameObject tower in towers)
            {
                BallSpawner[] spawners = tower.GetComponentsInChildren<BallSpawner>();
                int counter = 0;
                foreach (BallSpawner spawner in spawners)
                {
                    int index = counter % validElements.Count;
                    spawner.SetSpawnerElement(validElements[index].type);
                    counter++;
                }
            }
        }

        public void StartGame()
        {
            DestroyAllBalls();

            if (!_firstGame)
            {
                ResetCrystals();
            }

            PrepareGoblins();

            _timeBetweenWaves = currentLevelSo.timeBetweenWaves;
            LoadGoblinWave(0);

            UpdateState(GameState.Playing);
        }

        private void PrepareGoblins()
        {
            foreach (LevelSO.GoblinWaveData waveData in _goblinWaves)
            {
                _totalGoblinToBanish += waveData.totalGoblins;
            }

            _goblinBanishedCount = 0;
            _stolenCrystals = 0;
        }

        private void ResetCrystals()
        {
            foreach (var o in GameObject.FindGameObjectsWithTag("Crystal"))
            {
                DestroyImmediate(o);
            }

            int _totalCrystals = currentLevelSo.totalCrystals;
            _totalSpawnedCrystals = _crystalGenerator.GenerateCrystals(_totalCrystals);
        }

        public void GameOverWin()
        {
            GameOver(true);
        }

        public void GameOverLost()
        {
            GameOver(false);
        }

        public void GameOver(bool win)
        {
            Debug.Log($"Total Crystals: {_totalSpawnedCrystals}, Total Goblins to Banish: {_totalGoblinToBanish}");
            Debug.Log($"Stolen Crystals: {_stolenCrystals}, Goblins Banished: {_goblinBanishedCount}");
            _firstGame = false;
            DestroyAllBalls();
            DestroyAllGoblins();
            _goblinSpawner.DisableSpawner();

            // handling highscore
            int currentHighScore = PlayerPrefs.GetInt(HighscoreLabelPlayerPrefs, 0);

            var crystalHandlers = GameObject.FindGameObjectsWithTag("Crystal");
            int currentScore = 0;
            foreach (var crystal in crystalHandlers)
            {
                currentScore += crystal.GetComponent<CrystalHandler>().GetScore();
            }

            bool newHighScore = currentScore > currentHighScore;
            if (newHighScore)
            {
                // saving new highscore
                PlayerPrefs.SetInt(HighscoreLabelPlayerPrefs, currentScore);
                PlayerPrefs.Save();
            }

            _interfaceManager.UpdateGameOverUI(currentScore, newHighScore, win);

            UpdateState(GameState.EndGame);
        }

        public void RestartGame()
        {
            StartGame();
        }

        private void LoadGoblinWave(int index)
        {
            _currentGoblinWaveIndex = index;
            LevelSO.GoblinWaveData data = _goblinWaves[_currentGoblinWaveIndex];
            _goblinSpawner.EnableSpawner(data);
        }

        public void DestroyAllBalls()
        {
            DestroyObjectsInSceneByType<BallHandler>();
        }

        public void DestroyAllGoblins()
        {
            DestroyObjectsInSceneByType<GoblinController>();
        }

        public void OnGoblinBanished(bool byPlayer)
        {
            _goblinBanishedCount += 1;
        }

        public void OnRemovedCrystal()
        {
            _stolenCrystals += 1;
        }

        public static void DestroyObjectsInSceneByType<T>() where T : MonoBehaviour
        {
            T[] gos = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (T go in gos)
            {
                Destroy(go.gameObject);
            }
        }

        public static List<GoblinController> GetGoblinInRange(Vector3 position, float radius)
        {
            Collider[] inRange = Physics.OverlapSphere(position, radius);
            List<GoblinController> goblinControllers = new();
            foreach (var coll in inRange)
            {
                GoblinController controller = coll.gameObject.GetComponentInParent<GoblinController>();

                if (controller is null) continue;
                if (!controller.gameObject.CompareTag("Goblin")) continue;

                goblinControllers.Add(controller);
            }

            return goblinControllers;
        }

        public static Vector3 GetApproxApproachPosition(CrystalHandler crystalHandler, Vector3 myPosition)
        {
            Bounds crystalBounds = crystalHandler.GetCrystalBounds();
            Vector3 approxApproachingDir = myPosition - crystalBounds.center;
            Vector3 crystalExtents = crystalBounds.extents;
            float approxCrystalRadius = Mathf.Max(crystalExtents.x, crystalExtents.z);
            Vector3 agentActualDestination = approxApproachingDir.normalized * approxCrystalRadius + crystalBounds.center;
            return agentActualDestination;
        }
    }
}