#if FIMPOSSIBLE_JOBSANDBURST_READY
using Unity.Collections;
using Unity.Mathematics;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator
    {
        internal TailSegment ManagerGhostParent { get { return GhostParent; } }
        internal TailSegment ManagerGhostChild { get { return GhostChild; } }
        internal bool ManagerChildrenDetached { get { return childrenDetached; } }

        internal static TurboTailSegmentState BuildInitialState(TailSegment segment)
        {
            return new TurboTailSegmentState
            {
                ProceduralPosition = segment.ProceduralPosition,
                BlendedPosition = segment.ProceduralPositionWeightBlended,
                PreviousPosition = segment.PreviousPosition,
                PositionVelocity = segment.VelocityHelper,
                PosRefRotation = segment.PosRefRotation,
                PreviousPosReferenceRotation = segment.PreviousPosReferenceRotation,
                RotationVelocity = segment.QVelocityHelper,
                TrueTargetRotation = segment.TrueTargetRotation,
                CollisionRelevancy = -1f,
                CollisionFramePosition = segment.ProceduralPosition
            };
        }

        internal TurboTailRootState BuildInitialRootState()
        {
            TailSegment safetyParent = GhostParent.ParentBone;
            quaternion safetyPositionReferenceRotation;
            quaternion safetyPreviousPositionReferenceRotation;

            if (safetyParent != null)
            {
                safetyPositionReferenceRotation = safetyParent.PosRefRotation;
                safetyPreviousPositionReferenceRotation = safetyParent.PreviousPosReferenceRotation;
            }
            else
            {
                safetyPositionReferenceRotation = quaternion.identity;
                safetyPreviousPositionReferenceRotation = quaternion.identity;
            }
      
            return new TurboTailRootState
            {
                ProceduralPosition = GhostParent.ProceduralPosition,
                PreviousPosition = GhostParent.PreviousPosition,
                PosRefRotation = GhostParent.PosRefRotation,
                PreviousPosReferenceRotation = GhostParent.PreviousPosReferenceRotation,
                SafetyPosRefRotation = safetyPositionReferenceRotation,
                SafetyPreviousPosRefRotation = safetyPreviousPositionReferenceRotation,
                PreviousBasePosition = previousWorldPosition
            };
        }

        /// <summary> Refreshes the artificial root position for editor previews </summary>
        public void Editor_TailCalculations_RefreshArtificialParentBone()
        {
            if (GhostParent == null || GhostParent.transform == null) return;
            GhostParent.ProceduralPosition = GhostParent.transform.position + FEngineering.TransformVector(GhostParent.transform.rotation, GhostParent.transform.lossyScale, GhostParent.LocalOffset);
        }
    }

    struct TurboTailSegmentInput
    {
        public int SourceTransformIndex;

        public float3 SourcePosition;
        public quaternion SourceRotation;
        public quaternion SourceLocalRotation;
        public quaternion ParentRotation;
        public quaternion ParentLocalRotation;
        public float3 ParentScale;
        public float3 LocalPosition;

        public float PositionSpeed;
        public float RotationSpeed;

        public float Slithery;
        public float Curling;
        public float Springiness;

        public float ColliderRadius;
        public float SourceScaleX;
        public float CollisionDamping;
        public float CollisionSlippery;
        public float CollisionReflect;

        public float GravityWeight;

        // Configured bone blend; the simulation applies distance fading to a local copy.
        public float BlendValue;
    }

    struct TurboTailSegmentState
    {
        public float3 ProceduralPosition;
        public float3 BlendedPosition;
        public float3 PreviousPosition;
        public float3 PositionVelocity;

        public quaternion PosRefRotation;
        public quaternion PreviousPosReferenceRotation;
        public quaternion RotationVelocity;
        public quaternion TrueTargetRotation;

        public int CollisionContact;
        public float CollisionRelevancy;
        public float CollisionBlend;
        public float CollisionActiveContactBlend;
        public float CollisionInheritedContact;
        public float3 CollisionFramePosition;
        public float3 CollisionOffset;
    }

    struct TurboTailRootInput
    {
        public float DeltaTime;
        public float DistanceCullingBlend;

        public float3 BasePosition;
        public float3 ParentPosition;
        public quaternion ParentRotation;
        public float3 ParentScale;
        public float3 ParentLocalOffset;

        public int HasSafetyTransform;
        public quaternion SafetyRotation;

        public float MotionInfluence;
        public float MotionInfluenceInY;

        public float MaxStretching;
        public float ReactionSpeed;

        public float AngleLimit;
        public float3 AngleLimitAxis;
        public float2 LimitAxisRange;
        public float LimitSmoothing;
        public int AnimateRoll;

        public int UseCollision;
        public int CollisionMode;

        public float3 Gravity;
        public float3 WindEffect;
    }

    struct TurboTailRootState
    {
        public float3 ProceduralPosition;
        public float3 PreviousPosition;

        public quaternion PosRefRotation;
        public quaternion PreviousPosReferenceRotation;

        public quaternion SafetyPosRefRotation;
        public quaternion SafetyPreviousPosRefRotation;

        public float3 PreviousBasePosition;
    }

    struct TurboTailSimulation
    {
        [ReadOnly] public NativeSlice<TurboTailSegmentInput> Inputs;

        public NativeSlice<TurboTailSegmentState> States;
        public NativeSlice<TurboTailRootState> RootState;

        [WriteOnly] public NativeSlice<float3> OutputPositions;
        [WriteOnly] public NativeSlice<quaternion> OutputRotations;

        [ReadOnly] public TurboTailRootInput RootInput;

        [ReadOnly] public NativeArray<TurboColliderData> CollisionData;
        public NativeSlice<TurboCapsuleAlongContactPlane> CapsuleAlongContactPlanes;

        public int ColliderStart;
        public int ColliderCount;

        public void Execute()
        {
            int transformCount = OutputPositions.Length;
            int stateCount = States.Length;
            float deltaTime = RootInput.DeltaTime;
            float frameFactor = math.min(1f, deltaTime * 60f);

            TurboTailRootState root = RootState[0];
            ApplyMotionInfluence(ref root);
            UpdateArtificialParent(ref root);

            // The first position is anchored, and its current source rotation (including waving) is the slithery reference
            TurboTailSegmentState start = States[0];

            start.PreviousPosition = start.ProceduralPosition;
            start.ProceduralPosition = Inputs[0].SourcePosition;
            start.BlendedPosition = start.ProceduralPosition;
            start.PreviousPosReferenceRotation = start.PosRefRotation;
            start.PosRefRotation = Inputs[0].SourceRotation;

            States[0] = start;

            for (int i = 0; i < stateCount; i++)
            {
                TurboTailSegmentState state = States[i];

                if (RootInput.UseCollision != 0 && Inputs[i].CollisionDamping > 0f)
                    state.CollisionFramePosition = state.ProceduralPosition;
                else
                    state.CollisionFramePosition = float3.zero;

                float3 push = state.ProceduralPosition - state.PreviousPosition;
                state.PreviousPosition = state.ProceduralPosition;

                float springiness = Inputs[i].Springiness;

                if (RootInput.UseCollision != 0 && state.CollisionContact != 0)
                    springiness *= Inputs[i].CollisionSlippery;

                state.ProceduralPosition += push * springiness;

                States[i] = state;
            }

            if (RootInput.UseCollision != 0) // When collisions are enabled
            {
                TurboTailSegmentState rootBone = States[0];
                TurboTailSegmentInput rootBoneInput = Inputs[0];

                rootBoneInput.BlendValue *= RootInput.DistanceCullingBlend;

                // Turbo Tail Animator simply anchors the first bone, let collisions add only their projection offset instead of also exposing its internal spring velocity
                rootBone.ProceduralPosition = rootBoneInput.SourcePosition;
                
                PrepareCollisionResponse(ref rootBone, ref rootBoneInput, deltaTime);
                ResolveCollisions(0, ref rootBone, rootBoneInput.ColliderRadius * rootBoneInput.SourceScaleX, deltaTime);
                
                rootBone.BlendedPosition = math.lerp(rootBoneInput.SourcePosition, rootBone.ProceduralPosition, rootBoneInput.BlendValue);
                rootBone.CollisionOffset = rootBone.BlendedPosition - rootBoneInput.SourcePosition;
                
                States[0] = rootBone;
            }

            for (int i = 1; i < stateCount; i++)
            {
                TurboTailSegmentInput input = Inputs[i];
                input.BlendValue *= RootInput.DistanceCullingBlend;
                TurboTailSegmentState state = States[i];

                float collisionResponse = PrepareCollisionResponse(ref state, ref input, deltaTime);

                float3 parentPosition = States[i - 1].ProceduralPosition;
                quaternion grandParentRotation;
                quaternion grandParentPreviousRotation;

                if (i == 1)
                {
                    grandParentRotation = root.PosRefRotation;
                    grandParentPreviousRotation = root.PreviousPosReferenceRotation;
                }
                else
                {
                    TurboTailSegmentState grandParent = States[i - 2];
                    grandParentRotation = grandParent.PosRefRotation;
                    grandParentPreviousRotation = grandParent.PreviousPosReferenceRotation;
                }

                float3 boneDimensions = input.ParentScale * input.LocalPosition;
                float boneLength = math.length(boneDimensions);
                quaternion swingRotation = GetSwingRotation(input, grandParentRotation, grandParentPreviousRotation);

                float3 targetPosition = parentPosition + math.mul(swingRotation, boneDimensions);
                targetPosition = ApplyForcesToTarget(parentPosition, targetPosition, boneLength, RootInput.Gravity * input.GravityWeight + RootInput.WindEffect);
                
                ApplyCollisionDirectionDamping(i, ref state, parentPosition, ref targetPosition, input.CollisionDamping, collisionResponse);
                
                state.ProceduralPosition += (targetPosition - state.ProceduralPosition) * input.Curling * frameFactor;

                if (input.PositionSpeed < 1f)
                {
                    float smoothTime = math.lerp(0.08f, 0.0001f, math.sqrt(math.sqrt(input.PositionSpeed)));
                    state.ProceduralPosition = SmoothDamp( state.PreviousPosition, state.ProceduralPosition, ref state.PositionVelocity, smoothTime, deltaTime);
                }

                if (i < transformCount && RootInput.UseCollision != 0 && RootInput.CollisionMode == (int)TurboTailAnimator.ECollisionMode.SpherePerBone)
                {
                    ResolveCollisions(i, ref state, input.ColliderRadius * input.SourceScaleX, deltaTime);
                }
                else if (i >= transformCount || RootInput.UseCollision == 0)
                {
                    ClearCollisionState(ref state);
                }

                if (RootInput.MaxStretching < 1f) LimitStretching(ref state.ProceduralPosition, parentPosition, boneLength, RootInput.MaxStretching, RootInput.ReactionSpeed, deltaTime);

                if (i < transformCount && RootInput.UseCollision != 0 && RootInput.CollisionMode == (int)TurboTailAnimator.ECollisionMode.CapsulesAlongBones)
                {
                    float parentRadius = Inputs[i - 1].ColliderRadius * Inputs[i - 1].SourceScaleX;
                    float boneRadius = input.ColliderRadius * input.SourceScaleX;

                    ResolveCapsuleAlongCollisions(i, ref state, parentPosition, parentRadius, boneRadius, deltaTime);
                }

                // Apply this after position smoothing, collisions and the length constraint so those systems cannot make the following simulation tick re-correct the bend
                if (RootInput.AngleLimit < 181f)
                    state.ProceduralPosition = LimitAngle(state.ProceduralPosition, parentPosition, input, RootInput.AngleLimit,
                        RootInput.AngleLimitAxis, RootInput.LimitAxisRange, RootInput.LimitSmoothing, deltaTime);

                state.BlendedPosition = math.lerp(input.SourcePosition, state.ProceduralPosition, input.BlendValue);
       
                States[i] = state;
            }

            for (int childIndex = 1; childIndex < stateCount; childIndex++)
            {
                int parentIndex = childIndex - 1;

                TurboTailSegmentInput childInput = Inputs[childIndex];
                TurboTailSegmentState childState = States[childIndex];
                TurboTailSegmentState parentState = States[parentIndex];

                quaternion parentRotation = childInput.ParentRotation;

                if (parentIndex > 0)
                {
                    quaternion inheritedRotation = math.mul(States[parentIndex - 1].TrueTargetRotation, math.inverse(Inputs[parentIndex].ParentRotation));
                    parentRotation = math.mul(inheritedRotation, parentRotation);
                }

                float3 sourceDirection = math.mul(parentRotation, childInput.LocalPosition);
                float3 targetDirection = childState.BlendedPosition - parentState.BlendedPosition;
                quaternion targetRotation = math.mul(FromToRotation(sourceDirection, targetDirection), parentRotation);

                if (RootInput.AnimateRoll != 0)
                {
                    // Smooth the final target rotation rather than only the position-reference rotation used by slithery motion
                    float rollLerp = math.saturate(deltaTime * math.lerp(10f, 60f, childInput.RotationSpeed));
                    targetRotation = LerpUnclamped(parentState.TrueTargetRotation, targetRotation, rollLerp);
                }

                parentState.TrueTargetRotation = targetRotation;
                parentState.PreviousPosReferenceRotation = parentState.PosRefRotation;

                if (RootInput.AnimateRoll == 0 && childInput.RotationSpeed < 1f)
                {
                    float smoothTime = math.lerp(0.25f, 0.0001f, math.sqrt(math.sqrt(childInput.RotationSpeed)));
                    parentState.PosRefRotation = SmoothDampRotation(parentState.PosRefRotation, targetRotation, ref childState.RotationVelocity, smoothTime, deltaTime);
                }
                else
                {
                    parentState.PosRefRotation = targetRotation;
                }

                States[parentIndex] = parentState;
                States[childIndex] = childState;
                OutputRotations[parentIndex] = targetRotation;
            }

            for (int i = 0; i < transformCount; i++) OutputPositions[i] = States[i].BlendedPosition;

            root.PreviousBasePosition = RootInput.BasePosition;

            RootState[0] = root;
        }

        float PrepareCollisionResponse(ref TurboTailSegmentState state, ref TurboTailSegmentInput input, float deltaTime)
        {
            if (RootInput.UseCollision == 0)
            {
                ClearCollisionState(ref state);
                return 1f;
            }

            float damping = input.CollisionDamping;

            if (damping > 0f)
            {
                float target;
                if (state.CollisionRelevancy > 0f) target = 1f;
                else target = 0f;

                float response = 1f - math.exp(-deltaTime * 20f / damping);
                state.CollisionBlend = math.lerp(state.CollisionBlend, target, response);
                if (target == 0f && state.CollisionBlend < 0.0001f) state.CollisionBlend = 0f;

                float activeTarget;
                if (state.CollisionContact != 0) activeTarget = 1f;
                else activeTarget = 0f;

                state.CollisionActiveContactBlend = math.lerp(state.CollisionActiveContactBlend, activeTarget, response);
                if (activeTarget == 0f && state.CollisionActiveContactBlend < 0.0001f) state.CollisionActiveContactBlend = 0f;
                input.Slithery = math.lerp(input.Slithery, input.CollisionReflect, state.CollisionBlend);
                
                return response;
            }
            else
            {
                state.CollisionBlend = 0f;
                state.CollisionActiveContactBlend = 0f;
                state.CollisionInheritedContact = 0f;
                state.CollisionFramePosition = float3.zero;
                if (state.CollisionRelevancy > 0f) input.Slithery = input.CollisionReflect;
                return 1f;
            }
        }

        void ApplyCollisionDirectionDamping(int boneIndex, ref TurboTailSegmentState state, float3 parentPosition, ref float3 targetPosition, float damping, float contactResponse)
        {
            if (damping <= 0f || boneIndex <= 0)
            {
                state.CollisionInheritedContact = 0f;
                return;
            }

            TurboTailSegmentState parent = States[boneIndex - 1];
            state.CollisionInheritedContact = math.max(parent.CollisionBlend, parent.CollisionInheritedContact);

            // Free bones following a recent contact retain their previous solved direction, an actively colliding bone stays responsive so it can slide over the surface
            float followContact = math.max(state.CollisionInheritedContact, state.CollisionBlend) * (1f - state.CollisionActiveContactBlend);
            if (followContact <= 0.0001f) return;

            float3 previousDirection = state.CollisionFramePosition - parent.CollisionFramePosition;
            float3 targetOffset = targetPosition - parentPosition;

            float previousLengthSq = math.lengthsq(previousDirection);
            float targetLengthSq = math.lengthsq(targetOffset);
            if (previousLengthSq <= 0.000000000001f || targetLengthSq <= 0.000000000001f) return;

            float response = math.lerp(1f, contactResponse, followContact);
            float3 dampedDirection = SlerpDirection(previousDirection, targetOffset, response);

            targetPosition = parentPosition + dampedDirection * math.sqrt(targetLengthSq);
        }

        static float3 SlerpDirection(float3 from, float3 to, float value)
        {
            float3 fromNormal = math.normalizesafe(from);
            float3 toNormal = math.normalizesafe(to);
            float dot = math.clamp(math.dot(fromNormal, toNormal), -1f, 1f);

            if (dot > 0.9995f) return math.normalizesafe(math.lerp(fromNormal, toNormal, value), toNormal);

            if (dot < -0.9995f)
            {
                float3 reference;
                if (math.abs(fromNormal.y) < 0.9f) reference = new float3(0f, 1f, 0f);
                else reference = new float3(1f, 0f, 0f);

                float3 perpendicular = math.normalizesafe(math.cross(fromNormal, reference), new float3(0f, 0f, 1f));
                float oppositeAngle = math.PI * value;

                return fromNormal * math.cos(oppositeAngle) + perpendicular * math.sin(oppositeAngle);
            }

            float angleBetween = math.acos(dot);

            float3 relative = math.normalizesafe(toNormal - fromNormal * dot, toNormal);

            float interpolationAngle = angleBetween * value;

            return fromNormal * math.cos(interpolationAngle) + relative * math.sin(interpolationAngle);
        }

        void ResolveCollisions(int boneIndex, ref TurboTailSegmentState state, float boneRadius, float deltaTime)
        {
            if (state.CollisionContact != 0) state.CollisionContact = 0;
            else if (state.CollisionRelevancy > 0f) state.CollisionRelevancy -= deltaTime;

            if (boneRadius <= math.EPSILON || ColliderCount == 0) return;

            bool contact = false;

            for (int i = 0; i < ColliderCount; i++)
            {
                TurboColliderData collider = CollisionData[ColliderStart + i];
                if (collider.Type == (int)TurboColliderType.None) continue;

                if (collider.Type == (int)TurboColliderType.Box)
                {
                    if (PushOutFromBox(ref state.ProceduralPosition, boneRadius, collider))
                    {
                        contact = true;
                    }
                    else
                    {
                        float3 contactCenter;
                        float3 contactNormal;

                        if (TrySweepSphereFromBox(state.PreviousPosition, state.ProceduralPosition, boneRadius, collider, out contactCenter, out contactNormal))
                        {
                            float3 remaining = state.ProceduralPosition - contactCenter;
                            float intoSurface = math.dot(remaining, contactNormal);
                            if (intoSurface < 0f) remaining -= contactNormal * intoSurface;

                            state.ProceduralPosition = contactCenter + remaining;
                            PushOutFromBox(ref state.ProceduralPosition, boneRadius, collider);
                            contact = true;
                        }
                    }

                    continue;
                }

                if (collider.Radius <= 0f) continue;
                if (PushOutFromCapsule(ref state.ProceduralPosition, boneRadius, collider)) contact = true;
            }

            if (!contact) return;

            state.CollisionContact = 1;
            state.CollisionRelevancy = deltaTime * 7f;

            MarkChildCollisionRelevancy(boneIndex + 1, deltaTime * 3.5f);
            MarkChildCollisionRelevancy(boneIndex + 2, deltaTime * 3f);
        }

        void MarkChildCollisionRelevancy(int index, float relevancy)
        {
            if (index >= States.Length) return;

            TurboTailSegmentState child = States[index];
            child.CollisionRelevancy = math.max(child.CollisionRelevancy, relevancy);

            States[index] = child;
        }

        void ResolveCapsuleAlongCollisions(int boneIndex, ref TurboTailSegmentState state, float3 parentPosition, float parentRadius, float boneRadius, float deltaTime)
        {
            if( state.CollisionContact != 0 )
            {
                state.CollisionContact = 0;
            }
            else
            {
                if( state.CollisionRelevancy > 0f ) state.CollisionRelevancy -= deltaTime;
            }

            parentRadius = math.max(0f, parentRadius);
            boneRadius = math.max(0f, boneRadius);

            if (ColliderCount == 0 || (parentRadius <= math.EPSILON && boneRadius <= math.EPSILON)) return;

            float3 wanted = state.ProceduralPosition - parentPosition;

            float sideLength = math.length(wanted);
            if (sideLength <= 0.0000001f) return;

            float thickness = math.max(parentRadius, boneRadius);
            int planeCount = 0;
            bool pointFallbackContact = false;

            for (int i = 0; i < ColliderCount; i++)
            {
                TurboColliderData collider = CollisionData[ColliderStart + i];

                if (collider.Type == (int)TurboColliderType.None) continue;

                if (collider.Type == (int)TurboColliderType.Box)
                {
                    float3 point = parentPosition + wanted;

                    if (PushOutFromBox(ref point, boneRadius, collider))
                    {
                        float3 fallbackNormal = math.normalizesafe(point - (parentPosition + wanted), CapsuleAlongPerpendicular(wanted));

                        CapsuleAlongContactPlanes[planeCount++] = new TurboCapsuleAlongContactPlane
                        {
                            Normal = fallbackNormal,
                            Distance = math.dot(point - parentPosition, fallbackNormal)
                        };

                        pointFallbackContact = true;
                    }

                    continue;
                }

                if (collider.Radius <= 0f) continue;

                float reach = sideLength + thickness + collider.Radius + 0.0001f;
                if (PointSegmentDistanceSq(parentPosition, collider.PointA, collider.PointB) > reach * reach) continue;

                float3 normal;
                CapsuleAlongSurfaceGap(parentPosition, parentPosition + wanted, parentRadius, boneRadius, collider, out normal);
                float cBorder = math.max(collider.Radius, 0.001f) * 0.0001f;

                if (collider.Type == (int)TurboColliderType.Sphere)
                {
                    // The forbidden directions around a sphere form a cone, solving its boundary analytically avoids a changing contact plane while sliding
                    float3 away = parentPosition - collider.PointA;
                    float distance = math.length(away);

                    if (distance > 0.000001f)
                    {
                        float radius = parentRadius + collider.Radius + cBorder;
                        float taper = (boneRadius - parentRadius) / sideLength;
                        float radial = distance * distance - radius * radius;

                        bool tangentContact = false;
                        if (math.abs(taper) < 1f && math.max(0f, radial) <= sideLength * sideLength * (1f - taper * taper)) tangentContact = true;

                        float cosine;

                        if (tangentContact)
                        {
                            cosine = (SmoothCapsuleAlongTangent(math.max(0f, radial), radius) * math.sqrt(math.max(0f, 1f - taper * taper)) - radius * taper) / distance;
                        }
                        else
                        {
                            cosine = (radial + (1f - taper * taper) * sideLength * sideLength - 2f * radius * taper * sideLength) / (2f * distance * sideLength);
                        }

                        CapsuleAlongContactPlanes[planeCount++] = new TurboCapsuleAlongContactPlane
                        {
                            Normal = away / distance,
                            Distance = -cosine * sideLength
                        };

                        continue;
                    }
                }

                // Choose a supporting normal which keeps the already finalized parent sphere on the free side of the collider, then only the child endpoint is moved
                normal = ConstrainSupportingNormal(normal, parentPosition - collider.PointA, parentRadius + collider.Radius + cBorder);
                
                if (collider.Type == (int)TurboColliderType.Capsule && math.lengthsq(collider.PointB - collider.PointA) > 0.000000000001f)
                {
                    normal = ConstrainSupportingNormal(normal, parentPosition - collider.PointB, parentRadius + collider.Radius + cBorder);
                    normal = ConstrainSupportingNormal(normal, parentPosition - collider.PointA, parentRadius + collider.Radius + cBorder);
                }

                float support = math.max(math.dot(collider.PointA - parentPosition, normal), math.dot(collider.PointB - parentPosition, normal));

                CapsuleAlongContactPlanes[planeCount++] = new TurboCapsuleAlongContactPlane
                {
                    Normal = normal,
                    Distance = support + collider.Radius + boneRadius + cBorder
                };
            }

            float3 solved = SolveCapsuleAlongPlanes(wanted, sideLength, CapsuleAlongContactPlanes, planeCount);

            bool corrected = math.lengthsq(solved - wanted) > 0.000000000001f;
            state.ProceduralPosition = parentPosition + solved;

            if (corrected)
            {
                float3 velocity = state.ProceduralPosition - state.PreviousPosition;
                velocity = ProjectVelocityOnActivePlanes(velocity, solved, CapsuleAlongContactPlanes, planeCount);
                
                state.PreviousPosition = state.ProceduralPosition - velocity;
                state.PositionVelocity = ProjectVelocityOnActivePlanes(state.PositionVelocity, solved, CapsuleAlongContactPlanes, planeCount);
            }

            bool contact = pointFallbackContact;

            for (int i = 0; i < ColliderCount; i++)
            {
                TurboColliderData collider = CollisionData[ColliderStart + i];

                if (collider.Type == (int)TurboColliderType.None || collider.Type == (int)TurboColliderType.Box || collider.Radius <= 0f) continue;

                float reach = sideLength + thickness + collider.Radius + 0.0001f;

                if (PointSegmentDistanceSq(parentPosition, collider.PointA, collider.PointB) > reach * reach) continue;

                float3 normal;
                float gap = CapsuleAlongSurfaceGap(parentPosition, state.ProceduralPosition, parentRadius, boneRadius, collider, out normal);
                
                if (gap <= math.max(boneRadius * 0.02f, 0.00001f))
                {
                    contact = true;
                    break;
                }
            }

            if (!contact && !corrected) return;

            state.CollisionContact = 1;
            state.CollisionRelevancy = deltaTime * 7f;

            MarkChildCollisionRelevancy(boneIndex + 1, deltaTime * 3.5f);
            MarkChildCollisionRelevancy(boneIndex + 2, deltaTime * 3f);

            TurboTailSegmentState parent = States[boneIndex - 1];

            parent.CollisionContact = 1;
            parent.CollisionRelevancy = deltaTime * 7f;

            States[boneIndex - 1] = parent;
        }

        static float3 ConstrainSupportingNormal(float3 normal, float3 offset, float radius)
        {
            float distance = math.length(offset);

            if (distance <= 0.00000001f) return normal;

            float3 axis = offset / distance;
            float requiredDot = math.saturate(radius / distance);
            float currentDot = math.dot(normal, axis);

            if (currentDot >= requiredDot) return normal;

            float3 tangent = normal - axis * currentDot;

            tangent = math.normalizesafe(tangent, CapsuleAlongPerpendicular(axis));

            return math.normalizesafe(axis * requiredDot + tangent * math.sqrt(math.max(0f, 1f - requiredDot * requiredDot)), axis);
        }

        static float3 SolveCapsuleAlongPlanes(float3 wanted, float length, NativeSlice<TurboCapsuleAlongContactPlane> planes, int planeCount)
        {
            float3 fallback = math.normalizesafe(wanted, new float3(0f, 0f, 1f));
            float3 best = fallback * length;

            float tolerance = 0.000001f * math.max(1f, length);
            float bestViolation = CapsuleAlongPlaneViolation(best, planes, planeCount);

            if (bestViolation <= tolerance) return best;

            float bestDistance = math.lengthsq(best - wanted);
            float lengthSq = length * length;

            for (int i = 0; i < planeCount; i++)
            {
                TurboCapsuleAlongContactPlane a = planes[i];
                float3 origin = a.Normal * a.Distance;
                float originSq = math.lengthsq(origin);

                if (originSq <= lengthSq)
                {
                    float3 tangent = wanted - a.Normal * math.dot(wanted, a.Normal);
                    float radial = math.sqrt(math.max(0f, lengthSq - originSq));
                    float3 projected = origin + math.normalizesafe(tangent, CapsuleAlongPerpendicular(a.Normal)) * radial;
                    
                    ConsiderCapsuleAlongCandidate(projected, wanted, length, tolerance, planes, planeCount, ref best, ref bestViolation, ref bestDistance);
                    
                    if (math.lengthsq(tangent) < 0.000000000001f && radial > 0f) ConsiderCapsuleAlongCandidate(origin - CapsuleAlongPerpendicular(a.Normal) * radial, wanted, length, tolerance, planes, planeCount, ref best, ref bestViolation, ref bestDistance);
                }

                for (int j = 0; j < i; j++)
                {
                    TurboCapsuleAlongContactPlane b = planes[j];
                    float3 cross = math.cross(a.Normal, b.Normal);

                    float determinant = math.lengthsq(cross);

                    if (determinant < 0.000000000001f) continue;

                    float dot = math.dot(a.Normal, b.Normal);
                    origin = (a.Normal * (a.Distance - b.Distance * dot) + b.Normal * (b.Distance - a.Distance * dot)) / determinant;
                    originSq = math.lengthsq(origin);

                    if (originSq <= lengthSq)
                    {
                        float3 line = cross * math.rsqrt(determinant);
                        float limit = math.sqrt(math.max(0f, lengthSq - originSq));
                        float along = math.clamp(math.dot(wanted - origin, line), -limit, limit);

                        ConsiderCapsuleAlongCandidate(origin + line * along, wanted, length, tolerance, planes, planeCount, ref best, ref bestViolation, ref bestDistance);
                        
                        if (math.abs(along) < limit)
                        {
                            ConsiderCapsuleAlongCandidate(origin + line * limit, wanted, length, tolerance, planes, planeCount, ref best, ref bestViolation, ref bestDistance);
                            ConsiderCapsuleAlongCandidate(origin - line * limit, wanted, length, tolerance, planes, planeCount, ref best, ref bestViolation, ref bestDistance);
                        }
                    }

                    for (int k = 0; k < j; k++)
                    {
                        TurboCapsuleAlongContactPlane c = planes[k];
                        float volume = math.dot(cross, c.Normal);

                        if (math.abs(volume) < 0.00000001f) continue;

                        float3 vertex = (math.cross(b.Normal, c.Normal) * a.Distance + math.cross(c.Normal, a.Normal) * b.Distance + cross * c.Distance) / volume;
                        ConsiderCapsuleAlongCandidate(vertex, wanted, length, tolerance, planes, planeCount, ref best, ref bestViolation, ref bestDistance);
                    }
                }
            }

            return best;
        }

        static void ConsiderCapsuleAlongCandidate(float3 candidate, float3 wanted, float length, float tolerance, NativeSlice<TurboCapsuleAlongContactPlane> planes, int planeCount, ref float3 best, ref float bestViolation, ref float bestDistance)
        {
            float candidateLength = math.length(candidate);

            if (!math.isfinite(candidateLength) || math.abs(candidateLength - length) > tolerance) return;

            float violation = CapsuleAlongPlaneViolation(candidate, planes, planeCount);
            float distance = math.lengthsq(candidate - wanted);
            bool feasible = violation <= tolerance;
            bool bestFeasible = bestViolation <= tolerance;

            if ( (feasible && !bestFeasible) 
                || (feasible && bestFeasible && distance < bestDistance)
                || (!feasible && !bestFeasible && (violation < bestViolation || (violation == bestViolation && distance < bestDistance))))
                {
                    best = candidate;
                    bestViolation = violation;
                    bestDistance = distance;
                }
        }

        static float CapsuleAlongPlaneViolation(float3 value, NativeSlice<TurboCapsuleAlongContactPlane> planes, int planeCount)
        {
            float violation = 0f;
            
            for (int i = 0; i < planeCount; i++) 
                violation = math.max(violation, planes[i].Distance - math.dot(value, planes[i].Normal));
            
            return violation;
        }

        static float3 ProjectVelocityOnActivePlanes(float3 velocity, float3 solved, NativeSlice<TurboCapsuleAlongContactPlane> planes, int planeCount)
        {
            for (int pass = 0; pass < 3; pass++)
            {
                for (int i = 0; i < planeCount; i++)
                {
                    TurboCapsuleAlongContactPlane plane = planes[i];
                    if (math.dot(solved, plane.Normal) > plane.Distance + 0.0001f) continue;

                    float inward = math.dot(velocity, plane.Normal);
                    if (inward < 0f) velocity -= plane.Normal * inward;
                }
            }

            return velocity;
        }

        static float CapsuleAlongSurfaceGap(float3 a, float3 b, float radiusA, float radiusB, TurboColliderData collider, out float3 normal)
        {
            float s, t;

            ClosestCapsuleAlongParameters(a, b, radiusA, radiusB, collider.PointA, collider.PointB, out s, out t);
            
            float3 separation = math.lerp(a, b, s) - math.lerp(collider.PointA, collider.PointB, t);
            float distance = math.length(separation);
            
            if (distance > 0.000001f) 
                normal = separation / distance;
            else 
                normal = CapsuleAlongPerpendicular(b - a);

            return distance - math.lerp(radiusA, radiusB, s) - collider.Radius;
        }

        static void ClosestCapsuleAlongParameters(float3 a, float3 b, float radiusA, float radiusB, float3 c, float3 d, out float s, out float t)
        {
            float3 side = b - a;
            float3 axis = d - c;

            float deltaRadius = radiusB - radiusA;
            s = MinimumTaperedDistance(a - c, side, deltaRadius, 0f, 1f);

            float axisLengthSq = math.lengthsq(axis);
            t = 0f;

            if (axisLengthSq <= 0.000000000001f) return;

            float best = TaperedCapsuleAlongSeparation(a, side, radiusA, deltaRadius, c, axis, axisLengthSq, s, out t);
            float candidate = MinimumTaperedDistance(a - d, side, deltaRadius, 0f, 1f);
            float candidateT;

            float separation = TaperedCapsuleAlongSeparation(a, side, radiusA, deltaRadius, c, axis, axisLengthSq, candidate, out candidateT);
            
            if (separation < best)
            {
                best = separation;
                s = candidate;
                t = candidateT;
            }

            float startProjection = math.dot(a - c, axis) / axisLengthSq;
            float projectionStep = math.dot(side, axis) / axisLengthSq;

            float lower = 0f;
            float upper = 1f;

            if (math.abs(projectionStep) < 0.00000001f)
            {
                if (startProjection < 0f || startProjection > 1f) return;
            }
            else
            {
                float first = -startProjection / projectionStep;
                float last = (1f - startProjection) / projectionStep;
                lower = math.max(0f, math.min(first, last));
                upper = math.min(1f, math.max(first, last));

                if (lower > upper) return;
            }

            candidate = MinimumTaperedDistance(a - c - axis * startProjection, side - axis * projectionStep, deltaRadius, lower, upper);
            separation = TaperedCapsuleAlongSeparation(a, side, radiusA, deltaRadius, c, axis, axisLengthSq, candidate, out candidateT);
            
            if (separation < best)
            {
                s = candidate;
                t = candidateT;
            }
        }

        static float TaperedCapsuleAlongSeparation(float3 start, float3 side, float radius, float deltaRadius, float3 center, float3 axis, float axisLengthSq, float s, out float t)
        {
            float3 point = start + side * s;

            t = math.saturate(math.dot(point - center, axis) / axisLengthSq);

            return math.length(point - center - axis * t) - (radius + deltaRadius * s);
        }

        static float MinimumTaperedDistance(float3 offset, float3 direction, float deltaRadius, float lower, float upper)
        {
            float lengthSq = math.lengthsq(direction);

            if (lengthSq <= 0.000000000001f || deltaRadius * deltaRadius >= lengthSq)
            {
                if (deltaRadius > 0f) return upper; else return lower;
            }

            float dot = math.dot(offset, direction);
            float perpendicularSq = math.max(0f, math.lengthsq(offset) - dot * dot / lengthSq);
            float s = -dot / lengthSq + deltaRadius * math.sqrt(perpendicularSq / (lengthSq * (lengthSq - deltaRadius * deltaRadius)));
            
            return math.clamp(s, lower, upper);
        }

        static float PointSegmentDistanceSq(float3 point, float3 start, float3 end)
        {
            float3 line = end - start;
            float lengthSq = math.lengthsq(line);
            float t;

            if (lengthSq > 0.000000000001f) 
                t = math.saturate(math.dot(point - start, line) / lengthSq); 
            else 
                t = 0f;

            return math.lengthsq(point - start - line * t);
        }

        static float3 CapsuleAlongPerpendicular(float3 direction)
        {
            direction = math.normalizesafe(direction, new float3(0f, 0f, 1f));
            float3 reference;

            if (math.abs(direction.y) < 0.9f) 
                reference = new float3(0f, 1f, 0f);
            else 
                reference = new float3(1f, 0f, 0f);

            return math.normalizesafe(math.cross(direction, reference), new float3(0f, 1f, 0f));
        }

        static float SmoothCapsuleAlongTangent(float squared, float scale)
        {
            float weight = math.saturate(squared / math.max(scale * scale * 0.01f, 0.000000000001f));

            return math.sqrt(squared) * weight * weight * (3f - 2f * weight);
        }

        static bool PushOutFromBox(ref float3 position, float boneRadius, TurboColliderData collider)
        {
            float3 correction;
            float3 normal;

            if (!TryGetBoxSphereContact(position, boneRadius, collider, out correction, out normal)) return false;

            position += correction;

            return true;
        }

        static bool TrySweepSphereFromBox(float3 start, float3 end, float radius, TurboColliderData collider, out float3 contactCenter, out float3 normal)
        {
            float cBorder = BoxContactBorder(radius);

            if (!SweptSphereMightHitBox(start, end, radius + cBorder, collider))
            {
                contactCenter = float3.zero;
                normal = float3.zero;
                return false;
            }

            float startGap = GetBoxSphereGap(start, radius, collider);

            if (startGap <= cBorder)
            {
                float3 correction;

                if (!TryGetBoxSphereContact(start, radius + cBorder, collider, out correction, out normal))
                {
                    contactCenter = float3.zero;
                    return false;
                }

                // Do not retain a resting contact when the prediction is already separating
                if (math.dot(end - start, normal) >= 0f)
                {
                    contactCenter = float3.zero;
                    normal = float3.zero;
                    return false;
                }

                contactCenter = start + correction;

                return true;
            }

            float lower = 0f;
            float upper = 1f;

            // Match the reference collider's bounded, exact signed-distance search while keeping the uncommon box-sweep path predictable for Burst jobs
            for (int i = 0; i < 8; i++)
            {
                float first = (lower * 2f + upper) / 3f;
                float second = (lower + upper * 2f) / 3f;
                float firstGap = GetBoxSphereGap(math.lerp(start, end, first), radius, collider);
                float secondGap = GetBoxSphereGap(math.lerp(start, end, second), radius, collider);

                if( firstGap < secondGap )
                {
                    upper = second;
                }
                else
                {
                    if( secondGap < firstGap )
                    {
                        lower = first;
                    }
                    else
                    {
                        lower = first;
                        upper = second;
                    }
                }
            }

            float deepestTime = (lower + upper) * .5f;
            float bestGap = GetBoxSphereGap(math.lerp(start, end, deepestTime), radius, collider);
            float startOnlyGap = GetBoxSphereGap(start, radius, collider);

            if (startOnlyGap < bestGap)
            {
                bestGap = startOnlyGap;
                deepestTime = 0f;
            }

            float endGap = GetBoxSphereGap(end, radius, collider);

            if (endGap < bestGap)
            {
                bestGap = endGap;
                deepestTime = 1f;
            }

            if (bestGap > cBorder)
            {
                contactCenter = float3.zero;
                normal = float3.zero;
                return false;
            }

            lower = 0f;
            upper = deepestTime;

            for( int i = 0; i < 9; i++ )
            {
                float middle = ( lower + upper ) * .5f;

                if( GetBoxSphereGap( math.lerp( start, end, middle ), radius, collider ) <= cBorder )
                {
                    upper = middle;
                }
                else
                {
                    lower = middle;
                }
            }

            float3 center = math.lerp(start, end, upper);
            float3 contactCorrection;

            if (!TryGetBoxSphereContact(center, radius + cBorder, collider, out contactCorrection, out normal))
            {
                contactCenter = float3.zero;
                return false;
            }

            contactCenter = center + contactCorrection;

            return true;
        }

        static bool TryGetBoxSphereContact(float3 sphereCenter, float sphereRadius, TurboColliderData collider, out float3 correction, out float3 normal)
        {
            sphereRadius = math.max(0f, sphereRadius);

            float3 localPoint = math.mul(collider.BoxInverseRotation, sphereCenter - collider.PointA);
            float3 closest = math.clamp(localPoint, -collider.BoxHalfExtents, collider.BoxHalfExtents);
            float3 localDelta = localPoint - closest;

            float distanceSq = math.lengthsq(localDelta);
            float radiusSq = sphereRadius * sphereRadius;

            if (distanceSq > radiusSq)
            {
                correction = float3.zero;
                normal = float3.zero;

                return false;
            }

            float cBorder = BoxContactBorder(sphereRadius);

            float3 localNormal;
            float3 localCorrection;

            if (distanceSq > 0.000000000001f)
            {
                float distance = math.sqrt(distanceSq);
                localNormal = localDelta / distance;
                localCorrection = localNormal * (sphereRadius + cBorder - distance);
            }
            else
            {
                // Choose the shortest deterministic exit so normals do not alternate at edges
                float3 faceDistance = collider.BoxHalfExtents - math.abs(localPoint);

                if (faceDistance.x <= faceDistance.y && faceDistance.x <= faceDistance.z)
                {
                    localNormal = new float3(localPoint.x < 0f ? -1f : 1f, 0f, 0f);
                    localCorrection = localNormal * (math.max(0f, faceDistance.x) + sphereRadius + cBorder);
                }
                else if (faceDistance.y <= faceDistance.z)
                {
                    localNormal = new float3(0f, localPoint.y < 0f ? -1f : 1f, 0f);
                    localCorrection = localNormal * (math.max(0f, faceDistance.y) + sphereRadius + cBorder);
                }
                else
                {
                    localNormal = new float3(0f, 0f, localPoint.z < 0f ? -1f : 1f);
                    localCorrection = localNormal * (math.max(0f, faceDistance.z) + sphereRadius + cBorder);
                }
            }

            normal = math.mul(collider.BoxRotation, localNormal);
            correction = math.mul(collider.BoxRotation, localCorrection);
            return true;
        }

        static bool SweptSphereMightHitBox(float3 start, float3 end, float radius, TurboColliderData collider)
        {
            float3 sweptMin = math.min(start, end) - radius;
            float3 sweptMax = math.max(start, end) + radius;

            float3 boxMin = collider.PointA - collider.BoxBoundsExtents;
            float3 boxMax = collider.PointA + collider.BoxBoundsExtents;

            return 
                sweptMin.x <= boxMax.x && sweptMax.x >= boxMin.x
                && sweptMin.y <= boxMax.y && sweptMax.y >= boxMin.y
                && sweptMin.z <= boxMax.z && sweptMax.z >= boxMin.z
                ;
        }

        static float GetBoxSphereGap(float3 point, float radius, TurboColliderData collider)
        {
            float3 local = math.mul(collider.BoxInverseRotation, point - collider.PointA);
            float3 outside = math.abs(local) - collider.BoxHalfExtents;
            float3 outsidePositive = math.max(outside, 0f);

            float signedDistance = math.length(outsidePositive) + math.min(math.max(outside.x, math.max(outside.y, outside.z)), 0f);

            return signedDistance - radius;
        }

        static float BoxContactBorder(float radius)
        {
            return math.max(math.max(0f, radius) * .001f, 0.000001f);
        }

        static bool PushOutFromCapsule(ref float3 position, float boneRadius, TurboColliderData collider)
        {
            float3 line = collider.PointB - collider.PointA;
            float lineLengthSq = math.lengthsq(line);
            float t;

            if (lineLengthSq > 0.0000001f) 
                t = math.saturate(math.dot(position - collider.PointA, line) / lineLengthSq);
            else 
                t = 0f;

            float3 closest = collider.PointA + line * t;
            float3 delta = position - closest;
            float distanceSq = math.lengthsq(delta);
            float combinedRadius = boneRadius + collider.Radius;

            if (distanceSq >= combinedRadius * combinedRadius) return false;

            float3 normal;

            if (distanceSq > 0.0000001f)
            {
                normal = delta * math.rsqrt(distanceSq);
            }
            else if (lineLengthSq > 0.0000001f)
            {
                float3 axis = line * math.rsqrt(lineLengthSq);
                float3 reference;
                if (math.abs(axis.y) < .9f) reference = new float3(0f, 1f, 0f);
                else reference = new float3(1f, 0f, 0f);

                normal = math.normalizesafe(math.cross(axis, reference), new float3(0f, 1f, 0f));
            }
            else
            {
                normal = new float3(0f, 1f, 0f);
            }

            position = closest + normal * combinedRadius;

            return true;
        }

        internal static void ClearCollisionState(ref TurboTailSegmentState state)
        {
            state.CollisionContact = 0;
            state.CollisionRelevancy = -1f;
            state.CollisionBlend = 0f;
            state.CollisionActiveContactBlend = 0f;
            state.CollisionInheritedContact = 0f;
            state.CollisionFramePosition = float3.zero;
            state.CollisionOffset = float3.zero;
        }

        void ApplyMotionInfluence(ref TurboTailRootState root)
        {
            if (RootInput.MotionInfluence == 1f) return;

            float3 baseDelta = RootInput.BasePosition - root.PreviousBasePosition;
            float3 offset = baseDelta * (1f - RootInput.MotionInfluence);
            
            if (RootInput.MotionInfluenceInY < 1f) 
                offset.y = baseDelta.y * (1f - RootInput.MotionInfluenceInY);

            for (int i = 0; i < States.Length; i++)
            {
                TurboTailSegmentState state = States[i];
                state.ProceduralPosition += offset;
                state.PreviousPosition += offset;
                States[i] = state;
            }
        }

        void UpdateArtificialParent(ref TurboTailRootState root)
        {
            root.PreviousPosReferenceRotation = root.PosRefRotation;
            root.PosRefRotation = RootInput.ParentRotation;
            root.PreviousPosition = root.ProceduralPosition;

            root.ProceduralPosition = RootInput.ParentPosition + math.mul(RootInput.ParentRotation, RootInput.ParentScale * RootInput.ParentLocalOffset);

            if (RootInput.HasSafetyTransform != 0)
            {
                root.SafetyPreviousPosRefRotation = root.SafetyPosRefRotation;
                root.SafetyPosRefRotation = RootInput.SafetyRotation;
            }
        }

        static quaternion GetSwingRotation(TurboTailSegmentInput input, quaternion grandParentRotation, quaternion grandParentPreviousRotation)
        {
            quaternion stiff = input.ParentRotation;
            quaternion slitheryReference;

            if (input.Slithery <= 1f)
            {
                slitheryReference = grandParentRotation;
            }
            else
            {
                slitheryReference = LerpUnclamped(grandParentRotation, grandParentPreviousRotation, (input.Slithery - 1f) * 5f);
            }
            
            quaternion slithery = math.mul(slitheryReference, input.ParentLocalRotation);

            if (input.Slithery >= 1f) return slithery;

            if (input.Slithery > math.EPSILON) return LerpUnclamped(stiff, slithery, input.Slithery);
            
            return stiff;
        }

        internal static float3 ApplyForcesToTarget(float3 parentPosition, float3 targetPosition, float boneLength, float3 force)
        {
            if (math.lengthsq(force) == 0f) return targetPosition;

            float3 toBone = targetPosition - parentPosition;
            float3 direction = toBone + force * boneLength;
            float length = math.length(direction);

            float3 normalized;

            if (length > 0.00001f) 
                normalized = direction / length;
            else 
                normalized = float3.zero;

            return parentPosition + normalized * math.length(toBone);
        }

        static void LimitStretching(ref float3 position, float3 parentPosition, float boneLength, float maxStretching, float reactionSpeed, float deltaTime)
        {
            float3 backDirection = parentPosition - position;
            float distance = math.length(backDirection);
            
            if (distance <= math.EPSILON || boneLength <= math.EPSILON) return;

            float tooLongHelper = math.lerp(0.4f, 0f, maxStretching);
            float tooShortHelper = tooLongHelper * 1.5f;
            float maximumDistance = boneLength + boneLength * 2.5f * maxStretching;

            if (distance > maximumDistance)
            {
                if (maxStretching == 0f)
                {
                    position += backDirection * ((distance - boneLength) / distance);
                }
                else
                {
                    float3 limitedPosition = parentPosition - backDirection / distance * maximumDistance;
                    
                    float limitValue = 1f - maximumDistance / distance + tooLongHelper;
                    limitValue = math.min(0.99f, math.max(0f, limitValue));
                    if (reactionSpeed < 0.5f) limitValue *= deltaTime * (10f + reactionSpeed * 30f);
                
                    position = math.lerp(position, limitedPosition, limitValue);
                }
            }
            else
            {
                maximumDistance = boneLength + boneLength * 1.1f * maxStretching;
                
                if (distance >= maximumDistance) return;

                float3 limitedPosition = position + backDirection * ((distance - boneLength) / distance);
                
                if (maxStretching == 0f)
                    position = limitedPosition;
                else
                {
                    float limitValue = 1f - distance / maximumDistance + tooShortHelper;
                    position = math.lerp(position, limitedPosition, math.saturate(limitValue));
                }
            }
        }

        static float3 LimitAngle(float3 position, float3 parentPosition, TurboTailSegmentInput input, float angleLimit, float3 angleLimitAxis, float2 limitAxisRange, float limitSmoothing, float deltaTime)
        {
            float3 targetDirection = position - parentPosition;
            float targetDistance = math.length(targetDirection);
            float3 sourceDirection = math.mul(input.ParentRotation, input.LocalPosition);

            if (targetDistance <= math.EPSILON || math.lengthsq(sourceDirection) <= math.EPSILON) return position;

            quaternion targetWorldRotation = math.mul(FromToRotation(sourceDirection, targetDirection), input.ParentRotation);
            quaternion targetLocalRotation = math.mul(math.inverse(input.ParentRotation), targetWorldRotation);
            quaternion limitedLocalRotation = targetLocalRotation;

            float correctionAngle = 0f;

            if (math.lengthsq(angleLimitAxis) <= math.EPSILON)
            {
                float angle = Angle(input.SourceLocalRotation, targetLocalRotation);
                if (angle <= angleLimit) return position;

                limitedLocalRotation = LerpUnclamped(input.SourceLocalRotation, targetLocalRotation, math.saturate(angleLimit / math.max(angle, 0.0001f)));
                correctionAngle = Angle(targetLocalRotation, limitedLocalRotation);
            }
            else
            {
                float3 axis = math.normalizesafe(angleLimitAxis, new float3(1f, 0f, 0f));
                quaternion relative = math.mul(math.inverse(input.SourceLocalRotation), targetLocalRotation);
                float3 projected = axis * math.dot(relative.value.xyz, axis);
                quaternion twist = new quaternion(new float4(projected, relative.value.w));

                twist = new quaternion(math.normalizesafe(twist.value, quaternion.identity.value));

                float signedTwist = math.degrees(2f * math.atan2(math.dot(twist.value.xyz, axis), twist.value.w));

                if (signedTwist > 180f) 
                    signedTwist -= 360f;
                else 
                    if (signedTwist < -180f) signedTwist += 360f;

                float minLimit = -angleLimit;
                float maxLimit = angleLimit;

                if (math.abs(limitAxisRange.x - limitAxisRange.y) > math.EPSILON)
                {
                    minLimit += limitAxisRange.x;
                    maxLimit += limitAxisRange.y;
                }

                float clampedTwist = math.clamp(signedTwist, minLimit, maxLimit);
                if (math.abs(signedTwist - clampedTwist) <= math.EPSILON) return position;

                quaternion swing = math.mul(relative, math.inverse(twist));

                limitedLocalRotation = math.mul(input.SourceLocalRotation, math.mul(swing, quaternion.AxisAngle(axis, math.radians(clampedTwist))));
                correctionAngle = Angle(targetLocalRotation, limitedLocalRotation);
            }

            if (limitSmoothing > math.EPSILON)
            {
                float speed = math.lerp(55f, 15f, math.saturate(limitSmoothing));
                float weight = math.saturate(correctionAngle / math.max(angleLimit, 0.0001f));
                limitedLocalRotation = LerpUnclamped(targetLocalRotation, limitedLocalRotation, math.saturate(deltaTime * speed * weight));
            }

            quaternion limitedWorldRotation = math.mul(input.ParentRotation, limitedLocalRotation);
            float3 limitedDirection = math.mul(limitedWorldRotation, input.LocalPosition);
            float limitedLength = math.length(limitedDirection);

            if (limitedLength <= math.EPSILON) return position;

            // Preserve the already-solved distance, angle limiting should not fight stretching
            return parentPosition + limitedDirection * (targetDistance / limitedLength);
        }

        static float Angle(quaternion a, quaternion b)
        {
            float dot = math.clamp(math.abs(math.dot(a.value, b.value)), -1f, 1f);
            return math.degrees(2f * math.acos(dot));
        }

        static float3 SmoothDamp(float3 current, float3 target, ref float3 velocity, float smoothTime, float deltaTime)
        {
            smoothTime = math.max(0.0001f, smoothTime);
            
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exponential = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            
            float3 change = current - target;
            float3 originalTarget = target;
            float3 temporary = (velocity + omega * change) * deltaTime;
            
            velocity = (velocity - omega * temporary) * exponential;
            
            float3 output = target + (change + temporary) * exponential;

            if (math.dot(originalTarget - current, output - originalTarget) > 0f)
            {
                output = originalTarget;
                velocity = float3.zero;
            }

            return output;
        }

        static quaternion SmoothDampRotation( quaternion current, quaternion target, ref quaternion velocity, float smoothTime, float deltaTime)
        {
            if (math.dot(current.value, target.value) < 0f) target.value = -target.value;

            float4 velocityValue = velocity.value;

            float4 smoothed = new float4
                (
                SmoothDampScalar(current.value.x, target.value.x, ref velocityValue.x, smoothTime, deltaTime),
                SmoothDampScalar(current.value.y, target.value.y, ref velocityValue.y, smoothTime, deltaTime),
                SmoothDampScalar(current.value.z, target.value.z, ref velocityValue.z, smoothTime, deltaTime),
                SmoothDampScalar(current.value.w, target.value.w, ref velocityValue.w, smoothTime, deltaTime)
                );

            smoothed = math.normalizesafe(smoothed, quaternion.identity.value);
            velocityValue -= smoothed * math.dot(velocityValue, smoothed);
            velocity.value = velocityValue;
            
            return new quaternion(smoothed);
        }

        static float SmoothDampScalar(float current, float target, ref float velocity, float smoothTime, float deltaTime)
        {
            smoothTime = math.max(0.0001f, smoothTime);
            
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exponential = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            float change = current - target;

            float temporary = (velocity + omega * change) * deltaTime;
            velocity = (velocity - omega * temporary) * exponential;
            
            float output = target + (change + temporary) * exponential;

            if ((target - current > 0f) == (output > target))
            {
                output = target;
                velocity = 0f;
            }

            return output;
        }

        static quaternion LerpUnclamped(quaternion from, quaternion to, float value)
        {
            if (math.dot(from.value, to.value) < 0f) to.value = -to.value;

            return new quaternion(math.normalizesafe(math.lerp(from.value, to.value, value), quaternion.identity.value));
        }

        static quaternion FromToRotation(float3 from, float3 to)
        {
            float3 fromNormal = math.normalizesafe(from);
            float3 toNormal = math.normalizesafe(to);

            if (math.lengthsq(fromNormal) <= math.EPSILON || math.lengthsq(toNormal) <= math.EPSILON) return quaternion.identity;

            float dot = math.clamp(math.dot(fromNormal, toNormal), -1f, 1f);
            if (dot > 0.999999f) return quaternion.identity;

            if (dot < -0.999999f)
            {
                float3 axis = math.cross(fromNormal, new float3(1f, 0f, 0f));
                if (math.lengthsq(axis) < 0.000001f) axis = math.cross(fromNormal, new float3(0f, 1f, 0f));
                return quaternion.AxisAngle(math.normalize(axis), math.PI);
            }

            float3 cross = math.cross(fromNormal, toNormal);

            return new quaternion(math.normalize(new float4(cross, 1f + dot)));
        }
    }

}
#endif
