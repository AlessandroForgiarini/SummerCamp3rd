using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    public class TitleTextAnimation : MonoBehaviour
    {
        private Transform myTransform;
        private bool isIncreasing;

        [SerializeField, Range(0, 10)]
        private float changeSizeSpeed = 5;
        [SerializeField, Range(0, 2)]
        private float minScale = 1f;
        [SerializeField, Range(0, 2)]
        private float maxScale = 1.2f;
        private float _currentScale;

        void Start()
        {
            myTransform = transform;
            isIncreasing = true;
        }

        void Update()
        {
            float changeAmount = changeSizeSpeed * Time.deltaTime;
            if (isIncreasing)
            {
                _currentScale += changeAmount;
            }
            else
            {
                _currentScale -= changeAmount;

            }

            if (_currentScale > maxScale)
            {
                _currentScale = maxScale;
                isIncreasing = false;
            }
            else if (_currentScale < minScale)
            {
                _currentScale = minScale;
                isIncreasing = true;
            }
            Vector3 newSize = new Vector3(_currentScale, _currentScale, _currentScale);
            myTransform.localScale = newSize;
        }
    }
}