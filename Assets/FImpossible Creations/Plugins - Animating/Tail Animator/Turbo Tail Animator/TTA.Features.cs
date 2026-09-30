#if FIMPOSSIBLE_JOBSANDBURST_READY
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator
    {
        // Curve values

        public bool UseSlitheryCurve = false;
        [FPD_FixedCurveWindow( 0, 0f, 1f, 1.2f, .1f, 0.8f, 1f, 0.9f )]
        public AnimationCurve SlitheryCurve = AnimationCurve.EaseInOut( 0f, 1f, 1f, 0.75f );

        public bool UseCurlingCurve = false;
        [FPD_FixedCurveWindow(0, 0f, 1f, 1.2f, .1f, 0.8f, 1f, 0.9f)]
        public AnimationCurve CurlingCurve = AnimationCurve.EaseInOut(0f, .75f, 1f, 1f);

        public bool UseSpringCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.9f, 0.7f, 0.2f, 0.9f)]
        public AnimationCurve SpringCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

        public bool UsePosSpeedCurve = false;
        [FPD_FixedCurveWindow( 0, 0, 1f, 1f, .2f, 1f, 0.3f, .9f )]
        public AnimationCurve PosCurve = AnimationCurve.EaseInOut( 0f, .7f, 1f, 1f );

        public bool UseRotSpeedCurve = false;
        [FPD_FixedCurveWindow( 0, 0, 1f, 1f, 0.7f, 0.7f, 0.7f, 0.9f )]
        public AnimationCurve RotCurve = AnimationCurve.EaseInOut( 0f, 1f, 1f, 0.9f );


        // Auto Waving

        [Tooltip("Using auto waving option to give floating effect")]
        public bool UseWaving = true;

        [Tooltip("Adding some variation to waving animation")]
        public bool CosinusAdd = false;

        [Tooltip("If you want few tails to wave in the same way you can set this sinus period cycle value")]
        public float FixedCycle = 0f;

        [Tooltip("How frequent swings should be")]
        public float WavingSpeed = 3f;
        [Tooltip("How big swings should be")]
        public float WavingRange = 0.8f;

        [Tooltip("What rotation axis should be used in auto waving")]
        public Vector3 WavingAxis = new Vector3(1.0f, 1.0f, 1.0f);

        public Quaternion WavingRotationOffset { get; private set; }

        [Tooltip("Type of waving animation algorithm, it can be simple trigonometric wave or animation based on noises (advanced)")]
        public FEWavingType WavingType = FEWavingType.Advanced;
        public enum FEWavingType { Simple, Advanced }

        [Tooltip("Offsetting perlin noise to generate different variation of tail rotations")]
        public float AlternateWave = 1f;


        // Collisions
        public enum ECollisionMode { SpherePerBone = 0, CapsulesAlongBones = 1 }

        [Tooltip("Makes the tail bend on selected colliders using the chosen detection shape")]
        public bool UseCollision = false;

        [Tooltip("Sphere Per Bone checks each tail bone as a sphere. Capsules Along Bones also checks the tapered space between consecutive bones. Both modes support SphereCollider, CapsuleCollider, CharacterController and BoxCollider. Boxes use per-bone point contacts in Capsules Along Bones mode")]
        public ECollisionMode CollisionMode = ECollisionMode.SpherePerBone;

        [Tooltip("Damps collision response. 0 clears and disables collision memory; 1 keeps full damping. Lower values blend toward an immediate response and make remembered directions fade faster after contact")]
        [Range(0f, 1f)]
        public float CollisionDamping = 0f;
        public bool UseCollDampingCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.2f, 0.9f, 0.6f, 0.9f)]
        public AnimationCurve CollDampingCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

        [Tooltip("Sphere, capsule, character controller and box colliders checked by this tail")]
        public List<Collider> IncludedColliders = new List<Collider>();

        [Tooltip("Collide with disabled collider components. Their GameObjects must still be active")]
        public bool CollideWithDisabledColliders = true;

        public bool UseSlipperyCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.2f, 0.9f, 0.6f, 0.9f)]
        public AnimationCurve SlipperyCurve = AnimationCurve.EaseInOut(0f, .7f, 1f, 1f);

        [Range(0f, 1f)]
        [Tooltip("How much velocity is preserved while a bone is touching a collider")]
        public float CollisionSlippery = 1f;

        public bool UseCollisionReflectCurve = false;
        [FPD_FixedCurveWindow(0, 0, 1f, 1f, 0.2f, 0.9f, 0.6f, 0.9f)]
        public AnimationCurve CollisionReflectCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.5f);

        [Range(0f, 1f)]
        [Tooltip("Changes the slithery response around recent contacts. Higher values produce a more reflective reaction")]
        public float ReflectCollision = 0f;

        [FPD_FixedCurveWindow(0f, 0f, 1f, 1f)]
        public AnimationCurve CollidersScaleCurve = AnimationCurve.EaseInOut(.4f, 1f, 1f, .75f);

        [Tooltip("Multiplier for the collision sphere radius generated for every tail bone")]
        public float CollidersScaleMul = .75f;

        /// <summary> Adds a supported collider to this tail's selective collision list </summary>
        public void AddCollider(Collider collider)
        {
            if (collider == null || !IsSupportedCollisionCollider(collider)) return;
            if (IncludedColliders == null) IncludedColliders = new List<Collider>();
            if (IncludedColliders.Contains(collider)) return;
            IncludedColliders.Add(collider);
            RefreshRuntimeParameters();
        }

        /// <summary> Removes missing and duplicate entries from the selective collision list </summary>
        public void CheckForColliderDuplicatesAndNulls()
        {
            if (IncludedColliders == null)
            {
                IncludedColliders = new List<Collider>();
                return;
            }

            HashSet<Collider> unique = new HashSet<Collider>();
            for (int i = IncludedColliders.Count - 1; i >= 0; i--)
            {
                Collider collider = IncludedColliders[i];
                if (collider == null || !unique.Add(collider)) IncludedColliders.RemoveAt(i);
            }
        }

        internal static bool IsSupportedCollisionCollider(Collider collider)
        {
            return collider is SphereCollider || collider is CapsuleCollider || collider is CharacterController || collider is BoxCollider;
        }

        internal float GetColliderSphereRadiusFor(int index)
        {
            int count;

            if (_TransformsGhostChain == null) count = 0;
            else count = _TransformsGhostChain.Count;

            float step = 1f / Mathf.Max(1, count - 1);

            return .5f * CollidersScaleMul * CollidersScaleCurve.Evaluate(step * index);
        }

        // Forces support
        public bool UseWind = false;

        [FPD_Suffix(0f, 2.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped)]
        public float WindEffectPower = 1f;
        [FPD_Suffix(0f, 2.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped)]
        public float WindTurbulencePower = 1f;
        [FPD_Suffix(0f, 1.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped)]
        public float WindWorldNoisePower = 0.5f;

        /// <summary> Full length of tail computed at initialize in world space, used for unify animation feature </summary>
        public float TailLengthForWind { get; private set; } = 0.5f;

        [Tooltip( "Spread gravity weight over tail segments" )]
        public bool UseGravityCurve = false;
        [FPD_FixedCurveWindow( 0, 0, 1f, 1f, 0.85f, .35f, 0.25f, 0.85f )]
        [Tooltip( "Spread gravity weight over tail segments" )]
        public AnimationCurve GravityCurve = AnimationCurve.EaseInOut( 0f, .65f, 1f, 1f );
        [Tooltip( "Simulate gravity weight for tail logics" )]
        public Vector3 Gravity = Vector3.zero;

        Vector3 previousWorldPosition;
    }

    // Captured at buffer creation and when RefreshRuntimeParameters marks a tail dirty
    struct TurboTailForceInput
    {
        public float3 Gravity;
        public int UseWind;
        public float TailLength;
        public float WindEffectPower;
        public float WindTurbulencePower;
        public float WindWorldNoisePower;
    }
}
#endif
