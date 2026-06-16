using UnityEngine;

/// <summary>
/// This script makes the attached GameObject always face the main camera.
/// </summary>
///
namespace PbwanskaasProductions.BasicThirdPersonController
{
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;

        /// <summary>
        /// Initializes the main camera reference.
        /// </summary>
        void Start()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// Called after all Update functions have been called.
        /// Ensures the object's rotation matches the camera's rotation.
        /// </summary>
        void LateUpdate()
        {
            if (mainCamera == null) return;

            transform.rotation = mainCamera.transform.rotation;
        }
    }
}
