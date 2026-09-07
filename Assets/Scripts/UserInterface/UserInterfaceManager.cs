using System;
using UnityEngine;
using GameState = UniudSummerCamp2025_2nd.GameManager.GameState;

namespace UniudSummerCamp2025_2nd
{
    public class UserInterfaceManager : MonoBehaviour
    {
        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private GameOverUI gameOverUI;

        public void ShowPanel(GameState gameState)
        {
            bool showingSomething = false;

            // Mostrare l'interfaccia corretta
            switch (gameState)
            {
                case GameState.Loading:
                    break;
                case GameState.WaitToPlay:
                    mainMenuUI.ShowPanel();
                    gameOverUI.HidePanel();
                    showingSomething = true;
                    break;
                case GameState.Playing:
                case GameState.WaitToSpawn:
                    mainMenuUI.HidePanel();
                    gameOverUI.HidePanel();
                    showingSomething = false;
                    break;
                case GameState.EndGame:
                    mainMenuUI.HidePanel();
                    gameOverUI.ShowPanel();
                    showingSomething = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gameState), gameState, null);
            }

            if (showingSomething)
            {
                Transform playerCameraTransform = Camera.main.transform;
                Vector3 playerCameraPosition = playerCameraTransform.position;
                var watchtowers = GameObject.FindGameObjectsWithTag("Tower");

                float distance = float.MaxValue;
                Transform closest = null;
                foreach (GameObject watchtower in watchtowers)
                {
                    Vector3 towerPosition = watchtower.transform.position;

                    if (Vector3.Distance(towerPosition, playerCameraPosition) < distance)
                    {
                        distance = Vector3.Distance(towerPosition, playerCameraPosition);
                        closest = watchtower.transform;
                    }
                }

                if (closest != null)
                {
                    Vector3 worldForwardFromSource = closest.TransformDirection(Vector3.forward);
                    transform.forward = -worldForwardFromSource;
                }
            }
        }

        public void UpdateMainMenuHighScore(int currentHighScore)
        {
            mainMenuUI.UpdateHighScore(currentHighScore);
        }

        public void UpdateGameOverUI(int currentScore, bool newHighScore, bool win)
        {
            gameOverUI.UpdateUI(currentScore, newHighScore, win);
        }
    }
}