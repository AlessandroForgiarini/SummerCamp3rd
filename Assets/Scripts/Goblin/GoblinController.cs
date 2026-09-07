using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using ElementType = UniudSummerCamp2025_2nd.ElementsListSO.ElementType;

namespace UniudSummerCamp2025_2nd
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class GoblinController : MonoBehaviour
    {
        #region Labels For Animations
        private static readonly int MovingLabel = Animator.StringToHash("Moving");
        private static readonly int BlendTreeLocomotionXLabel = Animator.StringToHash("LocomotionX");
        private static readonly int BlendTreeLocomotionYLabel = Animator.StringToHash("LocomotionY");
        private static readonly int StartGatherLabel = Animator.StringToHash("StartGather");
        private static readonly int EndGatherLabel = Animator.StringToHash("EndGather");
        private static readonly int BanishLabel = Animator.StringToHash("Hit");
        #endregion

        public enum State
        {
            IDLE,
            GO_TO_TARGET,
            GATHER,
            PREPARE_TO_RUN,
            RUN_AWAY,
            BANISHED,
        }

        [SerializeField] private ElementType activeElementType;
        [SerializeField] private ElementsListSO elementSos;
        [SerializeField] private GoblinDataSO goblinData;
        [SerializeField] private Transform backPackTransform;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        [Header("Visual")]
        [SerializeField] private Renderer skinRenderer;
        [SerializeField] private Material goblinSkinMaterial;

        [Header("UI Elements")]
        [SerializeField] private Image[] healthHearts;
        [SerializeField] private Image progressBar;

        private State _currentState = State.IDLE;
        private NavMeshAgent _agent;
        private Animator _anim;
        private float _gatherTimer;
        private bool _goblinBanished;
        private Transform _spawnPointTransform;
        private int _currentHealth;
        // Crystal holding
        private CrystalHandler _linkedCrystalHandler;
        // Crystal we are going to gather
        private CrystalHandler _targetCrystalHandler;

        private void Awake()
        {
            _agent = GetComponentInChildren<NavMeshAgent>();
            _anim = GetComponentInChildren<Animator>();
        }

        public void Init(Transform spawnPointTransform, ElementType element)
        {
            // Setting goblin properties
            _currentHealth = goblinData.maxHealth;
            _spawnPointTransform = spawnPointTransform;
            activeElementType = element;

            // Resetting goblin
            _goblinBanished = false;
            _linkedCrystalHandler = null;
            _targetCrystalHandler = null;
            _currentState = State.IDLE;
            UpdateVisual();
            UpdateHeartsUI(_currentHealth);

            CrystalHandler destinationCrystalHandler = FindNearestFreeCrystal();

            // Decide first Target
            if (destinationCrystalHandler != null)
            {
                Vector3 agentActualDestination = GameManager.GetApproxApproachPosition(destinationCrystalHandler, transform.position);
                _targetCrystalHandler = destinationCrystalHandler;
                PrepareForLocomotion(agentActualDestination, 1f);
                ChangeState(State.GO_TO_TARGET);
            }
            else
            {
                // Technically, we should only banish it if there are no more crystals on the scene, 
                // otherwise it might be that a goblin is carrying it, and thus it can be interrupted.
                // But for simplicity we banish him
                Banish();
            }
        }

        /// <summary>
        /// Sets Moving trigger, destination and speed for NavMeshAgent.
        /// </summary>
        private void PrepareForLocomotion(Vector3 destination, float normalizedSpeed)
        {
            _agent.SetDestination(destination);
            _anim.SetBool(MovingLabel, true);
            _anim.SetFloat(BlendTreeLocomotionYLabel, normalizedSpeed, goblinData.dampTime, Time.deltaTime);
            _agent.speed = normalizedSpeed > 0.5 ? goblinData.runningSpeed : goblinData.walkingSpeed;
        }

        /// <summary>
        /// Finds the nearest free crystal by checking all crystals in the scene.
        /// </summary>
        /// <returns>Reference to the crystal if found, null otherwise</returns>
        private CrystalHandler FindNearestFreeCrystal()
        {
            GameObject[] crystals = GameObject.FindGameObjectsWithTag("Crystal");
            float minDistance = float.MaxValue;
            CrystalHandler destSelected = null;
            Vector3 myPosition = transform.position;

            foreach (GameObject crystal in crystals)
            {
                CrystalHandler crystalHandler = crystal.GetComponent<CrystalHandler>();
                if (!crystalHandler.IsFree) continue;

                Vector3 crystalPosition = crystal.transform.position;
                float distance = Vector3.Distance(myPosition, crystalPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    destSelected = crystalHandler;
                }
            }

            /*
            if (destSelected == null)
            {
                Debug.Log("No crystal selected");
            }
            else
            {
                Debug.Log($"Selected position {destSelected.transform.position} of {destSelected.transform.name}");
            }
            */
            return destSelected;
        }

        private void ChangeState(State newState)
        {
            if (newState == _currentState)
            {
                //            Debug.LogWarning($"Same state provided [{newState}], not doing anything");
                return;
            }
            _currentState = newState;

            // reset gather progress bar
            UpdateGatherProgressBar(0f);
            switch (_currentState)
            {
                case State.IDLE:
                    break;
                case State.GO_TO_TARGET:
                    break;
                case State.GATHER:
                    _gatherTimer = 0;
                    break;
                case State.RUN_AWAY:
                    break;
                case State.BANISHED:
                    break;
                case State.PREPARE_TO_RUN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {
            if (transform.position.y < -1)
            {
                Banish();
            }
        }

        private void Update()
        {
            if (_goblinBanished) return;

            switch (_currentState)
            {
                case State.IDLE:
                    break;
                case State.GO_TO_TARGET:
                    if (!_targetCrystalHandler.IsFree)
                    {
                        FindNextTarget(State.GO_TO_TARGET, State.RUN_AWAY);
                        break;
                    }

                    if (_agent.pathPending) { return; }

                    //reaching target destination
                    if (_agent.remainingDistance > goblinData.arrivedDistanceThreshold)
                    {
                        HandleLocomotionAnimations(goblinData.normalizedMaxAllowedSpeed);
                    }
                    else //arrived at destination
                    {
                        PrepareForGathering();
                        ChangeState(State.GATHER);
                    }
                    break;
                case State.GATHER:
                    if (_targetCrystalHandler == null || _targetCrystalHandler.IsFree == false)
                    {
                        if (_anim.GetCurrentAnimatorStateInfo(0).IsName("GatheringProcess"))
                        {
                            _anim.SetTrigger(EndGatherLabel);
                        }

                        FindNextTarget(State.GO_TO_TARGET, State.RUN_AWAY);
                        break;
                    }

                    if (_gatherTimer > goblinData.gatherTime)
                    {
                        _linkedCrystalHandler = _targetCrystalHandler;
                        _linkedCrystalHandler.SetBackPackParent(backPackTransform);
                        _targetCrystalHandler = null;

                        _anim.SetTrigger(EndGatherLabel);
                        ChangeState(State.PREPARE_TO_RUN);
                    }
                    else
                    {
                        _gatherTimer += Time.deltaTime;
                        float progress = Mathf.Clamp01(_gatherTimer / goblinData.gatherTime);
                        UpdateGatherProgressBar(progress);
                    }
                    break;
                case State.PREPARE_TO_RUN:
                    if (_anim.GetCurrentAnimatorStateInfo(0).IsName("GatheringProcess"))
                    {
                        _anim.SetTrigger(EndGatherLabel);
                    }
                    if (_anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        PrepareForLocomotion(_spawnPointTransform.transform.position, goblinData.normalizedMaxAllowedSpeed);
                        ChangeState(State.RUN_AWAY);
                    }
                    break;
                case State.RUN_AWAY:
                    if (_agent.pathPending) { return; }

                    if (_agent.remainingDistance > goblinData.arrivedDistanceThreshold)
                    {
                        HandleLocomotionAnimations(goblinData.normalizedMaxAllowedSpeed);
                    }
                    else
                    {
                        StopAgent();
                        Banish();
                    }
                    break;
                case State.BANISHED:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FindNextTarget(State stateToReachFound, State stateToReachNotFound)
        {
            Vector3 targetPos;

            // if target crystal is not free, try to find another one
            CrystalHandler destinationCrystalHandler = FindNearestFreeCrystal();

            // Decide first Target
            if (destinationCrystalHandler != null)
            {
                Vector3 agentActualDestination = GameManager.GetApproxApproachPosition(destinationCrystalHandler, transform.position);
                _targetCrystalHandler = destinationCrystalHandler;
                targetPos = agentActualDestination;
                ChangeState(stateToReachFound);
            }
            else // no crystal is free, go away
            {
                targetPos = _spawnPointTransform.transform.position;
                ChangeState(stateToReachNotFound);
            }
            PrepareForLocomotion(targetPos, goblinData.normalizedMaxAllowedSpeed);
        }

        private void StopAgent()
        {
            _agent.ResetPath();
            _anim.SetBool(MovingLabel, false);
        }

        private void PrepareForGathering()
        {
            _gatherTimer = 0f;
            StopAgent();
            _anim.SetTrigger(StartGatherLabel);
        }

        private void HandleLocomotionAnimations(float animatorMaxFwdFactor)
        {
            Vector3 nextCorner = _agent.steeringTarget;
            Vector3 agentFwd = transform.forward;
            Vector3 directionToNextCorner = nextCorner - transform.position;
            directionToNextCorner = new Vector3(directionToNextCorner.x, 0, directionToNextCorner.z);
            directionToNextCorner.Normalize();

            float degsToTurn = Vector3.SignedAngle(agentFwd, directionToNextCorner, Vector3.up);
            float animatorTurnFactor = Mathf.Sin(degsToTurn * Mathf.Deg2Rad);
            float dampTime = goblinData.dampTime;

            _anim.SetFloat(BlendTreeLocomotionXLabel, animatorTurnFactor, dampTime, Time.deltaTime);

            float animatorNormalizedSpeed = 0f;

            if (Mathf.Abs(degsToTurn) > goblinData.degreeAlignThreshold)
            {
                animatorNormalizedSpeed = goblinData.locomotionAnimatorDeadZone;
            }
            else
            {
                animatorNormalizedSpeed = animatorMaxFwdFactor;
            }

            _anim.SetFloat(BlendTreeLocomotionYLabel, animatorNormalizedSpeed, dampTime, Time.deltaTime);
        }

        public void ApplyDamage(int damage)
        {
            if (_currentHealth <= 0) return;

            Invoke(nameof(PlayBanishAudioEffect), 0f);

            _currentHealth = _currentHealth - damage;
            _currentHealth = Mathf.Max(0, _currentHealth);

            if (_currentHealth == 0)
            {
                _anim.SetTrigger(BanishLabel);
                Banish(true);
            }

            UpdateHeartsUI(_currentHealth);
        }

        private void Banish(bool byPlayer = false)
        {
            if (_goblinBanished) return;
            // Debug.Log($"{transform.GetInstanceID()} is being banished.");
            _goblinBanished = true;
            ChangeState(State.BANISHED);

            StopAgent();

            GameManager.Instance.OnGoblinBanished(byPlayer);


            if (_linkedCrystalHandler != null)
            {
                if (byPlayer)
                {
                    _linkedCrystalHandler.ResetParent();
                }
                else
                {
                    GameManager.Instance.OnRemovedCrystal();
                    Destroy(_linkedCrystalHandler.gameObject);
                }
                _linkedCrystalHandler = null;
            }

            // remove goblin from the scene after some time
            Invoke(nameof(RemoveGoblin), goblinData.timeToWaitToRemoveGoblin);
        }

        private void PlayBanishAudioEffect()
        {
            FantasyAudioManager.Instance.PlayEffect(audioSource, goblinData.banishClip);
        }

        public void RemoveGoblin()
        {
            Destroy(gameObject);
        }

        public void HandleBallHit(int damage, ElementType sourceElement)
        {
            if (_currentState == State.BANISHED) return;
            if (sourceElement != activeElementType) return;
            ApplyDamage(damage);
        }

        private void OnValidate()
        {
            UpdateHeartsUI(_currentHealth);

            float progress = Mathf.Clamp01(_gatherTimer / goblinData.gatherTime);
            UpdateGatherProgressBar(progress);

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            Color goblinColor = elementSos.GetColorFromElement(activeElementType);
            Material newMaterial = new Material(goblinSkinMaterial);
            float startAlpha = goblinSkinMaterial.color.a;
            newMaterial.color = new Color(goblinColor.r, goblinColor.g, goblinColor.b, startAlpha);
            skinRenderer.material = newMaterial;
        }

        public void UpdateHeartsUI(int health)
        {
            for (int i = 0; i < healthHearts.Length; i++)
            {
                healthHearts[i].enabled = i < health;
            }
        }

        public void UpdateGatherProgressBar(float value)
        {
            value = Mathf.Clamp01(value);
            progressBar.fillAmount = value;
        }
    }
}