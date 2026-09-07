using UnityEngine;
using UnityEngine.UI;

namespace UniudSummerCamp2025_2nd
{
    [RequireComponent(typeof(Button))]
    public class PlayClickEffect : MonoBehaviour
    {
        private void Awake()
        {
            Button btn = GetComponent<Button>();
            btn.onClick.AddListener(delegate
            {
                FantasyAudioManager.Instance.PlayUIClickEffect();
            });
        }
    }
}