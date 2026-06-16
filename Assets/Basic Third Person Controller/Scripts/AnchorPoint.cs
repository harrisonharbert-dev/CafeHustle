using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple component to mark a GameObject's position.
/// This gives the object a purpose so it's not "empty", and makes it
/// visible in the Scene view with a small blue sphere icon.
/// </summary>

namespace PbwanskaasProductions.BasicThirdPersonController
{
    public class AnchorPoint : MonoBehaviour
    {
        [Tooltip("Optional note for this anchor point. This is just for organization.")]
        public string description = "This transform is used as the main reference point for the camera system.";


        // This #if UNITY_EDITOR block ensures the code inside it is
        // only active within the Unity Editor and is removed from final game builds.
        // This is important for performance.
#if UNITY_EDITOR

        // This function draws a gizmo in the Scene view to make the object visible.
        private void OnDrawGizmos()
        {
            // Set a visible color for the gizmo.
            Gizmos.color = Color.cyan;

            // Draw a small sphere at the object's position with a fixed size.
            Gizmos.DrawSphere(transform.position, 0.15f);
        }

#endif
    }
}

