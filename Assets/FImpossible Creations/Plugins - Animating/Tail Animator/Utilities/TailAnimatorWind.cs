#if FIMPOSSIBLE_JOBSANDBURST_READY
using Unity.Mathematics;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace FIMSpace.FTail
{
    /// <summary>
    /// FC: Experimental class under developement
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Tail Animator Utilities/Tail Animator Wind")]
    [DefaultExecutionOrder(-5)]
    public class TailAnimatorWind : MonoBehaviour, UnityEngine.EventSystems.IDropHandler, IFHierarchyIcon
    {
        #region Hierarchy Icon

        public string EditorIconPath { get { return "Tail Animator/TailAnimatorWindIconSmall"; } }
        public void OnDrop(UnityEngine.EventSystems.PointerEventData data) { }

        #endregion

        #region Singleton

        private static TailAnimatorWind _instance;

        public static TailAnimatorWind Instance
        {
            get
            {
                if (_instance == null || !_instance.isActiveAndEnabled) return null;
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance()
        {
            _instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterLoadedInstance()
        {
            if (Instance != null) return;

            TailAnimatorWind[] winds = FindObjectsOfType<TailAnimatorWind>();
            for (int i = 0; i < winds.Length; i++)
                if (winds[i].isActiveAndEnabled) winds[i].RegisterInstance();
        }

        private void OnEnable()
        {
            if (Application.isPlaying) RegisterInstance();
        }

        private void OnDisable()
        {
            if (_instance == this) _instance = null;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void RegisterInstance()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[Tail Animator Wind] Multiple active wind components detected. Keeping '" + _instance.name + "' and disabling '" + name + "'.", this);
                enabled = false;
                return;
            }

            _instance = this;
            if (persistThroughAllScenes) DontDestroyOnLoad(gameObject);
        }

        #endregion

        //[Header("In playmode you will find this object in DontDestroyOnLoad")]
        //[FPD_Header("Main Wind Setings", 2, 4)]
        [Tooltip("Global strength of the main wind and additive turbulence applied to all tails. The resulting strength also varies procedurally over time")]
        public float power = 0.65f;
        [Tooltip("Amplitude of the separate dynamic XYZ turbulence force added to each tail")]
        public float additionalTurbulence = 1f;
        [Tooltip("Controls part of the additive turbulence noise progression. Higher values make its direction vary faster; fixed time-based variation still remains at zero")]
        public float additionalTurbSpeed = 1f;

        //[Space(7)]
        [Tooltip("Use a Unity Wind Zone's forward direction and Wind Main value for the base wind. Wind Turbulence changes the speed of additive and world-position turbulence")]
        public WindZone SyncWithUnityWindZone;
        [Tooltip("Multiplies Unity Wind Zone intensity value which is applied to the the Tail Animator wind power (not affecting unity wind zone)")]
        public float UnityWindZonePowerMul = 2f;
        [Tooltip("Multiplies Unity Wind Zone Wind Turbulence value which is applied to the Tail Aniamator turbulence speed (not affecting unity wind zone)")]
        public float UnityWindZoneTurbMul = 1f;

        //[Header("Overriding wind if value below different than 0,0,0")]
        [Tooltip("A non-zero vector replaces the base procedural or Unity Wind Zone wind. Its direction determines the world-space wind direction and its magnitude scales the base strength. Additive turbulence and global strength variation remain active")]
        public Vector3 overrideWind = Vector3.zero;

        //[FPD_Header("Procedural Wind Settings (if not syncing and not overriding)", 6, 4)]
        [Tooltip("How quickly the procedural wind direction responds to noise changes. Higher values produce faster, less-smoothed direction changes")]
        [Range(0.1f, 1f)]
        public float rapidness = 0.65f;
        [Tooltip("Maximum procedural direction deviation in degrees. Applied fully to yaw and at one-sixth strength to pitch and roll")]
        [FPD_Suffix(0, 360, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "\u00B0" )]
        public float changesPower = 45f;
        [Tooltip("Speed at which the procedural wind-direction noise advances. Multiplied by Turbulence Speed")]
        public float directionChangeSpeed = 0.25f;

        //[Header("Extra")]
        [Tooltip("Speed of procedural wind-direction noise and global wind-strength fluctuations. Does not control additive or world-position turbulence")]
        [Range(0f, 10f)] public float turbulenceSpeed = 0.5f;

        //[FPD_Header("World Position Turbulence", 6, 4)]
        [Tooltip("Amount of position-based variation added to each tail's main wind strength. Requires a non-zero World Noise Power on the tail")]
        public float worldTurb = 1f;
        [Tooltip("Spatial frequency of world-position turbulence. Higher values make nearby positions differ more rapidly. This does not affect performance cost")]
        public float worldTurbScale = 512;
        [Tooltip("Speed of world-position turbulence over time. Unity Wind Zone turbulence and Unity Wind Zone Turb Mul scale this value when syncing")]
        public float worldTurbSpeed = 5f;

        //[FPD_Header("Tail Compoenents Related", 6, 4)]
        [Tooltip("Scale main wind strength using each tail's length, root Z scale, and bone count. Additive turbulence is unaffected")]
        public bool powerDependsOnTailLength = true;
        [Tooltip("Call DontDestroyOnLoad when this component becomes the active runtime instance so its GameObject persists when scenes change")]
        public bool persistThroughAllScenes = false;


        // Calculations variables
        private Vector3 targetWind = Vector3.zero;
        private Vector3 smoothWind = Vector3.zero;
        private Vector3 windVeloHelper = Vector3.zero;
        private Quaternion windOrientation = Quaternion.identity;
        private Quaternion smoothWindOrient = Quaternion.identity;
        private Quaternion smoothWindOrientHelper = Quaternion.identity;
        Vector3 smoothAddTurbulence = Vector3.zero;

        private float[] randNumbers;
        private float[] randTimes;
        private float[] randSpeeds;

        private int frameOffset = 2;
        float worldTurbulenceTime = 0f;

        Vector3 finalAddTurbulence = Vector3.zero;
        Vector3 addTurbHelper = Vector3.zero;

        void Update()
        {
            if (frameOffset > 0) { frameOffset--; return; }

            ComputeWind();
        }

        public static void Refresh()
        {
            if (Instance == null)
                UnityEngine.Debug.LogWarning("[Tail Animator Wind] No Tail Animator Wind component on the scene!");
        }

        public void AffectTailWithWind(TailAnimator2 t)
        {
            if (!t.UseWind) return;
            if (t.WindEffectPower <= 0f) return;
            if (t.TailSegments.Count <= 0) return;

            float lengthRatio = 1f;
            if (powerDependsOnTailLength)
            {
                lengthRatio = (t._TC_TailLength * t.TailSegments[0].transform.lossyScale.z) / 5f;
                if (t.TailSegments.Count > 3) lengthRatio *= Mathf.Lerp(0.7f, 3f, t.TailSegments.Count / 14f);
            }

            if (t.WindWorldNoisePower > 0f)
            {
                float worldPosTurbulence = (.5f + Mathf.Sin(worldTurbulenceTime + t.TailSegments[0].ProceduralPosition.x * worldTurbScale) / 2f) + (.5f + Mathf.Cos(worldTurbulenceTime + t.TailSegments[0].ProceduralPosition.z * worldTurbScale) / 2f);
                lengthRatio += worldPosTurbulence * worldTurb * t.WindWorldNoisePower;
            }

            lengthRatio *= t.WindEffectPower;

            if (t.WindTurbulencePower > 0f)
                t.WindEffect = new Vector3(targetWind.x * lengthRatio + finalAddTurbulence.x * t.WindTurbulencePower, targetWind.y * lengthRatio + finalAddTurbulence.y * t.WindTurbulencePower, targetWind.z * lengthRatio + finalAddTurbulence.z * t.WindTurbulencePower);
            else
                t.WindEffect = new Vector3(targetWind.x * lengthRatio, targetWind.y * lengthRatio, targetWind.z * lengthRatio);
        }

        private void Start()
        {
            int numCount = 10;

            randNumbers = new float[numCount];
            randTimes = new float[numCount];
            randSpeeds = new float[numCount];

            for (int i = 0; i < 10; i++)
            {
                randNumbers[i] = Random.Range(-1000f, 1000f);
                randTimes[i] = Random.Range(-1000f, 1000f);
                randSpeeds[i] = Random.Range(0.18f, 0.7f);
            }
        }


        void ComputeWind()
        {
            Vector3 newWind;

            if (SyncWithUnityWindZone)
            {
                worldTurbulenceTime += Time.deltaTime * worldTurbSpeed * SyncWithUnityWindZone.windTurbulence * UnityWindZoneTurbMul;
            }
            else
            {
                worldTurbulenceTime += Time.deltaTime * worldTurbSpeed;
            }

            if (overrideWind != Vector3.zero) 
            {
                newWind = overrideWind;
            }
            else if (SyncWithUnityWindZone)
            {
                newWind = SyncWithUnityWindZone.transform.forward * SyncWithUnityWindZone.windMain * UnityWindZonePowerMul;
                transform.rotation = SyncWithUnityWindZone.transform.rotation;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    randTimes[i] += Time.deltaTime * randSpeeds[i] * turbulenceSpeed * directionChangeSpeed;

                Quaternion windDir = windOrientation;

                float x = -1f + Mathf.PerlinNoise(randTimes[0], 256f + randTimes[1]) * 2f;
                float y = -1f + Mathf.PerlinNoise(-randTimes[1], 55f + randTimes[2]) * 2f;
                float z = -1f + Mathf.PerlinNoise(-randTimes[3], 55f + randTimes[0]) * 2f;
                windDir *= Quaternion.Euler(new Vector3(0, y, 0) * changesPower);
                windDir = Quaternion.Euler(x * (changesPower / 6f), windDir.eulerAngles.y, z * (changesPower / 6f));

                smoothWindOrient = FEngineering.SmoothDampRotation(smoothWindOrient, windDir, ref smoothWindOrientHelper, 1f - rapidness, Time.deltaTime);

                transform.rotation = smoothWindOrient;
                newWind = smoothWindOrient * Vector3.forward;
            }

            // Additional turbulence
            smoothAddTurbulence = Vector3.SmoothDamp(smoothAddTurbulence, GetAddTurbulence() * additionalTurbulence, ref addTurbHelper, 0.05f, Mathf.Infinity, Time.deltaTime);

            // Smooth out
            smoothWind = Vector3.SmoothDamp(smoothWind, newWind, ref windVeloHelper, 0.1f, Mathf.Infinity, Time.deltaTime);

            for (int i = 7; i < 10; i++)
                randTimes[i] += Time.deltaTime * randSpeeds[i] * turbulenceSpeed;

            float turbulencedPower = power * 0.015f;
            turbulencedPower *= 0.5f + Mathf.PerlinNoise(randTimes[7] * 2f, 25 + randTimes[8] * 0.5f);

            finalAddTurbulence = smoothAddTurbulence * turbulencedPower;
            targetWind = smoothWind * turbulencedPower;
        }

        private Vector3 GetAddTurbulence()
        {
            float turb = additionalTurbSpeed;
            if (SyncWithUnityWindZone) turb *= (SyncWithUnityWindZone.windTurbulence * UnityWindZoneTurbMul);

            for (int i = 4; i < 7; i++)
                randTimes[i] += Time.deltaTime * randSpeeds[i] * turb;

            float x = -1f + Mathf.PerlinNoise(randTimes[4] + 7.123f, -2.324f + Time.time * 0.24f) * 2f;
            float y = -1f + Mathf.PerlinNoise(randTimes[5] - 4.7523f, -25.324f + Time.time * 0.54f) * 2f;
            float z = -1f + Mathf.PerlinNoise(randTimes[6] + 1.123f, -63.324f + Time.time * -0.49f) * 2f;
            return new Vector3(x, y, z);
        }


        #region Turbo Tail Animator Jobs
#if FIMPOSSIBLE_JOBSANDBURST_READY

        /// <summary> Capture Unity objects once. Turbo Tail Animators evaluate their individual wind inside Burst jobs </summary>
        internal static TurboTailWindSnapshot CaptureTurboWindSnapshot()
        {
            TailAnimatorWind wind = Instance;

            if (wind == null || !wind.isActiveAndEnabled) return default(TurboTailWindSnapshot);

            return new TurboTailWindSnapshot
            {
                Enabled = 1,
                TargetWind = wind.targetWind,
                AdditionalTurbulence = wind.finalAddTurbulence,
                PowerDependsOnTailLength = wind.powerDependsOnTailLength ? 1 : 0,
                WorldTurbulence = wind.worldTurb,
                WorldTurbulenceScale = wind.worldTurbScale,
                WorldTurbulenceTime = wind.worldTurbulenceTime
            };
        }

        /// <summary> Value-only snapshot: no MonoBehaviour, Transform, WindZone or Time access in jobs </summary>
        internal struct TurboTailWindSnapshot
        {
            public int Enabled;
            public float3 TargetWind;
            public float3 AdditionalTurbulence;
            public int PowerDependsOnTailLength;
            public float WorldTurbulence;
            public float WorldTurbulenceScale;
            public float WorldTurbulenceTime;

            public float3 AffectTailWithWind(TurboTailForceInput tail, int boneCount, float3 position, float rootScaleZ)
            {
                if (Enabled == 0 || tail.UseWind == 0 || tail.WindEffectPower <= 0f || boneCount <= 0)
                    return float3.zero;

                float lengthRatio = 1f;
                if (PowerDependsOnTailLength != 0)
                {
                    lengthRatio = tail.TailLength * rootScaleZ / 5f;
                    if (boneCount > 3) lengthRatio *= math.lerp(0.7f, 3f, math.saturate(boneCount / 14f));
                }

                if (tail.WindWorldNoisePower > 0f)
                {
                    float worldNoise = (0.5f + math.sin(WorldTurbulenceTime + position.x * WorldTurbulenceScale) / 2f)
                        + (0.5f + math.cos(WorldTurbulenceTime + position.z * WorldTurbulenceScale) / 2f);
                    lengthRatio += worldNoise * WorldTurbulence * tail.WindWorldNoisePower;
                }

                float3 effect = TargetWind * (lengthRatio * tail.WindEffectPower);
                if (tail.WindTurbulencePower > 0f) effect += AdditionalTurbulence * tail.WindTurbulencePower;
                return effect;
            }
        }

#endif
        #endregion

    }

}
