using UnityEngine;
using UnityEngine.XR;

namespace Core
{
    public class DPIManager : MonoBehaviour 
    {
        [Header("DPI Settings")]
        [SerializeField] private float targetDPI = 160f;
        [SerializeField] private bool logDPIInfo = true;
        [SerializeField] private bool enableForVR = false; // Usually not needed for VR
        
        [Header("Platform Overrides")]
        [SerializeField] private float iosTargetDPI = 326f; // iPhone standard
        [SerializeField] private float vrFallbackDPI = 160f; // VR headsets don't use DPI scaling
        
        void Start() 
        {
            // Only apply DPI scaling if appropriate for the current platform
            if (ShouldApplyDPIScaling())
            {
                AdjustForDPI();
            }
            else if (logDPIInfo)
            {
                Debug.Log($"DPI scaling skipped for platform: {Application.platform}, XR Active: {XRSettings.enabled}");
            }
        }
        
        bool ShouldApplyDPIScaling()
        {
            // Skip DPI scaling for VR unless explicitly enabled
            if (XRSettings.enabled || XRSettings.loadedDeviceName != "None")
            {
                if (logDPIInfo)
                    Debug.Log($"XR Device detected: {XRSettings.loadedDeviceName}");
                return enableForVR;
            }
            
            // Apply DPI scaling for mobile and desktop platforms
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    return true;
                    
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.LinuxPlayer:
                    return true; // Desktop can benefit from DPI scaling for high-DPI displays
                    
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    return false; // Skip in editor unless testing
                    
                default:
                    return false;
            }
        }
        
        void AdjustForDPI() 
        {
            float currentDPI = GetPlatformDPI();
            float effectiveTargetDPI = GetEffectiveTargetDPI();
            
            if (logDPIInfo)
            {
                Debug.Log($"Platform: {Application.platform}");
                Debug.Log($"Screen.dpi reports: {Screen.dpi}");
                Debug.Log($"Platform-specific DPI: {currentDPI}");
                Debug.Log($"Target DPI: {effectiveTargetDPI}");
            }
            
            if (currentDPI <= 0)
            {
                Debug.LogWarning("Could not determine valid DPI, skipping scaling");
                return;
            }
            
            float scaleFactor = effectiveTargetDPI / currentDPI;
            
            // Apply scaling to UI Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null) 
            {
                canvas.scaleFactor *= scaleFactor;
                if (logDPIInfo)
                    Debug.Log($"Applied scale factor: {scaleFactor:F2} to Canvas");
            }
            else
            {
                Debug.LogWarning("No Canvas found to apply DPI scaling");
            }
        }
        
        float GetEffectiveTargetDPI()
        {
            // Use platform-appropriate target DPI
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    return iosTargetDPI;
                    
                case RuntimePlatform.Android:
                    return targetDPI;
                    
                default:
                    return targetDPI;
            }
        }
        
        float GetPlatformDPI()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return GetAndroidDPI();
                    
                case RuntimePlatform.IPhonePlayer:
                    return GetiOSDPI();
                    
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.LinuxPlayer:
                    return GetDesktopDPI();
                    
                default:
                    return Screen.dpi > 0 ? Screen.dpi : targetDPI;
            }
        }
        
        float GetAndroidDPI()
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
            try 
            {
                using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics"))
                        {
                            activity.Call<AndroidJavaObject>("getWindowManager")
                                    .Call<AndroidJavaObject>("getDefaultDisplay")
                                    .Call("getMetrics", metrics);
                            
                            // Return densityDpi (what Android OS actually uses)
                            // Note: densityDpi is an int field, not float
                            int densityDpiInt = metrics.Get<int>("densityDpi");
                            float densityDpi = (float)densityDpiInt;
                            
                            if (logDPIInfo)
                            {
                                float xdpi = metrics.Get<float>("xdpi");
                                float ydpi = metrics.Get<float>("ydpi");
                                Debug.Log($"Android DPI - densityDpi: {densityDpi}, xdpi: {xdpi}, ydpi: {ydpi}");
                            }
                            
                            return densityDpi;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting Android DPI: {e.Message}");
                return Screen.dpi > 0 ? Screen.dpi : 160f;
            }
    #else
            return Screen.dpi > 0 ? Screen.dpi : 160f;
    #endif
        }
        
        float GetiOSDPI()
        {
    #if UNITY_IOS && !UNITY_EDITOR
            // iOS Screen.dpi should be reliable according to Unity staff
            float iOSDPI = Screen.dpi;
            
            if (logDPIInfo)
            {
                Debug.Log($"iOS Screen.dpi: {iOSDPI}");
                Debug.Log($"iOS Screen resolution: {Screen.width}x{Screen.height}");
            }
            
            // iOS has standardized DPI values, so Screen.dpi should be accurate
            return iOSDPI > 0 ? iOSDPI : 326f; // iPhone standard fallback
    #else
            return Screen.dpi > 0 ? Screen.dpi : 326f;
    #endif
        }
        
        float GetDesktopDPI()
        {
            // Desktop platforms - Screen.dpi works but may need validation
            float desktopDPI = Screen.dpi;
            
            if (logDPIInfo)
            {
                Debug.Log($"Desktop Screen.dpi: {desktopDPI}");
                Debug.Log($"Desktop Screen resolution: {Screen.width}x{Screen.height}");
            }
            
            // Validate reasonable DPI range for desktop (72-300+ DPI)
            if (desktopDPI < 50 || desktopDPI > 500)
            {
                Debug.LogWarning($"Desktop DPI seems unreasonable: {desktopDPI}, using fallback");
                return 96f; // Windows standard DPI
            }
            
            return desktopDPI;
        }
        
        // Utility method to check if we're in VR mode
        public bool IsVRActive()
        {
            return XRSettings.enabled && XRSettings.loadedDeviceName != "None";
        }
        
        // Method to force reapply DPI scaling (useful for runtime DPI changes)
        [ContextMenu("Reapply DPI Scaling")]
        public void ReapplyDPIScaling()
        {
            if (ShouldApplyDPIScaling())
            {
                AdjustForDPI();
            }
        }
    }
}
