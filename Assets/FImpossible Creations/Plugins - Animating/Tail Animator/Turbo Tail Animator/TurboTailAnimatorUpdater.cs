#if FIMPOSSIBLE_JOBSANDBURST_READY
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace FIMSpace.FTail
{
    /// <summary> Global owner of one restore/read/simulate/write job chain for all turbo tails </summary>
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-4)]
    public sealed class TurboTailAnimatorUpdater : MonoBehaviour
    {
        static TurboTailAnimatorUpdater instance;
        static Transform distanceCullingReference;
        
        readonly List<TurboTailAnimator> tails = new List<TurboTailAnimator>();
        readonly List<TurboTailAnimator> builtTails = new List<TurboTailAnimator>();

        readonly Dictionary<TurboTailAnimator, int> builtTailIndices = new Dictionary<TurboTailAnimator, int>();
        
        readonly List<Collider> collisionSources = new List<Collider>();

        readonly HashSet<TurboTailAnimator> parameterDirty = new HashSet<TurboTailAnimator>();

        TransformAccessArray readTransforms;
        TransformAccessArray writeTransforms;

        NativeArray<TurboTailRootState> rootStates;

        NativeArray<TurboTransformPose> poses;
        NativeArray<TurboTailDescriptor> descriptors;

        NativeArray<TurboTailRuntimeState> runtimeStates;
        NativeArray<TurboTailForceInput> forceInputs;

        NativeArray<TurboTailSegmentInput> inputs;
        NativeArray<TurboTailSegmentState> states;

        NativeArray<TurboColliderData> collisionData;
        NativeArray<TurboCapsuleAlongContactPlane> capsuleAlongContactPlanes;

        NativeArray<float3> outputPositions;
        NativeArray<quaternion> outputRotations;

        NativeArray<float3> restoreLocalPositions;
        NativeArray<quaternion> restoreLocalRotations;

        NativeArray<float3> bindRootPositions;
        NativeArray<quaternion> bindRootRotations;
        NativeArray<float3> bindRootScales;

        NativeArray<int> writeTailIndices;
        NativeArray<byte> activeTails;
        NativeArray<byte> restoreInFixedUpdate;
        
        bool structureDirty = true;
        bool hasFixedUpdateTails;
        bool hasCalibratedTails;
        bool hasUncalibratedTails;
        bool hasCalibratedRotationOnlyTails;

        #region Performance Measurement

#if UNITY_EDITOR
        public static FDebug_PerformanceTest GetPerformanceClock_Update
        {
            get
            {
                if (instance) return instance._PerfClock;
                else return null;
            }
        }

        private FDebug_PerformanceTest _PerfClock = new FDebug_PerformanceTest();

        private void PerformanceMeasure_Update_Start() { _PerfClock.Start(null, false); }
        private void PerformanceMeasure_Update_Pause() { _PerfClock.Pause(); }
        private void PerformanceMeasure_Update_Continue() { _PerfClock.Continue(); }
        private void PerformanceMeasure_Update_End() { _PerfClock.Finish(false); }
#endif

        #endregion


        /// <summary>
        /// Overrides the transform used for distance culling : set null to use Camera.main
        /// </summary>
        public static void SetDistanceCullingReference( Transform camera )
        {
            distanceCullingReference = camera;
        }

        public static int GetTurboTailsCount
        {
            get
            {
                if (instance) 
                    return instance.activeTails.Length;
                else 
                    return 0;
            }
        }

        static TurboTailAnimatorUpdater GetInstance()
        {
            if (instance != null) return instance;

            GameObject managerObject = new GameObject("Turbo Tails Updater");
            instance = managerObject.AddComponent<TurboTailAnimatorUpdater>();

            return instance;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void OnReload() { instance = null; }

        void Awake()
        {
            if (instance != null && instance != this)
            { 
                Destroy(gameObject);
                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            hideFlags = HideFlags.HideAndDontSave;
        }

        void Update()
        {
            #region Performance Measure (Editor Only)
#if UNITY_EDITOR
            PerformanceMeasure_Update_Start();
#endif
            #endregion

            RemoveInvalidTails();
            PrepareBuffers();
            ApplyParameterChanges();
            ScheduleRestore(0);

            #region Performance Measure (Editor Only)
#if UNITY_EDITOR
            PerformanceMeasure_Update_Pause();
#endif
            #endregion
        }

        void FixedUpdate()
        {
            if (!hasFixedUpdateTails && !structureDirty) return;
            
            RemoveInvalidTails();
            PrepareBuffers();
            ApplyParameterChanges();
        
            if (!hasFixedUpdateTails) return;

            ScheduleRestore(1);
        }

        void ScheduleRestore(byte fixedUpdatePhase)
        {
            if (!writeTransforms.isCreated || writeTransforms.length == 0) return;
            if (!hasCalibratedTails) return;

            RefreshActiveTails();

            if (hasUncalibratedTails || hasCalibratedRotationOnlyTails)
            {
                new TurboRestoreConditionalTransformsJob
                {
                    LocalPositions = restoreLocalPositions,
                    LocalRotations = restoreLocalRotations,
                    TailIndices = writeTailIndices,
                    ActiveTails = activeTails,
                    RestoreInFixedUpdate = restoreInFixedUpdate,
                    Descriptors = descriptors,
                    FixedUpdatePhase = fixedUpdatePhase
                }
                .Schedule(writeTransforms).Complete();
            }
            else
            {
                new TurboRestoreTransformsJob
                {
                    LocalPositions = restoreLocalPositions,
                    LocalRotations = restoreLocalRotations,
                    TailIndices = writeTailIndices,
                    ActiveTails = activeTails,
                    RestoreInFixedUpdate = restoreInFixedUpdate,
                    FixedUpdatePhase = fixedUpdatePhase
                }
                .Schedule(writeTransforms).Complete();
            }
        }

        void LateUpdate()
        {
            #region Performance Measure (Editor Only)
#if UNITY_EDITOR
            PerformanceMeasure_Update_Continue();
#endif
            #endregion

            RemoveInvalidTails();
            PrepareBuffers();
            ApplyParameterChanges();
            
            int tailCount = builtTails.Count;

            if (tailCount == 0 || !readTransforms.isCreated || readTransforms.length == 0) return;

            RefreshActiveTails(true);
            RefreshCollisionData();
            
            JobHandle readHandle = new TurboReadTransformsJob { Poses = poses }.Schedule(readTransforms);
            
            JobHandle simulationHandle = new TurboTailsSimulationJob
            {
                FrameDeltaTime = math.max(0f, Time.deltaTime),
                UnscaledDeltaTime = math.max(0f, Time.unscaledDeltaTime),
                FixedDeltaTime = math.max(0f, Time.fixedDeltaTime),

                Wind = TailAnimatorWind.CaptureTurboWindSnapshot(),
                ForceInputs = forceInputs,

                Poses = poses,
                Descriptors = descriptors,
                ActiveTails = activeTails,
                RuntimeStates = runtimeStates,
                Inputs = inputs,
                States = states,
                RootStates = rootStates,

                CollisionData = collisionData,
                CapsuleAlongContactPlanes = capsuleAlongContactPlanes,

                BindLocalPositions = restoreLocalPositions,
                BindLocalRotations = restoreLocalRotations,
                BindRootPositions = bindRootPositions,
                BindRootRotations = bindRootRotations,
                BindRootScales = bindRootScales,

                OutputPositions = outputPositions,
                OutputRotations = outputRotations
            }
            .Schedule(tailCount, 1, readHandle);
            

            JobHandle writeHandle = new TurboWriteTransformsJob
            {
                TailIndices = writeTailIndices,
                ActiveTails = activeTails,
                Descriptors = descriptors,
                Positions = outputPositions,
                Rotations = outputRotations
            }
            .Schedule(writeTransforms, simulationHandle);
            

            // One completion covers every registered tail and transform
            writeHandle.Complete();


            #region Performance Measure (Editor Only)
#if UNITY_EDITOR
            PerformanceMeasure_Update_End();
#endif
            #endregion
        }

        void OnDestroy()
        {
            DisposeBuffers();

            tails.Clear();
            builtTails.Clear();
            collisionSources.Clear();
            parameterDirty.Clear();

            if (instance == this) instance = null;
        }

        internal static void AddToUpdate(TurboTailAnimator tail)
        {
            if (tail == null) return;

            TurboTailAnimatorUpdater updater = GetInstance();
            if (updater.tails.Contains(tail)) return;

            updater.tails.Add(tail);

            updater.structureDirty = true;
        }

        internal static void RemoveFromUpdate(TurboTailAnimator tail)
        {
            if (instance == null || tail == null) return;

            if (instance.tails.Remove(tail)) instance.structureDirty = true;

            instance.builtTailIndices.Remove(tail);
            instance.parameterDirty.Remove(tail);
        }

        internal static Vector3 GetWindEffect(TurboTailAnimator tail)
        {
            if (instance == null || tail == null || !instance.runtimeStates.IsCreated) return Vector3.zero;

            int tailIndex;
            if (!instance.builtTailIndices.TryGetValue(tail, out tailIndex)) return Vector3.zero;

            // LateUpdate completes the entire job chain before returning to other main-thread callers
            return instance.runtimeStates[tailIndex].WindEffect;
        }

        internal static void MarkParametersDirty(TurboTailAnimator tail)
        {
            if (instance == null || tail == null) return;

            instance.parameterDirty.Add(tail);

            int tailIndex;
            if (!instance.builtTailIndices.TryGetValue(tail, out tailIndex) || !instance.descriptors.IsCreated) return;

            int colliderCount;
            if (tail.IncludedColliders == null) colliderCount = 0;
            else colliderCount = tail.IncludedColliders.Count;

            if (instance.descriptors[tailIndex].ColliderCount != colliderCount) instance.structureDirty = true;
        }

        void RemoveInvalidTails()
        {
            for (int i = tails.Count - 1; i >= 0; i--)
            {
                if (tails[i] == null)
                {
                    tails.RemoveAt(i);
                    structureDirty = true;
                }
            }
        }

        void PrepareBuffers()
        {
            if (!structureDirty) return;
            
            RebuildBuffers();
            structureDirty = false;
            parameterDirty.Clear();
        }

        void RebuildBuffers()
        {
            DisposeBuffers();
            builtTails.Clear();
            collisionSources.Clear();
            hasCalibratedTails = false;
            hasUncalibratedTails = false;
            hasCalibratedRotationOnlyTails = false;
            
            HashSet<Transform> claimedBones = new HashSet<Transform>();
            
            for (int i = 0; i < tails.Count; i++)
            {
                TurboTailAnimator tail = tails[i];
                
                if (tail == null || !tail.IsInitialized || tail.TailSegments.Count == 0) continue;
                
                bool valid = true;
                
                for (int b = 0; b < tail.TailSegments.Count; b++)
                {
                    Transform bone = tail.TailSegments[b].transform;
                    if (bone == null || claimedBones.Contains(bone)) { valid = false; break; }
                }
                
                if (!valid)
                {
                    Debug.LogWarning("[Turbo Tail Animator] Skipping invalid or overlapping chain on " + tail.name + ".", tail);
                    continue;
                }

                for (int b = 0; b < tail.TailSegments.Count; b++) claimedBones.Add(tail.TailSegments[b].transform);
                
                builtTailIndices.Add(tail, builtTails.Count);
                builtTails.Add(tail);
            }

            int tailCount = builtTails.Count;
            if (tailCount == 0) return;
            
            int boneCount = 0;
            int stateCount = 0;
            int colliderCount = 0;

            for (int i = 0; i < tailCount; i++)
            { 
                boneCount += builtTails[i].TailSegments.Count;
                stateCount += builtTails[i].TailSegments.Count + 1;

                if (builtTails[i].IncludedColliders != null) colliderCount += builtTails[i].IncludedColliders.Count;
            }

            List<Transform> readList = new List<Transform>(boneCount + tailCount * 2);
            Dictionary<Transform, int> readIndices = new Dictionary<Transform, int>();
            List<Transform> writeList = new List<Transform>(boneCount);
            
            descriptors = new NativeArray<TurboTailDescriptor>(tailCount, Allocator.Persistent);
            runtimeStates = new NativeArray<TurboTailRuntimeState>(tailCount, Allocator.Persistent);

            forceInputs = new NativeArray<TurboTailForceInput>(tailCount, Allocator.Persistent);
            rootStates = new NativeArray<TurboTailRootState>(tailCount, Allocator.Persistent);
            activeTails = new NativeArray<byte>(tailCount, Allocator.Persistent);

            restoreInFixedUpdate = new NativeArray<byte>(tailCount, Allocator.Persistent);

            inputs = new NativeArray<TurboTailSegmentInput>(stateCount, Allocator.Persistent);
            states = new NativeArray<TurboTailSegmentState>(stateCount, Allocator.Persistent);

            // Keep the job field valid even when no registered tail currently uses collisions
            collisionData = new NativeArray<TurboColliderData>(math.max(1, colliderCount), Allocator.Persistent);
            
            // One supporting plane per included collider is sufficient for a single tail side
            // Each tail owns its collider range, so parallel simulations never share scratch slots
            capsuleAlongContactPlanes = new NativeArray<TurboCapsuleAlongContactPlane>(math.max(1, colliderCount), Allocator.Persistent);
            
            outputPositions = new NativeArray<float3>(boneCount, Allocator.Persistent);
            outputRotations = new NativeArray<quaternion>(boneCount, Allocator.Persistent);
            
            restoreLocalPositions = new NativeArray<float3>(boneCount, Allocator.Persistent);
            restoreLocalRotations = new NativeArray<quaternion>(boneCount, Allocator.Persistent);

            bindRootPositions = new NativeArray<float3>(boneCount, Allocator.Persistent);
            bindRootRotations = new NativeArray<quaternion>(boneCount, Allocator.Persistent);
            bindRootScales = new NativeArray<float3>(boneCount, Allocator.Persistent);

            writeTailIndices = new NativeArray<int>(boneCount, Allocator.Persistent);

            int boneStart = 0;
            int stateStart = 0;
            int colliderStart = 0;

            for (int tailIndex = 0; tailIndex < tailCount; tailIndex++)
            {
                TurboTailAnimator tail = builtTails[tailIndex];
                
                int count = tail.TailSegments.Count;

                TurboTailAnimator.TailSegment ghostParent = tail.ManagerGhostParent;
                TurboTailAnimator.TailSegment ghostChild = tail.ManagerGhostChild;

                Transform parentTransform = ghostParent.transform;
                Transform safetyTransform;

                if (ghostParent.ParentBone != null) 
                    safetyTransform = ghostParent.ParentBone.transform;
                else 
                    safetyTransform = null;
                
                int parentIndex = AddReadTransform(parentTransform, readList, readIndices);
                int safetyIndex = AddReadTransform(safetyTransform, readList, readIndices);
            
                bool directChain = parentTransform != null && tail.TailSegments[0].transform.parent == parentTransform;

                for (int b = 0; b < count; b++)
                {
                    TurboTailAnimator.TailSegment segment = tail.TailSegments[b];
                    
                    int sourceIndex = AddReadTransform(segment.transform, readList, readIndices);
                    writeList.Add(segment.transform);
                    
                    if (b > 0 && segment.transform.parent != tail.TailSegments[b - 1].transform) directChain = false;
                    
                    int stateIndex = stateStart + b, outputIndex = boneStart + b;
                    
                    TurboTailSegmentInput input = inputs[stateIndex];
                    input.PositionSpeed = segment.PositionSpeed;
                    input.RotationSpeed = segment.RotationSpeed;

                    input.Slithery = segment.Slithery;
                    input.Curling = segment.Curling;
                    input.Springiness = segment.Springiness;

                    input.ColliderRadius = segment.ColliderRadius;
                    input.CollisionDamping = math.saturate(segment.CollisionDamping);
                    input.CollisionSlippery = segment.CollisionSlippery;
                    input.CollisionReflect = segment.CollisionReflect;

                    input.GravityWeight = segment.GravityWeight;

                    input.BlendValue = segment.BlendValue;

                    input.SourceTransformIndex = sourceIndex;
                    inputs[stateIndex] = input;
                    
                    states[stateIndex] = TurboTailAnimator.BuildInitialState(segment);

                    outputPositions[outputIndex] = segment.ProceduralPositionWeightBlended;
                    outputRotations[outputIndex] = segment.TrueTargetRotation;

                    restoreLocalPositions[outputIndex] = segment.InitialLocalPosition;
                    restoreLocalRotations[outputIndex] = segment.InitialLocalRotation;

                    bindRootPositions[outputIndex] = segment.InitialLocalPositionInRoot;
                    bindRootRotations[outputIndex] = segment.InitialLocalRotationInRoot;
                    bindRootScales[outputIndex] = segment.InitialScaleInRoot;

                    writeTailIndices[outputIndex] = tailIndex;
                }

                TurboTailSegmentInput ghostInput = inputs[stateStart + count];
                ghostInput.PositionSpeed = ghostChild.PositionSpeed; 
                ghostInput.RotationSpeed = ghostChild.RotationSpeed;

                ghostInput.Slithery = ghostChild.Slithery; 
                ghostInput.Curling = ghostChild.Curling;
                ghostInput.Springiness = ghostChild.Springiness;

                ghostInput.ColliderRadius = 0f;
                ghostInput.CollisionDamping = math.saturate(ghostChild.CollisionDamping);
                ghostInput.CollisionSlippery = ghostChild.CollisionSlippery;
                ghostInput.CollisionReflect = ghostChild.CollisionReflect;

                ghostInput.GravityWeight = ghostChild.GravityWeight;

                ghostInput.BlendValue = ghostChild.BlendValue;

                ghostInput.SourceTransformIndex = -1; inputs[stateStart + count] = ghostInput;

                states[stateStart + count] = TurboTailAnimator.BuildInitialState(ghostChild);
                rootStates[tailIndex] = tail.BuildInitialRootState();

                float initialWave;
                if (tail.FixedCycle != 0f) 
                    initialWave = tail.FixedCycle;
                else 
                    initialWave = math.lerp(-math.PI * 100f, math.PI * 100f, Hash01(tail.GetInstanceID()));

                float initialCosine;
                if (tail.FixedCycle != 0f) 
                    initialCosine = tail.FixedCycle;
                else 
                    initialCosine = initialWave * 0.5f;
                
                runtimeStates[tailIndex] = new TurboTailRuntimeState
                { 
                    WaveTime = initialWave, 
                    CosTime = initialCosine
                };

                int tailColliderCount;

                if (tail.IncludedColliders == null) 
                    tailColliderCount = 0;
                else 
                    tailColliderCount = tail.IncludedColliders.Count;

                for (int c = 0; c < tailColliderCount; c++) collisionSources.Add(tail.IncludedColliders[c]);

                TurboTailDescriptor descriptor = BuildDescriptor(tail, boneStart, stateStart, count, colliderStart, tailColliderCount, parentIndex, safetyIndex, parentTransform == tail.TailSegments[0].transform, directChain);
                descriptors[tailIndex] = descriptor;
                TrackRestoreMode(descriptor);
                forceInputs[tailIndex] = BuildForceInput(tail);
                
                UpdateRestorePhase(tailIndex, tail);
                
                boneStart += count;
                stateStart += count + 1;
                colliderStart += tailColliderCount;
            }

            readTransforms = new TransformAccessArray(readList.ToArray());
            writeTransforms = new TransformAccessArray(writeList.ToArray());

            poses = new NativeArray<TurboTransformPose>(readList.Count, Allocator.Persistent);
        }

        void ApplyParameterChanges()
        {
            if (parameterDirty.Count == 0 || !descriptors.IsCreated) return;
            
            foreach (TurboTailAnimator tail in parameterDirty)
            {
                int tailIndex;
                if (!builtTailIndices.TryGetValue(tail, out tailIndex)) continue;
            
                TurboTailDescriptor old = descriptors[tailIndex];
                
                descriptors[tailIndex] = BuildDescriptor(tail, old.BoneStart, old.StateStart, old.BoneCount, old.ColliderStart, old.ColliderCount, old.ParentTransformIndex, old.SafetyTransformIndex, old.RootUsesVirtualParent != 0, old.DirectChain != 0);
                forceInputs[tailIndex] = BuildForceInput(tail);

                for (int c = 0; c < old.ColliderCount; c++)
                    collisionSources[old.ColliderStart + c] = tail.IncludedColliders[c];
                
                UpdateRestorePhase(tailIndex, tail);
                bool isDisablingCollisions = old.UseCollision != 0 && descriptors[tailIndex].UseCollision == 0;
            
                for (int b = 0; b <= old.BoneCount; b++)
                {
                    TurboTailAnimator.TailSegment segment;

                    if (b < old.BoneCount) 
                        segment = tail.TailSegments[b];
                    else 
                        segment = tail.ManagerGhostChild;

                    int index = old.StateStart + b;
                    
                    TurboTailSegmentInput input = inputs[index];

                    input.PositionSpeed = segment.PositionSpeed;
                    input.RotationSpeed = segment.RotationSpeed;

                    input.Slithery = segment.Slithery;
                    input.Curling = segment.Curling;
                    input.Springiness = segment.Springiness;

                    if (b < old.BoneCount) 
                        input.ColliderRadius = segment.ColliderRadius;
                    else 
                        input.ColliderRadius = 0f;

                    input.CollisionDamping = math.saturate(segment.CollisionDamping);
                    input.CollisionSlippery = segment.CollisionSlippery;
                    input.CollisionReflect = segment.CollisionReflect;

                    input.GravityWeight = segment.GravityWeight;
                    input.BlendValue = segment.BlendValue;

                    inputs[index] = input;

                    if ( isDisablingCollisions)
                    {
                        TurboTailSegmentState state = states[index];
                        TurboTailSimulation.ClearCollisionState(ref state);
                        states[index] = state;
                    }
                }
            }

            parameterDirty.Clear();
            
            RefreshFixedUpdateFlag();
            RefreshRestoreModes();
        }

        void UpdateRestorePhase(int tailIndex, TurboTailAnimator tail)
        {
            restoreInFixedUpdate[tailIndex] = tail.AnimatePhysics && tail.Calibrate ? (byte)1 : (byte)0;
            
            if (restoreInFixedUpdate[tailIndex] != 0) hasFixedUpdateTails = true;
        }

        void RefreshFixedUpdateFlag()
        {
            hasFixedUpdateTails = false;

            for (int i = 0; i < restoreInFixedUpdate.Length; i++)
            {
                if (restoreInFixedUpdate[i] != 0) 
                { 
                    hasFixedUpdateTails = true;
                    break;
                }
            }
        }

        void RefreshRestoreModes()
        {
            hasCalibratedTails = false;
            hasUncalibratedTails = false;
            hasCalibratedRotationOnlyTails = false;

            for (int i = 0; i < descriptors.Length; i++)
            {
                TrackRestoreMode(descriptors[i]);
            }
        }

        void TrackRestoreMode(TurboTailDescriptor descriptor)
        {
            if (descriptor.Calibrate == 0)
            {
                hasUncalibratedTails = true;
                return;
            }

            hasCalibratedTails = true;
            if (descriptor.WritePositions == 0) hasCalibratedRotationOnlyTails = true;
        }

        void RefreshActiveTails(bool advanceDistanceBlend = false)
        {
            Transform cullingReference = null;
            bool cameraChecked = false;

            Vector3 cameraPosition = Vector3.zero;
            
            for (int i = 0; i < builtTails.Count; i++)
            {
                TurboTailAnimator tail = builtTails[i];
                
                bool active = tail != null && tail.isActiveAndEnabled && tail.IsInitialized;
                
                if (active && tail.OptimizeWithMeshes != null && tail.OptimizeWithMeshes.Length > 0)
                {
                    Renderer[] renderers = tail.OptimizeWithMeshes;
                    bool hasRenderer = false;
                    bool visible = false;
                    
                    for (int r = 0; r < renderers.Length && !visible; r++)
                    {
                        if (renderers[r] == null) continue;
                        hasRenderer = true;
                        visible = renderers[r].enabled && renderers[r].isVisible;
                    }

                    active = !hasRenderer || visible;
                }

                bool inDistanceRange = true;

                if (active && tail.CullAtDistance > 0f)
                {
                    if (!cameraChecked)
                    {
                        cullingReference = distanceCullingReference;

                        if (cullingReference == null)
                        {
                            Camera mainCamera = Camera.main;
                            if (mainCamera != null) cullingReference = mainCamera.transform;
                        }

                        if (cullingReference != null) cameraPosition = cullingReference.position;
                        cameraChecked = true;
                    }

                    if (cullingReference != null)
                    {
                        float maxDistanceSq = tail.CullAtDistance * tail.CullAtDistance;
                        inDistanceRange = (cameraPosition - tail.BaseTransform.position).sqrMagnitude <= maxDistanceSq;
                    }
                }

                if (active)
                {
                    float previousBlend = tail.DistanceCullingBlend;

                    // Advance only in LateUpdate, never once per restore / fixed-update call
                    if (advanceDistanceBlend)
                    {
                        float target = inDistanceRange ? 1f : 0f;
                        
                        if (tail.DistanceCullingTransitionDuration <= 0f)
                            tail.DistanceCullingBlend = target;
                        else
                            tail.DistanceCullingBlend = Mathf.MoveTowards(previousBlend, target, Mathf.Max(0f, Time.unscaledDeltaTime) / tail.DistanceCullingTransitionDuration);
                    }

                    active = inDistanceRange || previousBlend > 0f;

                    TurboTailDescriptor descriptor = descriptors[i];
                    descriptor.DistanceCullingBlend = tail.DistanceCullingBlend;
                    descriptors[i] = descriptor;
                }

                if (active) activeTails[i] = 1; else activeTails[i] = 0;
            }
        }

        void RefreshCollisionData()
        {
            if (!collisionData.IsCreated) return;

            for (int tailIndex = 0; tailIndex < builtTails.Count; tailIndex++)
            {
                TurboTailAnimator tail = builtTails[tailIndex];
                TurboTailDescriptor descriptor = descriptors[tailIndex];

                for (int localIndex = 0; localIndex < descriptor.ColliderCount; localIndex++)
                {
                    int i = descriptor.ColliderStart + localIndex;
                    Collider collider = collisionSources[i];

                    TurboColliderData data = default(TurboColliderData);

                    if (activeTails[tailIndex] != 0 && descriptor.UseCollision != 0 && collider != null && collider.gameObject.activeInHierarchy && (tail.CollideWithDisabledColliders || collider.enabled))
                    {
                        SphereCollider sphere = collider as SphereCollider;
                        if (sphere != null)
                        {
                            Matrix4x4 matrix = sphere.transform.localToWorldMatrix;

                            float3 scale = GetWorldAxisScale( matrix );
                            float radiusScale = math.max( math.abs( scale.x ), math.abs( scale.y ) );
                            radiusScale = math.max( radiusScale, math.abs( scale.z ) );
                            
                            data.Type = (int)TurboColliderType.Sphere;
                            data.PointA = matrix.MultiplyPoint3x4( sphere.center );
                            data.PointB = data.PointA;

                            data.Radius = math.abs( sphere.radius ) * radiusScale;
                        }
                        else
                        {
                            CapsuleCollider capsule = collider as CapsuleCollider;

                            if (capsule != null)
                            {
                                CaptureCapsule(capsule.transform, capsule.center, capsule.radius, capsule.height, capsule.direction, ref data);
                            }
                            else
                            {
                                CharacterController controller = collider as CharacterController;

                                if (controller != null)
                                    CaptureCapsule(controller.transform, controller.center, controller.radius, controller.height, 1, ref data);
                                else
                                {
                                    BoxCollider box = collider as BoxCollider;

                                    if (box != null)
                                        CaptureBox(box, ref data);
                                }
                            }
                        }
                    }

                    collisionData[i] = data;
                }
            }
        }

        static void CaptureCapsule(Transform transform, Vector3 localCenter, float localRadius, float localHeight, int direction, ref TurboColliderData data)
        {
            Matrix4x4 matrix = transform.localToWorldMatrix;
            float3 scale = GetWorldAxisScale( matrix );
            float axisScale;
            float radiusScale;
            float3 localAxis;

            if( direction == 0 )
            {
                localAxis = Vector3.right;
                axisScale = math.abs( scale.x );
                radiusScale = math.max( math.abs( scale.y ), math.abs( scale.z ) );
            }
            else if( direction == 2 )
            {
                localAxis = Vector3.forward;
                axisScale = math.abs( scale.z );
                radiusScale = math.max( math.abs( scale.x ), math.abs( scale.y ) );
            }
            else
            {
                localAxis = Vector3.up;
                axisScale = math.abs( scale.y );
                radiusScale = math.max( math.abs( scale.x ), math.abs( scale.z ) );
            }

            float radius = math.abs( localRadius ) * radiusScale;
            float halfLine = math.max( 0f, math.abs( localHeight ) * axisScale * .5f - radius );

            float3 center = matrix.MultiplyPoint3x4( localCenter );
            float3 axis = matrix.rotation * localAxis;

            data.Type = (int)TurboColliderType.Capsule;

            data.PointA = center - axis * halfLine;
            data.PointB = center + axis * halfLine;
            data.Radius = radius;
        }

        static void CaptureBox(BoxCollider box, ref TurboColliderData data)
        {
            Matrix4x4 matrix = box.transform.localToWorldMatrix;
            float3 scale = GetWorldAxisScale( matrix );
            float3 size = box.size;
            Quaternion rotation = matrix.rotation;

            data.Type = (int)TurboColliderType.Box;
            data.PointA = matrix.MultiplyPoint3x4( box.center );

            data.BoxHalfExtents = math.abs( size ) * math.abs( scale ) * .5f;

            data.BoxRotation = new quaternion( rotation.x, rotation.y, rotation.z, rotation.w );

            data.BoxInverseRotation = math.inverse( data.BoxRotation );

            float3 axisX = math.abs( math.mul( data.BoxRotation, new float3( 1f, 0f, 0f ) ) ) * data.BoxHalfExtents.x;
            float3 axisY = math.abs( math.mul( data.BoxRotation, new float3( 0f, 1f, 0f ) ) ) * data.BoxHalfExtents.y;
            float3 axisZ = math.abs( math.mul( data.BoxRotation, new float3( 0f, 0f, 1f ) ) ) * data.BoxHalfExtents.z;

            data.BoxBoundsExtents = axisX + axisY + axisZ;
        }

        static float3 GetWorldAxisScale( Matrix4x4 matrix )
        {
            return new float3
            (
                math.sqrt( matrix.m00 * matrix.m00 + matrix.m10 * matrix.m10 + matrix.m20 * matrix.m20 ),
                math.sqrt( matrix.m01 * matrix.m01 + matrix.m11 * matrix.m11 + matrix.m21 * matrix.m21 ),
                math.sqrt( matrix.m02 * matrix.m02 + matrix.m12 * matrix.m12 + matrix.m22 * matrix.m22 )
            );
        }

        static TurboTailForceInput BuildForceInput(TurboTailAnimator tail)
        {
            int useWind;
            if (tail.UseWind) useWind = 1; else useWind = 0;

            return new TurboTailForceInput
            {
                // SimpleTailAnimator expresses gravity in units of 1/40
                Gravity = tail.Gravity / 40f,
                UseWind = useWind,
                TailLength = tail.TailLengthForWind,
                WindEffectPower = tail.WindEffectPower,
                WindTurbulencePower = tail.WindTurbulencePower,
                WindWorldNoisePower = tail.WindWorldNoisePower
            };
        }

        static TurboTailDescriptor BuildDescriptor(TurboTailAnimator tail, int boneStart, int stateStart, int boneCount, int colliderStart, int colliderCount, int parentIndex, int safetyIndex, bool virtualParent, bool directChain)
        {
            int rootUsesVirtualParent;
            if (virtualParent) rootUsesVirtualParent = 1;
            else rootUsesVirtualParent = 0;

            int isDirectChain;
            if (directChain) isDirectChain = 1;
            else isDirectChain = 0;

            bool detached = tail.ManagerChildrenDetached;
            bool animateDetachedChildren = detached && tail.DetachMode == TurboTailAnimator.EDetachChildrenMode.DetachForAnimatorBones;

            int writePositions;
            if (!detached && tail.PreferRotationOnly && directChain && !virtualParent) writePositions = 0;
            else writePositions = 1;

            int useCollision;
            if (tail.UseCollision) useCollision = 1;
            else useCollision = 0;

            int useWaving;
            if (tail.UseWaving) useWaving = 1;
            else useWaving = 0;

            int cosinusAdd;
            if (tail.CosinusAdd) cosinusAdd = 1;
            else cosinusAdd = 0;

            return new TurboTailDescriptor
            {
                BoneStart = boneStart,
                StateStart = stateStart,
                BoneCount = boneCount,

                ColliderStart = colliderStart,
                ColliderCount = colliderCount,

                ParentTransformIndex = parentIndex,
                SafetyTransformIndex = safetyIndex,

                RootUsesVirtualParent = rootUsesVirtualParent,
                DirectChain = isDirectChain,
                WritePositions = writePositions,
                Calibrate = tail.Calibrate ? 1 : 0,

                DetachChildren = detached ? 1 : 0,
                AnimateDetachedChildren = animateDetachedChildren ? 1 : 0,

                ParentLocalOffset = tail.ManagerGhostParent.LocalOffset,
                ArtificialChildLocalPosition = tail.ManagerGhostChild.InitialLocalPosition,

                ManagedDeltaTime = tail.UpdateRate > 0 ? 1f / tail.UpdateRate : 0f,
                DeltaType = tail.DeltaType,
                MaxSimulationSteps = math.max(1, tail.MaxSimulationStepsPerFrame),

                MotionInfluence = tail.MotionInfluence,
                MotionInfluenceInY = tail.MotionInfluenceInY,

                MaxStretching = tail.MaxStretching,
                ReactionSpeed = tail.ReactionSpeed,

                AngleLimit = tail.AngleLimit,
                AngleLimitAxis = tail.AngleLimitAxis,
                LimitAxisRange = tail.LimitAxisRange,
                LimitSmoothing = tail.LimitSmoothing,
                AnimateRoll = tail.AnimateRoll ? 1 : 0,

                UseCollision = useCollision,
                CollisionMode = (int)tail.CollisionMode,

                UseWaving = useWaving,
                WavingType = (int)tail.WavingType,
                CosinusAdd = cosinusAdd,
                WavingSpeed = tail.WavingSpeed,
                WavingRange = tail.WavingRange,
                WavingAxis = tail.WavingAxis,
                AlternateWave = tail.AlternateWave
            };
        }

        static int AddReadTransform(Transform transform, List<Transform> transforms, Dictionary<Transform, int> indices)
        {
            if (transform == null) return -1;
            
            int index; 
            if (indices.TryGetValue(transform, out index)) return index;
            
            index = transforms.Count;
            transforms.Add(transform);
            indices.Add(transform, index);
            
            return index;
        }

        static float Hash01(int value)
        {
            uint x = (uint)value; x ^= x >> 16;
            x *= 0x7feb352du; x ^= x >> 15;
            x *= 0x846ca68bu; x ^= x >> 16; 
            return (x & 0x00ffffffu) / 16777215f;
        }

        void DisposeBuffers()
        {
            builtTailIndices.Clear();
            if (readTransforms.isCreated) readTransforms.Dispose();
            if (writeTransforms.isCreated) writeTransforms.Dispose();
            if (poses.IsCreated) poses.Dispose(); 
            if (descriptors.IsCreated) descriptors.Dispose();
            if (runtimeStates.IsCreated) runtimeStates.Dispose(); 
            if (forceInputs.IsCreated) forceInputs.Dispose();
            if (inputs.IsCreated) inputs.Dispose();
            if (states.IsCreated) states.Dispose(); 
            if (rootStates.IsCreated) rootStates.Dispose();
            if (collisionData.IsCreated) collisionData.Dispose();
            if (capsuleAlongContactPlanes.IsCreated) capsuleAlongContactPlanes.Dispose();
            if (outputPositions.IsCreated) outputPositions.Dispose(); 
            if (outputRotations.IsCreated) outputRotations.Dispose();
            if (restoreLocalPositions.IsCreated) restoreLocalPositions.Dispose();
            if (restoreLocalRotations.IsCreated) restoreLocalRotations.Dispose();
            if (bindRootPositions.IsCreated) bindRootPositions.Dispose();
            if (bindRootRotations.IsCreated) bindRootRotations.Dispose();
            if (bindRootScales.IsCreated) bindRootScales.Dispose();
            if (writeTailIndices.IsCreated) writeTailIndices.Dispose(); 
            if (activeTails.IsCreated) activeTails.Dispose();
            if (restoreInFixedUpdate.IsCreated) restoreInFixedUpdate.Dispose();
            
            hasFixedUpdateTails = false;
        }
    }

    struct TurboTransformPose
    {
        public float3 Position; 
        public quaternion Rotation; 
        public float3 LocalPosition;
        public quaternion LocalRotation; 
        public float3 Scale;
    }

    struct TurboTailDescriptor
    {
        public int BoneStart, StateStart, BoneCount, ColliderStart, ColliderCount, ParentTransformIndex, SafetyTransformIndex;
        public int RootUsesVirtualParent, DirectChain, WritePositions, Calibrate, DetachChildren, AnimateDetachedChildren;
        public float3 ParentLocalOffset, ArtificialChildLocalPosition;
        public float ManagedDeltaTime; 
        public TurboTailAnimator.EFDeltaType DeltaType;
        public int MaxSimulationSteps;
        public float MotionInfluence, MotionInfluenceInY, MaxStretching, ReactionSpeed;
        public float AngleLimit, LimitSmoothing;
        public float3 AngleLimitAxis;
        public float2 LimitAxisRange;
        public int AnimateRoll;
        public int UseCollision, CollisionMode;
        public int UseWaving, WavingType, CosinusAdd;
        public float WavingSpeed, WavingRange; 
        public float3 WavingAxis; 
        public float AlternateWave;
        public float DistanceCullingBlend;
    }

    enum TurboColliderType
    {
        None = 0,
        Sphere = 1,
        Capsule = 2,
        Box = 3
    }

    struct TurboColliderData
    {
        public int Type;
        public float3 PointA;
        public float3 PointB;
        public float Radius;
        public float3 BoxHalfExtents;
        public float3 BoxBoundsExtents;
        public quaternion BoxRotation;
        public quaternion BoxInverseRotation;
    }

    struct TurboCapsuleAlongContactPlane
    {
        public float3 Normal;
        public float Distance;
    }

    struct TurboTailRuntimeState
    {
        public float Accumulator;
        public float WaveTime;
        public float CosTime;
        public int WasActive;
        public float3 WindEffect;
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
    struct TurboRestoreTransformsJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float3> LocalPositions;
        [ReadOnly] public NativeArray<quaternion> LocalRotations;

        [ReadOnly] public NativeArray<int> TailIndices;
        [ReadOnly] public NativeArray<byte> ActiveTails;
        [ReadOnly] public NativeArray<byte> RestoreInFixedUpdate;
        
        public byte FixedUpdatePhase;
        
        public void Execute(int index, TransformAccess transform)
        {
            int tailIndex = TailIndices[index];

            if (ActiveTails[tailIndex] == 0 || RestoreInFixedUpdate[tailIndex] != FixedUpdatePhase) return;

#if UNITY_2022_3_OR_NEWER
            transform.SetLocalPositionAndRotation(LocalPositions[index], LocalRotations[index]);
#else
            transform.localPosition = LocalPositions[index]; 
            transform.localRotation = LocalRotations[index];
#endif
        }
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
    struct TurboRestoreConditionalTransformsJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float3> LocalPositions;
        [ReadOnly] public NativeArray<quaternion> LocalRotations;

        [ReadOnly] public NativeArray<int> TailIndices;
        [ReadOnly] public NativeArray<byte> ActiveTails;
        [ReadOnly] public NativeArray<byte> RestoreInFixedUpdate;
        [ReadOnly] public NativeArray<TurboTailDescriptor> Descriptors;

        public byte FixedUpdatePhase;

        public void Execute(int index, TransformAccess transform)
        {
            int tailIndex = TailIndices[index];

            if (ActiveTails[tailIndex] == 0 || RestoreInFixedUpdate[tailIndex] != FixedUpdatePhase) return;

            TurboTailDescriptor descriptor = Descriptors[tailIndex];
            if (descriptor.Calibrate == 0) return;

            if (descriptor.WritePositions != 0 || index == descriptor.BoneStart)
            {
#if UNITY_2022_3_OR_NEWER
                transform.SetLocalPositionAndRotation(LocalPositions[index], LocalRotations[index]);
#else
                transform.localPosition = LocalPositions[index];
                transform.localRotation = LocalRotations[index];
#endif
            }
            else
            {
                transform.localRotation = LocalRotations[index];
            }
        }
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
    struct TurboReadTransformsJob : IJobParallelForTransform
    {
        [WriteOnly] public NativeArray<TurboTransformPose> Poses;

        public void Execute(int index, TransformAccess transform)
        {
            Matrix4x4 matrix = transform.localToWorldMatrix;

            Vector3 position;
            Quaternion rotation;
            Vector3 localPosition;
            Quaternion localRotation;

#if UNITY_2022_3_OR_NEWER
            transform.GetPositionAndRotation(out position, out rotation);
            transform.GetLocalPositionAndRotation(out localPosition, out localRotation);
#else
            position = transform.position;
            rotation = transform.rotation;
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
#endif
            
            float3 x = new float3(matrix.m00, matrix.m10, matrix.m20);
            float3 y = new float3(matrix.m01, matrix.m11, matrix.m21);
            float3 z = new float3(matrix.m02, matrix.m12, matrix.m22);
            
            Poses[index] = new TurboTransformPose
            {
                Position = position,
                Rotation = rotation,
                LocalPosition = localPosition,
                LocalRotation = localRotation,
                Scale = new float3(math.length(x), math.length(y), math.length(z))
            };
        }
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
    struct TurboTailsSimulationJob : IJobParallelFor
    {
        public float FrameDeltaTime;
        public float UnscaledDeltaTime;
        public float FixedDeltaTime;

        [ReadOnly] public NativeArray<byte> ActiveTails;

        [NativeDisableParallelForRestriction] public NativeArray<TurboTailRootState> RootStates;
        
        [ReadOnly] public NativeArray<TurboTailDescriptor> Descriptors;
        
        [ReadOnly] public NativeArray<TurboTransformPose> Poses;
        [NativeDisableParallelForRestriction] public NativeArray<TurboTailRuntimeState> RuntimeStates;
        
        [NativeDisableParallelForRestriction] public NativeArray<TurboTailSegmentInput> Inputs;
        [NativeDisableParallelForRestriction] public NativeArray<TurboTailSegmentState> States;

        [ReadOnly] public NativeArray<TurboColliderData> CollisionData;
        [NativeDisableParallelForRestriction] public NativeArray<TurboCapsuleAlongContactPlane> CapsuleAlongContactPlanes;

        [ReadOnly] public NativeArray<float3> BindLocalPositions;
        [ReadOnly] public NativeArray<quaternion> BindLocalRotations;
        [ReadOnly] public NativeArray<float3> BindRootPositions;
        [ReadOnly] public NativeArray<quaternion> BindRootRotations;
        [ReadOnly] public NativeArray<float3> BindRootScales;

        public TailAnimatorWind.TurboTailWindSnapshot Wind;
        [ReadOnly] public NativeArray<TurboTailForceInput> ForceInputs;

        [NativeDisableParallelForRestriction] public NativeArray<float3> OutputPositions;
        [NativeDisableParallelForRestriction] public NativeArray<quaternion> OutputRotations;

        public void Execute(int tailIndex)
        {
            TurboTailRuntimeState runtime = RuntimeStates[tailIndex];
            
            if (ActiveTails[tailIndex] == 0)
            { 
                runtime.WasActive = 0;
                runtime.Accumulator = 0f;
                runtime.WindEffect = float3.zero;
                RuntimeStates[tailIndex] = runtime;

                return;
            }

            TurboTailDescriptor descriptor = Descriptors[tailIndex];
            float deltaTime;

            switch (descriptor.DeltaType)
            {
                case TurboTailAnimator.EFDeltaType.UnscaledDeltaTime: deltaTime = UnscaledDeltaTime; break;
                case TurboTailAnimator.EFDeltaType.FixedDeltaTime: deltaTime = FixedDeltaTime; break;
                default: deltaTime = FrameDeltaTime; break;
            }

            quaternion wave = CalculateWave(descriptor, Inputs[descriptor.StateStart].BlendValue * descriptor.DistanceCullingBlend, deltaTime, ref runtime);
            
            FillInputs(descriptor, wave);

            if (descriptor.DistanceCullingBlend == 0f)
            {
                // Finish at the source pose even when this frame has no simulation tick
                ResetToSource(tailIndex, descriptor);

                runtime.WasActive = 0;
                runtime.Accumulator = 0f;
                runtime.WindEffect = float3.zero;
                RuntimeStates[tailIndex] = runtime;

                return;
            }
            
            if (runtime.WasActive == 0) 
            { 
                ResetToSource(tailIndex, descriptor);
                runtime.WasActive = 1;
            }

            TurboTailForceInput forces = ForceInputs[tailIndex];
            TurboTransformPose rootPose = Poses[Inputs[descriptor.StateStart].SourceTransformIndex];

            runtime.WindEffect = Wind.AffectTailWithWind(forces, descriptor.BoneCount, States[descriptor.StateStart].ProceduralPosition, rootPose.Scale.z);

            float simulationDeltaTime = descriptor.ManagedDeltaTime;
            int stepCount;

            if (simulationDeltaTime > 0f)
            {
                runtime.Accumulator += deltaTime;
                stepCount = math.min(descriptor.MaxSimulationSteps, (int)math.floor(runtime.Accumulator / simulationDeltaTime));

                if (stepCount > 0)
                {
                    runtime.Accumulator -= stepCount * simulationDeltaTime;
                    runtime.Accumulator = math.min(runtime.Accumulator, simulationDeltaTime);
                }
            }
            else
            {
                // Unlimited rate follows the current update loop without accumulating managed ticks
                runtime.Accumulator = 0f;
                simulationDeltaTime = deltaTime;
                stepCount = simulationDeltaTime > 0f ? 1 : 0;
            }
            
            if (stepCount > 0)
            {
                NativeSlice<TurboTailSegmentInput> tailInputs = new NativeSlice<TurboTailSegmentInput>(Inputs, descriptor.StateStart, descriptor.BoneCount + 1);
                NativeSlice<TurboTailSegmentState> tailStates = new NativeSlice<TurboTailSegmentState>(States, descriptor.StateStart, descriptor.BoneCount + 1);
                NativeSlice<TurboTailRootState> tailRoot = new NativeSlice<TurboTailRootState>(RootStates, tailIndex, 1);
                
                NativeSlice<float3> tailPositions = new NativeSlice<float3>(OutputPositions, descriptor.BoneStart, descriptor.BoneCount);
                NativeSlice<quaternion> tailRotations = new NativeSlice<quaternion>(OutputRotations, descriptor.BoneStart, descriptor.BoneCount);
                
                TurboTailRootInput rootInput = BuildRootInput(descriptor, simulationDeltaTime);
                rootInput.Gravity = forces.Gravity;
                rootInput.WindEffect = runtime.WindEffect;
                
                for (int step = 0; step < stepCount; step++)
                {
                    new TurboTailSimulation
                    {
                        Inputs = tailInputs,
                        States = tailStates,
                        RootState = tailRoot,
                        OutputPositions = tailPositions,
                        OutputRotations = tailRotations,
                        RootInput = rootInput,
                        CollisionData = CollisionData,
                        CapsuleAlongContactPlanes = new NativeSlice<TurboCapsuleAlongContactPlane>(CapsuleAlongContactPlanes, descriptor.ColliderStart, descriptor.ColliderCount),
                        ColliderStart = descriptor.ColliderStart,
                        ColliderCount = descriptor.ColliderCount
                    }
                    .Execute();
                }
            }

            // Follow source motion between managed ticks while preserving the last root projection
            OutputPositions[descriptor.BoneStart] = Inputs[descriptor.StateStart].SourcePosition;

            if (descriptor.UseCollision != 0)
                OutputPositions[descriptor.BoneStart] += States[descriptor.StateStart].CollisionOffset;

            RuntimeStates[tailIndex] = runtime;
        }

        void FillInputs(TurboTailDescriptor descriptor, quaternion wave)
        {
            if (descriptor.DetachChildren == 0)
            {
                FillAttachedInputs(descriptor, wave);
                return;
            }

            FillDetachedInputs(descriptor, wave);
        }

        void FillAttachedInputs(TurboTailDescriptor descriptor, quaternion wave)
        {
            TurboTransformPose previousPose = default(TurboTransformPose);

            TurboTransformPose sourceRoot = Poses[Inputs[descriptor.StateStart].SourceTransformIndex];

            quaternion worldWave = math.mul(math.mul(sourceRoot.Rotation, wave), math.inverse(sourceRoot.Rotation));

            for (int i = 0; i < descriptor.BoneCount; i++)
            {
                int inputIndex = descriptor.StateStart + i;
                
                TurboTailSegmentInput input = Inputs[inputIndex];
                TurboTransformPose pose = Poses[input.SourceTransformIndex];

                pose.Position = sourceRoot.Position + math.mul(worldWave, pose.Position - sourceRoot.Position);
                pose.Rotation = math.mul(worldWave, pose.Rotation);

                if (i == 0) pose.LocalRotation = math.mul(pose.LocalRotation, wave);
            
                if (i == 0)
                {
                    TurboTransformPose parent = Poses[descriptor.ParentTransformIndex];
                    
                    if (descriptor.RootUsesVirtualParent != 0)
                    { 
                        parent.Rotation = pose.Rotation;
                        parent.LocalRotation = quaternion.identity;
                    }

                    input.ParentRotation = parent.Rotation; input.ParentLocalRotation = parent.LocalRotation;
                    input.ParentScale = parent.Scale;

                    if (descriptor.RootUsesVirtualParent != 0) 
                        input.LocalPosition = -descriptor.ParentLocalOffset;
                    else 
                        input.LocalPosition = pose.LocalPosition;
                }
                else
                {
                    input.ParentRotation = previousPose.Rotation; input.ParentLocalRotation = previousPose.LocalRotation;
                    input.ParentScale = previousPose.Scale; input.LocalPosition = pose.LocalPosition;
                }
                
                input.SourcePosition = pose.Position;
                input.SourceRotation = pose.Rotation;
                input.SourceLocalRotation = pose.LocalRotation;
                input.SourceScaleX = math.abs(pose.Scale.x);

                Inputs[inputIndex] = input;

                previousPose = pose;
            }
            
            int ghostIndex = descriptor.StateStart + descriptor.BoneCount;

            TurboTailSegmentInput ghost = Inputs[ghostIndex];
            
            ghost.SourcePosition = previousPose.Position + math.mul(previousPose.Rotation, previousPose.Scale * descriptor.ArtificialChildLocalPosition);
            ghost.SourceRotation = previousPose.Rotation;
            ghost.SourceLocalRotation = previousPose.LocalRotation;
            ghost.ParentRotation = previousPose.Rotation; ghost.ParentLocalRotation = previousPose.LocalRotation;
            
            ghost.ParentScale = previousPose.Scale; ghost.LocalPosition = descriptor.ArtificialChildLocalPosition;
            
            Inputs[ghostIndex] = ghost;
        }

        void FillDetachedInputs(TurboTailDescriptor descriptor, quaternion wave)
        {
            bool animateDetachedChildren = descriptor.AnimateDetachedChildren != 0;

            TurboTransformPose previousPose = default(TurboTransformPose);
            TurboTransformPose previousSourcePose = default(TurboTransformPose);
            TurboTransformPose sourceRoot = Poses[Inputs[descriptor.StateStart].SourceTransformIndex];

            if (!animateDetachedChildren)
            {
                int rootBindIndex = descriptor.BoneStart;
                sourceRoot.LocalPosition = BindLocalPositions[rootBindIndex];
                sourceRoot.LocalRotation = BindLocalRotations[rootBindIndex];

                if (descriptor.RootUsesVirtualParent != 0)
                {
                    sourceRoot.Position = sourceRoot.LocalPosition;
                    sourceRoot.Rotation = sourceRoot.LocalRotation;
                }
                else
                {
                    TurboTransformPose parent = Poses[descriptor.ParentTransformIndex];
                    sourceRoot.Position = TransformPoint(parent, sourceRoot.LocalPosition);
                    sourceRoot.Rotation = math.mul(parent.Rotation, sourceRoot.LocalRotation);
                }
            }

            quaternion worldWave = math.mul(math.mul(sourceRoot.Rotation, wave), math.inverse(sourceRoot.Rotation));

            for (int i = 0; i < descriptor.BoneCount; i++)
            {
                int inputIndex = descriptor.StateStart + i;
                int bindIndex = descriptor.BoneStart + i;

                TurboTailSegmentInput input = Inputs[inputIndex];
                TurboTransformPose sourcePose = i == 0 ? sourceRoot : Poses[input.SourceTransformIndex];

                if (i > 0)
                {
                    if (animateDetachedChildren)
                    {
                        sourcePose.Position = TransformPoint(previousSourcePose, sourcePose.LocalPosition);
                        sourcePose.Rotation = math.mul(previousSourcePose.Rotation, sourcePose.LocalRotation);
                        sourcePose.Scale = sourceRoot.Scale * BindRootScales[bindIndex];
                    }
                    else
                    {
                        sourcePose.LocalPosition = BindLocalPositions[bindIndex];
                        sourcePose.LocalRotation = BindLocalRotations[bindIndex];
                        sourcePose.Position = sourceRoot.Position + math.mul(sourceRoot.Rotation, sourceRoot.Scale * BindRootPositions[bindIndex]);
                        sourcePose.Rotation = math.mul(sourceRoot.Rotation, BindRootRotations[bindIndex]);
                        sourcePose.Scale = sourceRoot.Scale * BindRootScales[bindIndex];
                    }
                }

                TurboTransformPose pose = sourcePose;

                // Rotating the start Transform in TailAnimator2 rotates the entire source hierarchy, the stiff reference and blended positions must include it
                pose.Position = sourceRoot.Position + math.mul(worldWave, pose.Position - sourceRoot.Position);
                pose.Rotation = math.mul(worldWave, pose.Rotation);

                if (i == 0) pose.LocalRotation = math.mul(pose.LocalRotation, wave);

                if (i == 0)
                {
                    TurboTransformPose parent = Poses[descriptor.ParentTransformIndex];

                    if (descriptor.RootUsesVirtualParent != 0)
                    {
                        parent.Rotation = pose.Rotation;
                        parent.LocalRotation = quaternion.identity;
                    }

                    input.ParentRotation = parent.Rotation; input.ParentLocalRotation = parent.LocalRotation;
                    input.ParentScale = parent.Scale;

                    if (descriptor.RootUsesVirtualParent != 0)
                        input.LocalPosition = -descriptor.ParentLocalOffset;
                    else
                        input.LocalPosition = pose.LocalPosition;
                }
                else
                {
                    input.ParentRotation = previousPose.Rotation; input.ParentLocalRotation = previousPose.LocalRotation;
                    input.ParentScale = previousPose.Scale; input.LocalPosition = pose.LocalPosition;
                }

                input.SourcePosition = pose.Position;
                input.SourceRotation = pose.Rotation;
                input.SourceLocalRotation = pose.LocalRotation;
                input.SourceScaleX = math.abs(pose.Scale.x);

                Inputs[inputIndex] = input;

                previousPose = pose;
                previousSourcePose = sourcePose;
            }

            int ghostIndex = descriptor.StateStart + descriptor.BoneCount;

            TurboTailSegmentInput ghost = Inputs[ghostIndex];

            ghost.SourcePosition = previousPose.Position + math.mul(previousPose.Rotation, previousPose.Scale * descriptor.ArtificialChildLocalPosition);
            ghost.SourceRotation = previousPose.Rotation;
            ghost.SourceLocalRotation = previousPose.LocalRotation;
            ghost.ParentRotation = previousPose.Rotation; ghost.ParentLocalRotation = previousPose.LocalRotation;

            ghost.ParentScale = previousPose.Scale; ghost.LocalPosition = descriptor.ArtificialChildLocalPosition;

            Inputs[ghostIndex] = ghost;
        }

        static float3 TransformPoint(TurboTransformPose parent, float3 localPosition)
        {
            return parent.Position + math.mul(parent.Rotation, parent.Scale * localPosition);
        }

        void ResetToSource(int tailIndex, TurboTailDescriptor descriptor)
        {
            TurboTransformPose lastPose = default(TurboTransformPose);

            if (descriptor.DetachChildren == 0)
                lastPose = Poses[Inputs[descriptor.StateStart + descriptor.BoneCount - 1].SourceTransformIndex];
            
            for (int i = 0; i <= descriptor.BoneCount; i++)
            {
                int stateIndex = descriptor.StateStart + i;
                
                TurboTailSegmentInput input = Inputs[stateIndex]; 
                TurboTailSegmentState state = States[stateIndex];

                float3 sourcePosition;
                quaternion sourceRotation;

                if (descriptor.DetachChildren != 0)
                {
                    // FillInputs has already reconstructed the logical bind pose.
                    sourcePosition = input.SourcePosition;
                    sourceRotation = input.SourceRotation;
                }
                else
                {
                    // Repose before waving, using this bone's own source rotation.
                    TurboTransformPose source;

                    if (i < descriptor.BoneCount)
                        source = Poses[input.SourceTransformIndex];
                    else
                        source = lastPose;

                    if (i == descriptor.BoneCount)
                        source.Position += math.mul(source.Rotation, source.Scale * descriptor.ArtificialChildLocalPosition);

                    sourcePosition = source.Position;
                    sourceRotation = source.Rotation;
                }

                state.ProceduralPosition = sourcePosition; state.BlendedPosition = sourcePosition;
                state.PreviousPosition = sourcePosition; state.PositionVelocity = float3.zero;
                
                state.PosRefRotation = sourceRotation; state.PreviousPosReferenceRotation = sourceRotation;
                state.RotationVelocity = new quaternion(float4.zero); state.TrueTargetRotation = sourceRotation;
               
                state.CollisionContact = 0; state.CollisionRelevancy = -1f; state.CollisionBlend = 0f;
                state.CollisionActiveContactBlend = 0f; state.CollisionInheritedContact = 0f;
                state.CollisionFramePosition = sourcePosition;
                state.CollisionOffset = float3.zero;
                
                States[stateIndex] = state;
                
                if (i < descriptor.BoneCount)
                {
                    OutputPositions[descriptor.BoneStart + i] = sourcePosition;
                    OutputRotations[descriptor.BoneStart + i] = sourceRotation;
                }
            }
            
            TurboTransformPose parent = Poses[descriptor.ParentTransformIndex];
            TurboTailRootState root = RootStates[tailIndex];
            
            root.ProceduralPosition = parent.Position + math.mul(parent.Rotation, parent.Scale * descriptor.ParentLocalOffset);
            root.PreviousPosition = root.ProceduralPosition; 
            root.PosRefRotation = parent.Rotation;
            root.PreviousPosReferenceRotation = parent.Rotation;
            root.PreviousBasePosition = Inputs[descriptor.StateStart].SourcePosition;
            
            if (descriptor.SafetyTransformIndex >= 0)
            { 
                root.SafetyPosRefRotation = Poses[descriptor.SafetyTransformIndex].Rotation;
                root.SafetyPreviousPosRefRotation = root.SafetyPosRefRotation;
            }

            RootStates[tailIndex] = root;
        }

        TurboTailRootInput BuildRootInput(TurboTailDescriptor descriptor, float simulationDeltaTime)
        {
            TurboTransformPose parent = Poses[descriptor.ParentTransformIndex];

            int rootIndex = Inputs[descriptor.StateStart].SourceTransformIndex;

            if (descriptor.RootUsesVirtualParent != 0) 
                parent.Rotation = Inputs[descriptor.StateStart].SourceRotation;

            int hasSafetyTransform;

            if (descriptor.SafetyTransformIndex >= 0) 
                hasSafetyTransform = 1;
            else 
                hasSafetyTransform = 0;

            quaternion safetyRotation;

            if (descriptor.RootUsesVirtualParent != 0)
            {
                safetyRotation = parent.Rotation;
            }
            else if (descriptor.SafetyTransformIndex >= 0)
            {
                safetyRotation = Poses[descriptor.SafetyTransformIndex].Rotation;
            }
            else
            {
                safetyRotation = quaternion.identity;
            }
            
            return new TurboTailRootInput
            {
                DeltaTime = simulationDeltaTime,
                DistanceCullingBlend = descriptor.DistanceCullingBlend,
                BasePosition = descriptor.DetachChildren != 0 ? Inputs[descriptor.StateStart].SourcePosition : Poses[rootIndex].Position,
                ParentPosition = parent.Position,
                ParentRotation = parent.Rotation,
                ParentScale = parent.Scale,
                ParentLocalOffset = descriptor.ParentLocalOffset,
                HasSafetyTransform = hasSafetyTransform,
                SafetyRotation = safetyRotation,
                MotionInfluence = descriptor.MotionInfluence,
                MotionInfluenceInY = descriptor.MotionInfluenceInY,
                MaxStretching = descriptor.MaxStretching,
                ReactionSpeed = descriptor.ReactionSpeed,
                AngleLimit = descriptor.AngleLimit,
                AngleLimitAxis = descriptor.AngleLimitAxis,
                LimitAxisRange = descriptor.LimitAxisRange,
                LimitSmoothing = descriptor.LimitSmoothing,
                AnimateRoll = descriptor.AnimateRoll,
                UseCollision = descriptor.UseCollision,
                CollisionMode = descriptor.CollisionMode
            };
        }

        quaternion CalculateWave(TurboTailDescriptor descriptor, float blend, float deltaTime, ref TurboTailRuntimeState runtime)
        {
            if (descriptor.UseWaving == 0) return quaternion.identity;

            runtime.WaveTime += deltaTime * (2f * descriptor.WavingSpeed);
            
            float3 euler;
            
            if (descriptor.WavingType == 0)
            {
                float angle = math.sin(runtime.WaveTime) * (30f * descriptor.WavingRange);
                
                if (descriptor.CosinusAdd != 0)
                { 
                    runtime.CosTime += deltaTime * (2.535f * descriptor.WavingSpeed);
                    angle += math.cos(runtime.CosTime) * (27f * descriptor.WavingRange);
                }

                euler = angle * descriptor.WavingAxis * blend;
            }
            else
            {
                float time = runtime.WaveTime * 0.23f;
                
                float x = noise.cnoise(new float2(time, descriptor.AlternateWave * -5f));
                float y = noise.cnoise(new float2(descriptor.AlternateWave * 100f + time, time + descriptor.AlternateWave * 100f));
                float z = noise.cnoise(new float2(descriptor.AlternateWave * 20f, time));
                
                euler = descriptor.WavingAxis * descriptor.WavingRange * 35f * blend * new float3(x, y, z);
            }

            // Match Unity's Quaternion.Euler rotation order while keeping waving
            return quaternion.EulerZXY(math.radians(euler));
        }

    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
    struct TurboWriteTransformsJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<byte> ActiveTails;
        [ReadOnly] public NativeArray<int> TailIndices;
        [ReadOnly] public NativeArray<TurboTailDescriptor> Descriptors;

        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public NativeArray<quaternion> Rotations;

        public void Execute(int index, TransformAccess transform)
        {
            int tailIndex = TailIndices[index];

            if (ActiveTails[tailIndex] == 0) return;

            // Restore the source root position even when descendant position writes are skipped
            TurboTailDescriptor descriptor = Descriptors[tailIndex];

            if (descriptor.WritePositions != 0 || index == descriptor.BoneStart)
            {
                transform.SetPositionAndRotation(Positions[index], Rotations[index]);
            }
            else
            {
                transform.rotation = Rotations[index];
            }
        }
    }
}
#endif
