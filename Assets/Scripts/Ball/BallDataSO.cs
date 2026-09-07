using UnityEngine;

namespace UniudSummerCamp2025_2nd
{
    [CreateAssetMenu()]
    public class BallDataSO : ScriptableObject
    {
        public float explosionRadius;
        public int damage;
        public AudioClip throwEffect;
        public AudioClip explodeEffect;
    }
}