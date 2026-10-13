using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace DriftWash
{
    public class CinemachineZoom : MonoBehaviour
    {
        [Header("References")]
        public InputReader inputReader;
        private CinemachineOrbitalFollow orbitalFollow;

        [Header("Zoom Settings")]
        public float minRadius = 3f;
        public float maxRadius = 10f;
        public float zoomSpeed = 2f;
        public float smoothSpeed = 5f;

        private float targetRadius;

        void Start()
        {
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow != null)
            {
                targetRadius = orbitalFollow.Radius;
            }
        }

        void Update()
        {
            if (orbitalFollow == null || inputReader == null) return;

            // Reads the scroll wheel value from your custom scriptable object path!
            float scrollValue = inputReader.ZoomValue;

            if (Mathf.Abs(scrollValue) > 0.01f)
            {
                // Scrolling up zooms in, scrolling down zooms out
                targetRadius -= (scrollValue / 120f) * zoomSpeed;
                targetRadius = Mathf.Clamp(targetRadius, minRadius, maxRadius);
            }

            // Smoothly move the camera radius
            orbitalFollow.Radius = Mathf.Lerp(orbitalFollow.Radius, targetRadius, Time.deltaTime * smoothSpeed);
        }
    }
}