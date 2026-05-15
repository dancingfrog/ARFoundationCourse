using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Logger = Core.Logger;

public class ARCapabilitiesManager : MonoBehaviour
{
    private ARSession arSession;
    
    private void Awake()
    {
        arSession = FindFirstObjectByType<ARSession>();
        if (arSession == null)
            Debug.LogError("No ARSession found in scene");
        else
            arSession.enabled = false;
    }
    
    private void Start()
    {
        StartCoroutine(CheckARSupport());
    }

    private IEnumerator CheckARSupport()
    {
        if (ARSession.state == ARSessionState.None
            || ARSession.state == ARSessionState.CheckingAvailability)
        {
            Logger.Instance?.LogInfo("Checking if AR is available...");
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state != ARSessionState.Unsupported)
        {
            Logger.Instance?.LogInfo("This device supports AR sessions");
            arSession.enabled = true;
        }
        else
        {
            Logger.Instance?.LogInfo("This device does not support AR sessions");
            arSession.enabled = false;
        }
    }

    private void OnEnable() 
    {
        ARSession.stateChanged += ARSessionStateChanged;
    }
    
    private void OnDisable() 
    {
        ARSession.stateChanged -= ARSessionStateChanged;
    }
    
    private void ARSessionStateChanged(ARSessionStateChangedEventArgs obj) 
    {
        Logger.Instance?.LogInfo($"AR session state changed: {obj.state}");
    }
    
    private void Update() {}
}
