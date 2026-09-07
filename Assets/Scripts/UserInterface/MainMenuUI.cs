using TMPro;
using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text highScoreText;

        public void UpdateHighScore(int currentHighScore)
        {
            highScoreText.text = currentHighScore.ToString();
        }

        public void StartGame()
        {
            GameManager.Instance.StartGame();
        }

        public void ExitApplication()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#else
        Application.Quit();  
#endif
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