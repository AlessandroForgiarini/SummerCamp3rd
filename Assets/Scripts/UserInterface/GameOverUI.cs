using TMPro;
using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text reachedScoreText;
        [SerializeField] private GameObject newHighScoreLabel;
        [SerializeField] private GameObject youWinLabel;
        [SerializeField] private GameObject youLoseLabel;

        public void UpdateUI(int score, bool newHighScore, bool win)
        {
            if (win)
            {
                youWinLabel.SetActive(true);
                youLoseLabel.SetActive(false);
            }
            else
            {
                youWinLabel.SetActive(false);
                youLoseLabel.SetActive(true);
            }

            reachedScoreText.text = score.ToString();

            if (newHighScore)
            {
                newHighScoreLabel.gameObject.SetActive(true);
            }
            else
            {
                newHighScoreLabel.gameObject.SetActive(false);
            }
        }

        public void PlayAgain()
        {
            GameManager.Instance.RestartGame();
        }

        public void ShowMainMenu()
        {
            GameManager.Instance.LoadMainMenu();
        }

        public void ShowPanel()
        {
            gameObject.SetActive(true);
        }

        public void HidePanel()
        {
            gameObject.SetActive(false);
        }
    }
}