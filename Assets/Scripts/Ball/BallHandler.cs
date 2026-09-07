using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using ElementType = UniudSummerCamp2025_2nd.ElementsListSO.ElementType;

namespace UniudSummerCamp2025_2nd
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallHandler : MonoBehaviour
    {
        [SerializeField] private BallDataSO ballData;
        [SerializeField] private ElementType activeElementType;
        [SerializeField] private ElementsListSO elementSos;

        [Header("Visual")]
        [SerializeField] private Renderer ballRenderer;
        [SerializeField] private Material ballMaterial;
        [SerializeField] private ParticleSystem smokeParticles;
        [SerializeField] private List<ParticleSystem> explosionParticles;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        public bool PickedUp { get; private set; }

        private Rigidbody _rigidbody;
        private Vector3 _oldPosition;
        private float _currentBallVelocityMagnitude;
        private bool _hitHandled;
        private bool _canExplode;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _canExplode = false;
            Init(transform.position, activeElementType);
        }

        private void FixedUpdate()
        {
            if (transform.position.y < -1)
            {
                DestroyBall();
                return;
            }

            // Calculating Velocity
            Vector3 currentPosition = transform.position;
            _currentBallVelocityMagnitude = Vector3.Distance(_oldPosition, currentPosition) / Time.fixedDeltaTime;
            _oldPosition = currentPosition;
        }

        private void OnCollisionEnter(Collision other)
        {
            BallHitSomething();
        }

        private void OnValidate()
        {
            UpdateVisual(activeElementType);
        }

        public void Init(Vector3 startingPosition, ElementType element, bool canExplode = false)
        {
            _canExplode = canExplode;
            // Reset ball properties
            _hitHandled = false;
            PickedUp = false;
            _rigidbody.isKinematic = false;

            activeElementType = element;
            UpdateVisual();
            StopExplosionVisual();

            // Placing in the correct Position
            transform.position = startingPosition;
            _oldPosition = startingPosition;
        }

        public void PickUp()
        {
            EnableVisuals();
            _rigidbody.isKinematic = false;
            PickedUp = true;
        }

        public void Throw()
        {
            if (!PickedUp) return;

            PlayThrowEffect(_currentBallVelocityMagnitude);
            PickedUp = false;
        }

        public void BallHitSomething()
        {
            if (_hitHandled) return;
            if (!_canExplode) return;

            _hitHandled = true;
            _rigidbody.isKinematic = true;

            float explosionDuration = ballData.explodeEffect.length;
            ExplodeVisual(explosionDuration);
            PlayExplodeEffect();

            Vector3 currentPosition = transform.position;
            float radius = ballData.explosionRadius;
            int damage = ballData.damage;

            List<GoblinController> goblins = GameManager.GetGoblinInRange(currentPosition, radius);

            foreach (GoblinController g in goblins)
            {
                g.HandleBallHit(damage, activeElementType);
            }

            Invoke(nameof(DestroyBall), explosionDuration);
        }

        public void DestroyBall()
        {
            Destroy(gameObject);
        }

        #region Visual Effects
        public void UpdateVisual(ElementType ballElement)
        {
            activeElementType = ballElement;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            Color ballColor = elementSos.GetColorFromElement(activeElementType);
            Material newMaterial = new Material(ballMaterial);
            float startAlpha = ballMaterial.color.a;
            newMaterial.color = new Color(ballColor.r, ballColor.g, ballColor.b, startAlpha);
            ballRenderer.sharedMaterial = newMaterial;

            ParticleSystem.MainModule mainSmoke = smokeParticles.main;
            mainSmoke.startColor = ballColor;

            UpdateExplosionParticle();
        }

        private void UpdateExplosionParticle()
        {
            Color ballColor = elementSos.GetColorFromElement(activeElementType);

            foreach (ParticleSystem particle in explosionParticles)
            {
                ParticleSystem.MainModule main = particle.main;
                main.startColor = ballColor;

                ParticleSystem.ShapeModule shape = particle.shape;
                shape.radius = ballData.explosionRadius;
            }
        }

        public void DisableVisuals()
        {
            ballRenderer.enabled = false;
            smokeParticles.Stop();

            foreach (ParticleSystem particle in explosionParticles)
            {
                particle.Stop();
            }
        }

        public void EnableVisuals()
        {
            ballRenderer.enabled = true;
            smokeParticles.Play();
        }

        public void ExplodeVisual(float explosionDuration)
        {
            smokeParticles.Stop();
            ballRenderer.enabled = false;
            UpdateExplosionParticle();

            foreach (ParticleSystem particle in explosionParticles)
            {
                particle.Play();
                ParticleSystem.ShapeModule shape = particle.shape;
                shape.radius = 0f;
            }

            StartCoroutine(nameof(AnimateExplosion));
        }

        private IEnumerator AnimateExplosion()
        {
            float explosionDuration = ballData.explodeEffect.length;
            float explosionRadius = ballData.explosionRadius;
            float timer = 0;
            float currentSize = 0;
            while (currentSize < explosionRadius)
            {
                timer += Time.deltaTime;
                currentSize = Mathf.Lerp(0, explosionRadius, timer / explosionDuration);

                foreach (ParticleSystem particle in explosionParticles)
                {
                    ParticleSystem.ShapeModule shape = particle.shape;
                    shape.radius = currentSize;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private void StopExplosionVisual()
        {
            foreach (ParticleSystem particle in explosionParticles)
            {
                particle.Stop();
            }
        }

        #endregion

        #region Audio Effects
        public void PlayThrowEffect(float throwSpeedMagnitude)
        {
            float velocityMaxAmplitudeEffect = 10;
            float scaledVelocity = throwSpeedMagnitude / velocityMaxAmplitudeEffect;
            // if velocity is over max scale down to 1
            float volume = Mathf.Min(1, scaledVelocity);
            if (FantasyAudioManager.Instance == null) return;
            FantasyAudioManager.Instance.PlayEffect(audioSource, ballData.throwEffect, volume);
        }

        public void PlayExplodeEffect()
        {
            if (FantasyAudioManager.Instance == null) return;
            FantasyAudioManager.Instance.PlayEffect(audioSource, ballData.explodeEffect);
        }
        #endregion

        #region XR Callbacks
        public void OnActivate(ActivateEventArgs activateEnterEventArgs)
        {
            PickUp();
        }

        public void OnDeactivate(DeactivateEventArgs deactivateEventArgs)
        {
            Throw();
        }

        public void OnSelectEnter(SelectEnterEventArgs selectEnterEventArgs)
        {
            PickUp();
        }

        public void OnSelectExit(SelectExitEventArgs selectExitEventArgs)
        {
            Throw();
        }
        #endregion
    }
}