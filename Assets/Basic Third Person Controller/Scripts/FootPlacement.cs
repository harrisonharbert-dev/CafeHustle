using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// This class handles the foot placement of the character on the ground using Inverse Kinematics (IK).
/// It adjusts the feet position and rotation to match the terrain.
/// </summary>
/// 
namespace PbwanskaasProductions.BasicThirdPersonController
{
    public class FootPlacement : MonoBehaviour
    {
        [Header("Components")]
        private Animator anim;

        [Header("Bone References (Drag from Skeleton)")]
        public Transform leftFootBone;
        public Transform rightFootBone;

        [Header("IK Targets")]
        public Transform leftFootIKTarget;
        public Transform rightFootIKTarget;

        [Header("IK Constraints")]
        public TwoBoneIKConstraint leftFootIKConstraint;
        public TwoBoneIKConstraint rightFootIKConstraint;

        [Header("Raycast Settings")]
        public LayerMask layerMask;
        [Range(0f, 0.2f)]
        public float distanceToGround = 0.05f;
        [Range(0f, 2f)]
        public float raycastDistance = 1.5f;

        [Header("Manual Correction Factor")]
        [Tooltip("Use this to fine-tune the foot rotation.")]
        public Vector3 rotationOffset;

        /// <summary>
        /// Initializes the Animator component.
        /// </summary>
        void Start()
        {
            anim = GetComponent<Animator>();
        }

        /// <summary>
        /// Called after all Update functions have been called. This is a good place to handle procedural animation and IK.
        /// </summary>
        void LateUpdate()
        {
            if (anim == null || leftFootBone == null || rightFootBone == null) return;

            Quaternion leftFootAnimatedRotation = leftFootBone.rotation;
            Quaternion rightFootAnimatedRotation = rightFootBone.rotation;
            Vector3 leftFootAnimatedPosition = leftFootBone.position;
            Vector3 rightFootAnimatedPosition = rightFootBone.position;

            ProcessFoot(leftFootIKTarget, leftFootAnimatedPosition, leftFootAnimatedRotation);
            ProcessFoot(rightFootIKTarget, rightFootAnimatedPosition, rightFootAnimatedRotation);
        }

        /// <summary>
        /// Processes the position and rotation of a single foot's IK target.
        /// </summary>
        /// <param name="ikTarget">The IK target to be processed.</param>
        /// <param name="animatedPosition">The original animated position of the foot bone.</param>
        /// <param name="animatedRotation">The original animated rotation of the foot bone.</param>
        void ProcessFoot(Transform ikTarget, Vector3 animatedPosition, Quaternion animatedRotation)
        {
            RaycastHit hit;
            Ray ray = new Ray(animatedPosition + Vector3.up * 0.5f, Vector3.down);

            if (Physics.Raycast(ray, out hit, raycastDistance, layerMask) && hit.transform.CompareTag("Walkable"))
            {
                ikTarget.position = hit.point + new Vector3(0, distanceToGround, 0);

                Vector3 boneUp = animatedRotation * Vector3.forward;
                Quaternion rotationCorrection = Quaternion.FromToRotation(boneUp, hit.normal);
                Quaternion targetRotation = rotationCorrection * animatedRotation;

                ikTarget.rotation = targetRotation * Quaternion.Euler(rotationOffset);

                return;
            }

            ikTarget.position = animatedPosition;
            ikTarget.rotation = animatedRotation;
        }
    }
}