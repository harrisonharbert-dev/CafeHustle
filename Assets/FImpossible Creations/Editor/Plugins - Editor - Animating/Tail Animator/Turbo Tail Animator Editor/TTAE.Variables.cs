#if FIMPOSSIBLE_JOBSANDBURST_READY
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator_Editor
    {
        Transform startBone { get { return Get.StartBone == null ? Get.transform : Get.StartBone; } set { Get.StartBone = value; } }

        // Main Properties
        protected SerializedProperty sp_StartBone;
        protected SerializedProperty sp_TailAnimatorAmount;
        protected SerializedProperty sp_TailAnimatorAmountCurve;

        protected SerializedProperty sp_Slithery;
        protected SerializedProperty sp_SlitCurve;
        protected SerializedProperty sp_UseSlitCurve;

        protected SerializedProperty sp_Curling;
        protected SerializedProperty sp_CurlingCurve;
        protected SerializedProperty sp_UseCurlingCurve;

        protected SerializedProperty sp_Springiness;
        protected SerializedProperty sp_SpringCurve;
        protected SerializedProperty sp_UseSpringCurve;

        protected SerializedProperty sp_MaxStretching;
        protected SerializedProperty sp_MotionInfluence;
        protected SerializedProperty sp_AngleLimit;
        protected SerializedProperty sp_AngleLimitAxis;
        protected SerializedProperty sp_LimitAxisRange;
        protected SerializedProperty sp_LimitSmoothing;

        protected SerializedProperty sp_useCollision;
        protected SerializedProperty sp_CollisionMode;
        protected SerializedProperty sp_CollisionDamping;
        protected SerializedProperty sp_UseCollDampingCurve;
        protected SerializedProperty sp_CollDampingCurve;
        protected SerializedProperty sp_IncludedColliders;
        protected SerializedProperty sp_CollideWithDisabledColliders;
        protected SerializedProperty sp_UseSlipperyCurve;
        protected SerializedProperty sp_SlipperyCurve;
        protected SerializedProperty sp_CollisionSlippery;
        protected SerializedProperty sp_UseCollisionReflectCurve;
        protected SerializedProperty sp_CollisionReflectCurve;
        protected SerializedProperty sp_ReflectCollision;
        protected SerializedProperty sp_CollidersScaleCurve;
        protected SerializedProperty sp_CollidersScaleMul;

        protected SerializedProperty sp_ReactSpeed;
        protected SerializedProperty sp_RotRelev;
        protected SerializedProperty sp_PosCurve;
        protected SerializedProperty sp_RotCurve;
        protected SerializedProperty sp_AnimateRoll;

        // Waving
        protected SerializedProperty sp_wavType;
        protected SerializedProperty sp_useWav;
        protected SerializedProperty sp_cosAd;
        protected SerializedProperty sp_wavSp;
        protected SerializedProperty sp_wavRa;
        protected SerializedProperty sp_wavAx;
        protected SerializedProperty sp_altWave;
        protected SerializedProperty sp_FixedCycle;

        // Features
        protected SerializedProperty sp_gravity;
        protected SerializedProperty sp_UseGravCurv;
        protected SerializedProperty sp_GravityCurve;

        protected SerializedProperty sp_UseWind;
        protected SerializedProperty sp_WindEffectPower;
        protected SerializedProperty sp_WindTurbulencePower;
        protected SerializedProperty sp_WindWorldNoisePower;

        // Performance
        protected SerializedProperty sp_DeltaType;
        protected SerializedProperty sp_AnimatePhysics;
        protected SerializedProperty sp_UpdateRate;
        protected SerializedProperty sp_MaxSimulationSteps;
        protected SerializedProperty sp_PreferRotationOnly;
        protected SerializedProperty sp_Calibrate;
        protected SerializedProperty sp_DetachChildren;
        protected SerializedProperty sp_HideDetached;
        protected SerializedProperty sp_OptimizeWithMeshes;
        protected SerializedProperty sp_CullAtDistance;
        protected SerializedProperty sp_DistanceCullingTransitionDuration;


        protected virtual void OnEnable()
        {
            FGUI_Finders.ResetFinders();

            sp_StartBone = serializedObject.FindProperty("StartBone");
            sp_TailAnimatorAmount = serializedObject.FindProperty("TailAnimatorAmount");
            sp_TailAnimatorAmountCurve = serializedObject.FindProperty("TailAnimatorAmountCurve");

            // Main Properties
            sp_Slithery = serializedObject.FindProperty("Slithery");
            sp_SlitCurve = serializedObject.FindProperty("SlitheryCurve");
            sp_UseSlitCurve = serializedObject.FindProperty("UseSlitheryCurve");

            sp_Curling = serializedObject.FindProperty("Curling");
            sp_CurlingCurve = serializedObject.FindProperty("CurlingCurve");
            sp_UseCurlingCurve = serializedObject.FindProperty("UseCurlingCurve");

            sp_Springiness = serializedObject.FindProperty("Springiness");
            sp_SpringCurve = serializedObject.FindProperty("SpringCurve");
            sp_UseSpringCurve = serializedObject.FindProperty("UseSpringCurve");

            sp_MaxStretching = serializedObject.FindProperty("MaxStretching");
            sp_MotionInfluence = serializedObject.FindProperty("MotionInfluence");
            sp_AngleLimit = serializedObject.FindProperty("AngleLimit");
            sp_AngleLimitAxis = serializedObject.FindProperty("AngleLimitAxis");
            sp_LimitAxisRange = serializedObject.FindProperty("LimitAxisRange");
            sp_LimitSmoothing = serializedObject.FindProperty("LimitSmoothing");

            sp_ReactSpeed = serializedObject.FindProperty("ReactionSpeed");
            sp_RotRelev = serializedObject.FindProperty("RotationRelevancy");
            sp_PosCurve = serializedObject.FindProperty("PosCurve");
            sp_RotCurve = serializedObject.FindProperty("RotCurve");
            sp_AnimateRoll = serializedObject.FindProperty( "AnimateRoll" );

            // Features
            sp_gravity = serializedObject.FindProperty("Gravity");
            sp_UseGravCurv = serializedObject.FindProperty("UseGravityCurve");
            sp_GravityCurve = serializedObject.FindProperty("GravityCurve");

            sp_UseWind = serializedObject.FindProperty("UseWind");
            sp_WindEffectPower = serializedObject.FindProperty("WindEffectPower");
            sp_WindTurbulencePower = serializedObject.FindProperty("WindTurbulencePower");
            sp_WindWorldNoisePower = serializedObject.FindProperty("WindWorldNoisePower");

            sp_useCollision = serializedObject.FindProperty("UseCollision");
            sp_CollisionMode = serializedObject.FindProperty("CollisionMode");
            sp_CollisionDamping = serializedObject.FindProperty("CollisionDamping");
            sp_UseCollDampingCurve = serializedObject.FindProperty("UseCollDampingCurve");
            sp_CollDampingCurve = serializedObject.FindProperty("CollDampingCurve");
            sp_IncludedColliders = serializedObject.FindProperty("IncludedColliders");
            sp_CollideWithDisabledColliders = serializedObject.FindProperty("CollideWithDisabledColliders");
            sp_UseSlipperyCurve = serializedObject.FindProperty("UseSlipperyCurve");
            sp_SlipperyCurve = serializedObject.FindProperty("SlipperyCurve");
            sp_CollisionSlippery = serializedObject.FindProperty("CollisionSlippery");
            sp_UseCollisionReflectCurve = serializedObject.FindProperty("UseCollisionReflectCurve");
            sp_CollisionReflectCurve = serializedObject.FindProperty("CollisionReflectCurve");
            sp_ReflectCollision = serializedObject.FindProperty("ReflectCollision");
            sp_CollidersScaleCurve = serializedObject.FindProperty("CollidersScaleCurve");
            sp_CollidersScaleMul = serializedObject.FindProperty("CollidersScaleMul");

            if (sp_useCollision.boolValue) { drawCollisions = true; drawInclud = false; }

            // Waving
            sp_wavType = serializedObject.FindProperty("WavingType");
            sp_useWav = serializedObject.FindProperty("UseWaving");
            sp_cosAd = serializedObject.FindProperty("CosinusAdd");
            sp_wavSp = serializedObject.FindProperty("WavingSpeed");
            sp_wavRa = serializedObject.FindProperty("WavingRange");
            sp_wavAx = serializedObject.FindProperty("WavingAxis");
            sp_altWave = serializedObject.FindProperty("AlternateWave");
            sp_FixedCycle = serializedObject.FindProperty("FixedCycle");

            // Performance
            sp_DeltaType = serializedObject.FindProperty("DeltaType");
            sp_AnimatePhysics = serializedObject.FindProperty("AnimatePhysics");
            sp_UpdateRate = serializedObject.FindProperty("UpdateRate");
            sp_MaxSimulationSteps = serializedObject.FindProperty("MaxSimulationStepsPerFrame");
            sp_PreferRotationOnly = serializedObject.FindProperty("PreferRotationOnly");
            sp_Calibrate = serializedObject.FindProperty("Calibrate");
            sp_DetachChildren = serializedObject.FindProperty("DetachMode");
            sp_HideDetached = serializedObject.FindProperty( "HideDetachedBones" );
            sp_OptimizeWithMeshes = serializedObject.FindProperty("OptimizeWithMeshes");
            sp_CullAtDistance = serializedObject.FindProperty("CullAtDistance");
            sp_DistanceCullingTransitionDuration = serializedObject.FindProperty("DistanceCullingTransitionDuration");

            SetupLangs();
        }
    }
}
#endif
