using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    [CreateAssetMenu]
    public class GoblinDataSO : ScriptableObject
    {
        [Range(1, 3)] public int maxHealth = 2;
        [Range(1, 10)] public float gatherTime = 5;
        [Range(1, 3)] public float walkingSpeed = 1.6f;
        [Range(1, 5)] public float runningSpeed = 3.5f;
        [Range(1, 5)] public float timeToWaitToRemoveGoblin = 5f;

        [Header("Locomotion")]
        [Range(0, 1)] public float arrivedDistanceThreshold = 0.5f;

        [Header("Animator")]
        [Tooltip("Time to smooth animations")]
        [Range(0, 1)] public float dampTime = 0.2f;
        [Range(0, 1)] public float normalizedMaxAllowedSpeed = 1f;
        [Range(0, 1)] public float locomotionAnimatorDeadZone = 0.1f;
        [Range(0, 180)] public float degreeAlignThreshold = 45;

        [Header("Audio")]
        public AudioClip banishClip;
    }
}