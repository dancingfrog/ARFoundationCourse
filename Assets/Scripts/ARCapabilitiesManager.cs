using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Logger = Core.Logger;

public class ARCapabilitiesManager : MonoBehaviour
{
    private ARSession arSession;
    
    private void Awake()
    {
        arSession = FindObjectOfType<ARSession>();
        arSession.enabled = false;
    }
    
    // Start is called before first frame update
    private void Start ()
    {
        StartCoroutine(CheckARSupport());
    }

    private IEnumerator CheckARSupport()
    {
        if (ARSession.state == ARSessionState.None
            || ARSession.state == ARSessionState.CheckingAvailability)
        {
            Logger.Instance.LogInfo("Checking if AR is available...");
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state != ARSessionState.Unsupported)
        {
            Logger.Instance.LogInfo("This device supports AR sessions");
            arSession.enabled = true;
        }
        else
        {
            Logger.Instance.LogInfo("This device does not support AR sessions");
            arSession.enabled = false;
        }
        
        throw new System.NotImplementedException();
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
        Logger.Instance.LogInfo($"AR session state changed: {obj.state}");
    }
    
    // Update is called once per frame; assume non-blocking (real-time)
    private void Update() {}
}
