using UnityEngine;

namespace FIMSpace.FTail
{
    [AddComponentMenu("FImpossible Creations/Turbo Tail Animator (Burst)")]
    [HelpURL("https://assetstore.unity.com/packages/tools/animation/tail-animator-121819")]
    public partial class TurboTailAnimator : MonoBehaviour
    {
#if FIMPOSSIBLE_JOBSANDBURST_READY

        // Main Tail Animator Properties
        [Tooltip("Blend Source Animation (keyframed / unanimated) and Tail Animator")]
        [FPD_Suffix(0f, 1f)]
        public float TailAnimatorAmount = 1f;

        [FPD_FixedCurveWindow(0f,0f, 1f, 1f, 0.2f, 1f, 0.4f)]
        public AnimationCurve TailAnimatorAmountCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);

        [Tooltip("Blending Slithery - smooth & soft tentacle like movement (value = 1)\nwith more stiff & springy motion (value = 0)\n\n0: Stiff somewhat like tree branch\n1: Soft like squid tentacle / Animal tail")]
        [Range(0f, 1.2f)]
        public float Slithery = 1f;

        [Tooltip("How curly motion should be applied to tail segments")]
        [Range(0f, 1f)]
        public float Curling = 0.5f;

        [Tooltip("Elastic spring effect making motion more 'meaty'")]
        [Range(0f, 1f)]
        public float Springiness = 0.0f;

        [Tooltip("If you want to limit stretching/gumminess of position motion when object moves fast. Recommended adjust to go with it under 0.3 value.\nValue = 1: Unlimited stretching")]
        [Range(0f, 1f)]
        public float MaxStretching = .375f;

        [Tooltip("If your object moves very fast making tail influenced by speed too much then you can controll it with this parameter")]
        [FPD_Suffix(0f, 1.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped)]
        public float MotionInfluence = 1f;
        [Tooltip("Additional Y influence controll useful when your character is jumping (works only when MotionInfluence value is other than 100%)")]
        [Range(0f, 1f)] public float MotionInfluenceInY = 1f;


        [Tooltip( "Limiting max rotation angle for each tail segment" )]
        [FPD_Suffix( 1f, 181f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "\u00B0" )]
        public float AngleLimit = 181f;
        [Tooltip( "If you need specific axis to be limited.\nLeave unchanged to limit all axes." )]
        public Vector3 AngleLimitAxis = Vector3.zero;
        [Tooltip( "If you want limit axes symmetrically leave this parameter unchanged, if you want limit one direction of axis more than reversed, tweak this parameter" )]
        public Vector2 LimitAxisRange = Vector2.zero;
        [Tooltip( "If limiting shouldn't be too rapidly performed" )]
        [Range( 0f, 1f )]
        public float LimitSmoothing = 0.5f;


        [Tooltip("Reaction Speed is defining how fast tail segments will return to target position, it gives animation more underwater/floaty feeling if it's lower")]
        [Range(0f, 1f)]
        public float ReactionSpeed = .9f;

        [Tooltip("Rotation speed is defining how fast tail segments will return to target rotation, it gives animation more lazy feeling if it's lower")]
        [Range(0f, 1f)]
        public float RotationRelevancy = 1f;

        [Tooltip("Making tail animate also roll rotation. Lower Rotation Relevancy to see the effect.")]
        public bool AnimateRoll = false;


        // Performance Properties

        public enum EFDeltaType { DeltaTime, UnscaledDeltaTime, FixedDeltaTime }

        [Tooltip("Time source for simulation and waving. FixedDeltaTime uses the fixed timestep once per rendered update")]
        public EFDeltaType DeltaType = EFDeltaType.DeltaTime;

        [Tooltip("Restore the source pose in FixedUpdate for physics-driven animation. Simulation and output remain jobified in LateUpdate")]
        public bool AnimatePhysics = false;

        [Tooltip("Target simulation updates per second. Zero is unlimited and simulates once per update loop. Lower positive values reduce CPU cost")]
        [UnityEngine.Serialization.FormerlySerializedAs("SimulationFrequency")]
        [Range(0, 150)] public int UpdateRate = 90;

        [Tooltip("Limits catch-up work after a frame spike")]
        [Range(1, 5)] public int MaxSimulationStepsPerFrame = 3;

        [Tooltip("Direct parent-child chains can write rotations only. It can improve final performance")]
        public bool PreferRotationOnly = false;

        [Tooltip("Restore tail transforms to their source pose before simulation. Disable only when an Animator or another system writes every tail transform before Turbo Tail's LateUpdate")]
        public bool Calibrate = true;

        public enum EDetachChildrenMode { None, DetachStatic, DetachForAnimatorBones }
        [Tooltip( "Flattens the configured tail chain at startup to reduce Unity transform-hierarchy work.\nThis setting is applied when the component is enabled and cannot be changed while playing" )]
        public EDetachChildrenMode DetachMode = EDetachChildrenMode.None;
        [Tooltip( "After detaching, the bones are scattered across hierarchy, you can hide visibility of them in the hierarchy window to keep hierarchy cleaner" )]
        public bool HideDetachedBones = false;

        [Tooltip("Skip simulation when none of the assigned renderers are visible. Leave empty to disable renderer culling")]
        public Renderer[] OptimizeWithMeshes = null;

        [Tooltip("Skip simulation farther than this distance from the main camera. Zero disables distance culling")]
        [Min(0f)] public float CullAtDistance = 0f;

        [Tooltip("Seconds to fade tail motion out of and back into camera range. Zero makes the transition immediate")]
        [Min(0f)] public float DistanceCullingTransitionDuration = 0.5f;



        private void Reset()
        {
            if (StartBone) return;

            SkinnedMeshRenderer[] skins = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skins.Length == 0) skins = transform.root.GetComponentsInChildren<SkinnedMeshRenderer>();

            Transform firstBone = null;
            int largestBoneCount = 0;

            for (int i = 0; i < skins.Length; i++)
            {
                Transform[] bones = skins[i].bones;

                // Prefer this transform when the component is already on a skeleton bone.
                for (int b = 0; b < bones.Length; b++)
                {
                    if (bones[b] != transform) continue;

                    StartBone = transform;
                    GetGhostChain(StartBone);
                    return;
                }

                if (bones.Length > largestBoneCount && bones[0])
                {
                    largestBoneCount = bones.Length;
                    firstBone = bones[0];
                }
            }

            if (firstBone)
            {
                StartBone = firstBone;
                GetGhostChain(StartBone);
            }
        }

        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            if (!initialized) return;

            if (DetachMode != EDetachChildrenMode.None) DetachChildrenTransforms();
            TurboTailAnimatorUpdater.AddToUpdate(this);
        }

        private void OnDisable()
        {
            TurboTailAnimatorUpdater.RemoveFromUpdate(this);
            RestoreChildrenHierarchy();
        }

        private void OnDestroy() 
        { 
            TurboTailAnimatorUpdater.RemoveFromUpdate(this); 
        }

        private void OnValidate()
        {
            UpdateRate = Mathf.Max(0, UpdateRate);
            CheckForColliderDuplicatesAndNulls();
            if (initialized == false) return;
            RefreshRuntimeParameters();
        }

        /// <summary>Call after changing Turbo Tail parameters from code, including collision, gravity and wind settings. Updates are applied before the next simulation.</summary>
        public void RefreshRuntimeParameters()
        {
            if (!initialized) return;
        
            USER_RefreshTailBonesMotionParametersAfterManualChanges();
            TurboTailAnimatorUpdater.MarkParametersDirty(this);
        }

#endif
    }
}
