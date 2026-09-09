using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[DisallowMultipleComponent]
public class XRSimulatorAutoToggle : MonoBehaviour
{
    public static XRSimulatorAutoToggle Instance { get; private set; }

    [Header("References")]
    [Tooltip("The XR Device Simulator.")]
    [SerializeField] private XRDeviceSimulator simulatorObject;

    [Tooltip("The XR Origin.")]
    [SerializeField] private XROrigin xrOrigin;

    [Header("Detection")]
    [Tooltip("How long to keep watching for a headset that comes up late. Detection is " +
             "also event-driven, so a headset connecting after this window still switches " +
             "the simulator off.")]
    [SerializeField] private float detectionWindow = 20f;

    [Tooltip("Log every detection signal. On a Quest build, read these with:  adb logcat -s Unity")]
    [SerializeField] private bool verboseLogging = true;

    [Header("Simulated Camera Height")]
    [Tooltip("Eye height in metres used while simulating.")]
    [SerializeField] private float simulatedEyeHeight = 1.6f;

    public static bool IsSimulating { get; private set; }

    private static bool SimulatorAllowedOnThisPlatform =>
#if UNITY_EDITOR
        true;
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        true;
#else
        false;
#endif

    private XROrigin.TrackingOriginMode originalOriginMode;
    private float originalCameraYOffset;
    private bool originalModeCached;
    private bool decisionLocked;
    private bool isDuplicate;
    private bool hasApplied;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        IsSimulating = false;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            isDuplicate = true;
            Log("A duplicate XRSimulatorAutoToggle was found — removing this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (simulatorObject != null && simulatorObject.gameObject == gameObject)
        {
            Debug.LogError("[XRSimulatorAutoToggle] This component must NOT live on the " +
                           "XR Device Simulator object. Move it to the Player root.", this);
        }

        if (xrOrigin == null)
            xrOrigin = GetComponentInChildren<XROrigin>(true);

        CacheOriginSettings();

        if (!SimulatorAllowedOnThisPlatform)
        {
            Log("Standalone headset build — the XR Device Simulator is disabled unconditionally.");
            decisionLocked = true;
            Apply(true);
            return;
        }

        bool loaderActive = IsXrLoaderActive();
        Log($"Awake — activeLoader: {loaderActive}, isDeviceActive: {XRSettings.isDeviceActive}, " +
            $"hmdDevices: {CountHmdDevices()}, loadedDeviceName: '{XRSettings.loadedDeviceName}', " +
            $"frameCount: {Time.frameCount}");

        Apply(loaderActive);
    }

    private void OnEnable()
    {
        if (isDuplicate) return;

        InputDevices.deviceConnected += OnDeviceConnected;
        StartCoroutine(ConfirmationRoutine());
    }

    private void OnDisable()
    {
        if (isDuplicate) return;

        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void CacheOriginSettings()
    {
        if (xrOrigin == null || originalModeCached) return;

        originalOriginMode = xrOrigin.RequestedTrackingOriginMode;
        originalCameraYOffset = xrOrigin.CameraYOffset;
        originalModeCached = true;
    }

    private static bool IsXrLoaderActive()
    {
        XRGeneralSettings settings = XRGeneralSettings.Instance;
        return settings != null
            && settings.Manager != null
            && settings.Manager.activeLoader != null;
    }

    private static int CountHmdDevices()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);

        int count = 0;
        foreach (InputDevice device in devices)
        {
            if (device.isValid) count++;
        }
        return count;
    }

    private static bool IsHeadsetFullyUp()
    {
        return IsXrLoaderActive() && XRSettings.isDeviceActive && CountHmdDevices() > 0;
    }

    private void OnDeviceConnected(InputDevice device)
    {
        if (isDuplicate) return;
        if (!device.isValid) return;
        if ((device.characteristics & InputDeviceCharacteristics.HeadMounted) == 0) return;

        Log($"Headset connected: '{device.name}' — disabling the XR Device Simulator.");
        decisionLocked = true;
        Apply(true);
    }

    private IEnumerator ConfirmationRoutine()
    {
        float elapsed = 0f;

        while (elapsed < detectionWindow)
        {
            if (!decisionLocked && IsHeadsetFullyUp())
            {
                Log("Headset confirmed during the detection window.");
                decisionLocked = true;
                Apply(true);
            }

            if (!IsSimulating && simulatorObject != null && simulatorObject.gameObject.activeSelf)
            {
                Log("The simulator was re-activated externally — disabling it again.");
                simulatorObject.gameObject.SetActive(false);
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!decisionLocked && !IsSimulating && SimulatorAllowedOnThisPlatform)
        {
            Log($"An XR loader is active but no headset reported in within {detectionWindow}s " +
                $"(isDeviceActive: {XRSettings.isDeviceActive}, hmdDevices: {CountHmdDevices()}). " +
                "Falling back to the XR Device Simulator.");
            Apply(false);
        }
    }

    private void Apply(bool headsetPresent)
    {
        if (isDuplicate) return;

        bool simulating = !headsetPresent && SimulatorAllowedOnThisPlatform;

        if (hasApplied && IsSimulating == simulating)
            return;

        hasApplied = true;
        IsSimulating = simulating;

        if (simulatorObject != null)
        {
            simulatorObject.gameObject.SetActive(simulating);
            simulatorObject.enabled = simulating;
        }

        ApplyOriginMode(simulating);

        Debug.Log(simulating
            ? "[XRSimulatorAutoToggle] No headset — running with the XR Device Simulator."
            : "[XRSimulatorAutoToggle] Headset present — XR Device Simulator disabled.");
    }

    private void ApplyOriginMode(bool simulating)
    {
        if (xrOrigin == null) return;

        CacheOriginSettings();

        if (simulating)
        {
            xrOrigin.CameraYOffset = simulatedEyeHeight;
            xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
        }
        else
        {
            xrOrigin.CameraYOffset = originalCameraYOffset;
            xrOrigin.RequestedTrackingOriginMode = originalOriginMode;
        }
    }

    private void Log(string message)
    {
        if (verboseLogging)
            Debug.Log($"[XRSimulatorAutoToggle] {message}");
    }
}
