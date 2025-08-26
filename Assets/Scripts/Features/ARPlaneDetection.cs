using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Logger = Core.Logger;

namespace Features
{
    [RequireComponent(typeof(ARPlaneManager))]
    public class ARPlaneDetection : MonoBehaviour
    {
        
        private ARPlaneManager planeManager;

        private void Awake()
        {
            planeManager = GetComponent<ARPlaneManager>();
        }

        private void OnEnable()
        {
            planeManager.planesChanged += PlanesChanged;
        }

        private void OnDisable()
        {
            planeManager.planesChanged -= PlanesChanged;
        }

        private void PlanesChanged(ARPlanesChangedEventArgs obj)
        {
            DisplayPlanesChanged("added", obj.added);
            DisplayPlanesChanged("updated", obj.updated);
            DisplayPlanesChanged("removed", obj.removed);
        }

        private void DisplayPlanesChanged(string action, IEnumerable<ARPlane> planes)
        {
            foreach (ARPlane plane in planes)
            {
                Logger.Instance.LogInfo($"Plane {action}: ARPlane trackableId ({plane.trackableId})");
            }
        }
        
        void Start()
        {
        }

        void Update()
        {
        }
    }
}
