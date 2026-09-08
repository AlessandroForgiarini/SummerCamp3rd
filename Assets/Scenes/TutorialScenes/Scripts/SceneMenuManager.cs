using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneReference : ISerializationCallbackReceiver
{
#if UNITY_EDITOR
    [Tooltip("Drag and drop your Scene asset here")]
    public SceneAsset sceneAsset;
#endif

    [HideInInspector]
    [SerializeField] private string sceneName = string.Empty;
    public string SceneName => sceneName;

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
#endif
    }

    public void OnAfterDeserialize() { }
}

public class SceneMenuManager : MonoBehaviour
{
    public static SceneMenuManager Instance { get; private set; }

    [Header("Scene Configuration")]
    [SerializeField] private SceneReference menuScene;
    [SerializeField] private List<SceneReference> scenes = new List<SceneReference>();

    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonContainer;

    [Header("XR Hold Settings")]
    [Tooltip("Target InputAction for B button (e.g., <XRController>{RightHand}/secondaryButton)")]
    [SerializeField] private InputActionProperty bButtonAction;

    [Tooltip("Duration in seconds to hold B before returning to the menu")]
    [SerializeField] private float holdDuration = 1.0f;

    [Tooltip("Optional: Fill bar/slider to visually show hold progress in VR")]
    [SerializeField] private Image holdProgressBar;

    private float currentHoldTimer = 0f;
    private bool isHolding = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        bButtonAction.action?.Enable();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        bButtonAction.action?.Disable();
    }

    private void Start()
    {
        GenerateSceneButtons();
        UpdateUIVisibility(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        // Only track hold if we are outside the menu scene
        if (SceneManager.GetActiveScene().name.Equals(menuScene.SceneName))
            return;

        HandleXRHoldInput();
    }

    private void HandleXRHoldInput()
    {
        bool isPressed = bButtonAction.action != null && bButtonAction.action.IsPressed();

        if (isPressed)
        {
            currentHoldTimer += Time.deltaTime;

            if (holdProgressBar != null)
            {
                holdProgressBar.gameObject.SetActive(true);
                holdProgressBar.fillAmount = Mathf.Clamp01(currentHoldTimer / holdDuration);
            }

            if (currentHoldTimer >= holdDuration && !isHolding)
            {
                isHolding = true;
                ResetHoldUI();
                LoadMenuScene();
            }
        }
        else
        {
            ResetHoldUI();
        }
    }

    private void ResetHoldUI()
    {
        currentHoldTimer = 0f;
        isHolding = false;

        if (holdProgressBar != null)
        {
            holdProgressBar.fillAmount = 0f;
            holdProgressBar.gameObject.SetActive(false);
        }
    }

    private void GenerateSceneButtons()
    {
        if (buttonPrefab == null || buttonContainer == null) return;

        foreach (SceneReference sceneRef in scenes)
        {
            string targetName = sceneRef.SceneName;
            if (string.IsNullOrEmpty(targetName)) continue;

            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);

            TextMeshProUGUI tmpText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = targetName;
            }
            else
            {
                Text legacyText = buttonObj.GetComponentInChildren<Text>();
                if (legacyText != null) legacyText.text = targetName;
            }

            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => LoadTargetScene(targetName));
            }
        }
    }

    public void LoadTargetScene(string targetSceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"Cannot load '{targetSceneName}'. Ensure it is added to Build Settings.");
        }
    }

    public void LoadMenuScene()
    {
        if (!string.IsNullOrEmpty(menuScene.SceneName))
        {
            LoadTargetScene(menuScene.SceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateUIVisibility(scene.name);
        ResetHoldUI();
    }

    private void UpdateUIVisibility(string currentSceneName)
    {
        bool isMenu = currentSceneName.Equals(menuScene.SceneName);

        if (menuPanel != null)
            menuPanel.SetActive(isMenu);
    }
}