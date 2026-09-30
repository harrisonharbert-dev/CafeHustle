using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        struct CapsuleAlongCollisionState
        {
            public TailSegment bone;
            public float radius;
            public float minLength;
            public float maxLength;
            public Vector3 lengthDirection;
            public Vector3 predictedPosition;
        }

        struct CapsuleAlongCollider
        {
            public Vector3 start;
            public Vector3 end;
            public float radius;
            public Bounds bounds;
            public FImp_ColliderData_Base pointFallback;
            public Collider source;
        }

        struct CapsuleAlongContactPlane
        {
            public Vector3 normal;
            public float distance;
            public Collider collider;
        }

        struct CapsuleAlongContactMemory
        {
            public TailSegment bone;
            public Collider collider;
            public Vector3 localNormal;
        }

        readonly List<CapsuleAlongCollider> capsuleAlongColliders = new List<CapsuleAlongCollider>();
        readonly List<CapsuleAlongContactPlane> capsuleAlongPlanes = new List<CapsuleAlongContactPlane>();
        readonly List<CapsuleAlongContactPlane> capsuleAlongNormalPlanes = new List<CapsuleAlongContactPlane>(2);
        readonly List<CapsuleAlongContactMemory> capsuleAlongContactMemory = new List<CapsuleAlongContactMemory>();
        readonly List<CapsuleAlongContactMemory> nextCapsuleAlongContactMemory = new List<CapsuleAlongContactMemory>();

        void ClearCapsuleAlongContactMemory()
        {
            capsuleAlongContactMemory.Clear();
            nextCapsuleAlongContactMemory.Clear();

            if (TailSegments != null)
                for (int i = 0; i < TailSegments.Count; i++)
                    TailSegments[i].CapsulePushPoseOffset = Vector3.zero;

            if (GhostChild != null) GhostChild.CapsulePushPoseOffset = Vector3.zero;
        }

        void ClearCollisionContactMemory()
        {
            ClearCapsuleAlongContactMemory();

            if (TailSegments != null)
            {
                for (int i = 0; i < TailSegments.Count; i++)
                {
                    TailSegments[i].CollisionContactBlend = 0f;
                    TailSegments[i].CollisionActiveContactBlend = 0f;
                    TailSegments[i].CollisionInheritedContact = 0f;
                    TailSegments[i].CollisionFramePosition = Vector3.zero;
                }
            }

            if (GhostChild != null)
            {
                GhostChild.CollisionContactBlend = 0f;
                GhostChild.CollisionActiveContactBlend = 0f;
                GhostChild.CollisionInheritedContact = 0f;
                GhostChild.CollisionFramePosition = Vector3.zero;
            }
        }

        void TailCalculations_ProcessCapsuleAlong(bool postProcess)
        {
            if (!UseSelectiveCollisionMemory) ClearCollisionContactMemory();

            float stretching = Mathf.Clamp01(MaxStretching);
            PrepareCapsuleAlongColliders();
            nextCapsuleAlongContactMemory.Clear();

            TailSegment bone = TailSegments[_tc_startPositionI];

            while (bone != GhostChild)
            {
                if (DetachChildren) 
                    TailSegment_PrepareRotationDetached(bone);
                else 
                    TailSegment_PrepareRotation(bone);

                TailSegment_BaseSwingProcessing(bone);
                TailCalculations_SegmentPreProcessingStack(bone);

                if (postProcess) TailCalculations_SegmentPostProcessing(bone);

                if (!bone.CollisionContactFlag && bone.CollisionContactRelevancy > 0f)
                    bone.CollisionContactRelevancy -= justDelta;

                bone.CollisionContactFlag = false;

                Vector3 offset = bone.ProceduralPosition - bone.ParentBone.ProceduralPosition;
                float length = offset.magnitude;

                Vector3 direction;

                if (length > 1e-6f)
                {
                    direction = offset / length;
                }
                else
                {
                    direction = bone.ParentBone.PosRefRotation * bone.BoneDimensionsScaled;
                }

                float minLength;
                float maxLength;

                if (stretching >= 1f)
                {
                    minLength = 0f;
                    maxLength = float.PositiveInfinity;
                }
                else
                {
                    if (stretching == 0f)
                    {
                        minLength = bone.BoneLengthScaled;
                    }
                    else
                    {
                        minLength = Mathf.Min(length, bone.BoneLengthScaled);
                    }

                    maxLength = bone.BoneLengthScaled * (1f + 2.5f * stretching);
                }

                if (direction.sqrMagnitude < 1e-12f) direction = Vector3.forward;

                CapsuleAlongCollisionState state = new CapsuleAlongCollisionState
                {
                    bone = bone,
                    radius = Mathf.Max(0f, bone.GetRadiusScaled()),
                    minLength = minLength,
                    maxLength = maxLength,
                    lengthDirection = direction.normalized,
                    predictedPosition = bone.ProceduralPosition
                };

                if (capsuleAlongColliders.Count > 0) SolveCapsuleAlongPosition(state);

                TailSegment_PreRotationPositionBlend(bone);
                bone = bone.ChildBone;
            }

            capsuleAlongContactMemory.Clear();

            capsuleAlongContactMemory.AddRange(nextCapsuleAlongContactMemory);
        }

        void PrepareCapsuleAlongColliders()
        {
            capsuleAlongColliders.Clear();

            for (int i = 0; i < CollidersDataToCheck.Count; i++)
            {
                FImp_ColliderData_Base data = CollidersDataToCheck[i];
                Collider collider = data.Collider;

                if (collider == null) continue;

                Transform colliderTransform = collider.transform;
                Vector3 scale = colliderTransform.lossyScale;
                scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                CapsuleAlongCollider shape = new CapsuleAlongCollider { source = collider };

                SphereCollider sphere = collider as SphereCollider;
                CapsuleCollider capsule = collider as CapsuleCollider;
                CharacterController character = collider as CharacterController;

                if (sphere)
                {
                    shape.start = shape.end = colliderTransform.TransformPoint(sphere.center);
                    shape.radius = Mathf.Max(0f, sphere.radius) * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
                }
                else if (capsule || character)
                {
                    int axis;
                    float radius;
                    float height;
                    Vector3 localCenter;

                    if (capsule)
                    {
                        axis = capsule.direction;
                        radius = capsule.radius;
                        height = capsule.height;
                        localCenter = capsule.center;
                    }
                    else
                    {
                        axis = 1;
                        radius = character.radius;
                        height = character.height;
                        localCenter = character.center;
                    }

                    Vector3 direction;

                    if (axis == 0)
                    {
                        direction = Vector3.right;
                    }
                    else if (axis == 1)
                    {
                        direction = Vector3.up;
                    }
                    else
                    {
                        direction = Vector3.forward;
                    }

                    float axisScale = scale[axis];
                    float radiusScale = Mathf.Max(scale[(axis + 1) % 3], scale[(axis + 2) % 3]);
                    shape.radius = Mathf.Max(0f, radius) * radiusScale;
                    
                    float halfLength = Mathf.Max(0f, height * axisScale * 0.5f - shape.radius);
                    
                    Vector3 center = colliderTransform.TransformPoint(localCenter);
                    Vector3 axisOffset = colliderTransform.rotation * direction * halfLength;
                    
                    shape.start = center - axisOffset;
                    shape.end = center + axisOffset;
                }
                else
                {
                    shape.pointFallback = data;
                }

                shape.bounds = new Bounds();

                shape.bounds.SetMinMax(Vector3.Min(shape.start, shape.end) - Vector3.one * shape.radius, Vector3.Max(shape.start, shape.end) + Vector3.one * shape.radius);

                capsuleAlongColliders.Add(shape);
            }
        }

        void SolveCapsuleAlongPosition(CapsuleAlongCollisionState state)
        {
            TailSegment bone = state.bone;

            bool useCollisionMemory = UsesSelectiveCollisionMemory(bone);
            float collisionDamping = GetCollisionDampingFor(bone);

            bool hasTailParent = bone.Index > 0;
            bool hasSimulatedParent = hasTailParent && bone.ParentBone.Index >= _tc_startPositionI;
            
            Vector3 parent = bone.ParentBone.ProceduralPosition;
            Vector3 wanted = ClampCapsuleAlongLength(state.predictedPosition - parent, state.minLength, state.maxLength, state.lengthDirection);
            
            float reach;
            float parentRadius;

            if (hasTailParent)
            {
                reach = wanted.magnitude;
                parentRadius = Mathf.Max(0f, bone.ParentBone.GetRadiusScaled());
            }
            else
            {
                reach = state.maxLength;
                parentRadius = state.radius;
            }

            float thickness = Mathf.Max(state.radius, parentRadius);
            float reachSq = (reach + thickness + 0.0001f) * (reach + thickness + 0.0001f);
            
            capsuleAlongPlanes.Clear();

            for (int c = 0; c < capsuleAlongColliders.Count; c++)
            {
                CapsuleAlongCollider collider = capsuleAlongColliders[c];
                Vector3 normal;

                if (collider.pointFallback != null)
                {
                    Vector3 point = parent + wanted;
                    
                    if (!collider.pointFallback.PushIfInside(ref point, state.radius, Vector3.zero)) continue;
                    normal = (point - parent - wanted).normalized;

                    capsuleAlongPlanes.Add(new CapsuleAlongContactPlane { normal = normal, distance = Vector3.Dot(point - parent, normal), collider = collider.source });
                    
                    MarkCollisionContact(bone);

                    continue;
                }

                if (collider.bounds.SqrDistance(parent) > reachSq) continue;

                Vector3 start;

                if (hasTailParent)
                {
                    start = parent;
                }
                else
                {
                    start = parent + wanted;
                }

                float gap = CapsuleAlongSurfaceGap(start, parent + wanted, parentRadius, state.radius, collider, out normal);
                float margin = Mathf.Max(state.radius * 0.02f, 0.00001f);

                if (gap <= margin)
                {
                    MarkCollisionContact(bone);
                    if (hasSimulatedParent) MarkCollisionContact(bone.ParentBone);
                }

                if (useCollisionMemory)
                {
                    for (int m = 0; m < capsuleAlongContactMemory.Count; m++)
                    {
                        CapsuleAlongContactMemory memory = capsuleAlongContactMemory[m];
                        
                        if (memory.bone != bone || memory.collider != collider.source) continue;
                        
                        Vector3 rememberedNormal = collider.source.transform.TransformDirection(memory.localNormal).normalized;
                        
                        if (collisionDamping >= 1f)
                        {
                            normal = rememberedNormal;
                        }
                        else
                        {
                            normal = Vector3.Slerp(normal, rememberedNormal, collisionDamping).normalized;
                        }

                        break;
                    }
                }

                float skin = Mathf.Max(collider.radius, 0.001f) * 0.0001f;

                if (hasTailParent && collider.start == collider.end && wanted.sqrMagnitude > 1e-12f)
                {
                    Vector3 away = parent - collider.start;

                    float distance = away.magnitude;
                    float sideLength = wanted.magnitude;

                    if (distance > 1e-6f)
                    {
                        float radius = parentRadius + collider.radius + skin;
                        float taper = (state.radius - parentRadius) / sideLength;
                        float radial = distance * distance - radius * radius;

                        bool tangentContact = Mathf.Abs(taper) < 1f && Mathf.Max(0f, radial) <= sideLength * sideLength * (1f - taper * taper);
                        float cosine;

                        if (tangentContact)
                        {
                            cosine = (SmoothCapsuleAlongTangent(Mathf.Max(0f, radial), radius) * Mathf.Sqrt(1f - taper * taper) - radius * taper) / distance;
                        }
                        else
                        {
                            cosine = (radial + (1f - taper * taper) * sideLength * sideLength - 2f * radius * taper * sideLength) / (2f * distance * sideLength);
                        }

                        capsuleAlongPlanes.Add(new CapsuleAlongContactPlane { normal = away / distance, distance = -cosine * sideLength, collider = collider.source });
                        
                        continue;
                    }
                }

                if (hasTailParent)
                {
                    capsuleAlongNormalPlanes.Clear();
                    
                    AddCapsuleAlongNormalPlane(parent - collider.start, parentRadius + collider.radius + skin);
                    
                    if (collider.start != collider.end)
                        AddCapsuleAlongNormalPlane(parent - collider.end, parentRadius + collider.radius + skin);
                    
                    normal = SolveCapsuleAlongPlanes(normal, 1f, 1f, normal, capsuleAlongNormalPlanes);
                }

                float support = Mathf.Max(Vector3.Dot(collider.start - parent, normal), Vector3.Dot(collider.end - parent, normal));
                
                capsuleAlongPlanes.Add(new CapsuleAlongContactPlane { normal = normal, distance = support + collider.radius + state.radius + skin, collider = collider.source });
            }

            float solvedLength = wanted.magnitude;

            Vector3 solved;

            if (hasTailParent)
            {
                solved = SolveCapsuleAlongPlanes(wanted, solvedLength, solvedLength, state.lengthDirection, capsuleAlongPlanes);
            }
            else
            {
                solved = SolveCapsuleAlongPlanes(wanted, state.minLength, state.maxLength, state.lengthDirection, capsuleAlongPlanes);
            }

            bone.ProceduralPosition = parent + solved;

            Vector3 velocity = state.predictedPosition - bone.PreviousPosition;

            if ((solved - wanted).sqrMagnitude > 1e-12f)
            {
                if (useCollisionMemory)
                    bone.CapsulePushPoseOffset = Vector3.ClampMagnitude(bone.CapsulePushPoseOffset + bone.ProceduralPosition - state.predictedPosition, Mathf.Max(bone.BoneLengthScaled, state.radius));

                capsuleAlongNormalPlanes.Clear();

                for (int p = 0; p < capsuleAlongPlanes.Count; p++)
                    if (Vector3.Dot(solved, capsuleAlongPlanes[p].normal) <= capsuleAlongPlanes[p].distance + 0.0001f)
                        capsuleAlongNormalPlanes.Add(new CapsuleAlongContactPlane { normal = capsuleAlongPlanes[p].normal });

                bone.PreviousPosition = bone.ProceduralPosition - SolveCapsuleAlongPlanes(velocity, 0f, float.PositiveInfinity, state.lengthDirection, capsuleAlongNormalPlanes);
                
                bone.VelocityHelper = SolveCapsuleAlongPlanes(bone.VelocityHelper, 0f, float.PositiveInfinity, state.lengthDirection, capsuleAlongNormalPlanes);
            }

            for (int c = 0; c < capsuleAlongColliders.Count; c++)
            {
                CapsuleAlongCollider collider = capsuleAlongColliders[c];

                if (collider.pointFallback != null || collider.bounds.SqrDistance(parent) > reachSq) continue;
                
                Vector3 normal;
                Vector3 start;

                if (hasTailParent)
                {
                    start = parent;
                }
                else
                {
                    start = bone.ProceduralPosition;
                }

                float gap = CapsuleAlongSurfaceGap(start, bone.ProceduralPosition, parentRadius, state.radius, collider, out normal);
                
                bool active = gap <= Mathf.Max(state.radius * 0.02f, 0.00001f);

                for (int p = 0; p < capsuleAlongPlanes.Count && !active; p++)
                {
                    active = capsuleAlongPlanes[p].collider == collider.source
                        && Vector3.Dot(wanted, capsuleAlongPlanes[p].normal) < capsuleAlongPlanes[p].distance
                        && Vector3.Dot(solved, capsuleAlongPlanes[p].normal) <= capsuleAlongPlanes[p].distance + 0.0001f;

                    if (!active) break;
                }

                if (!active) continue;

                MarkCollisionContact(bone);

                if (hasSimulatedParent) MarkCollisionContact(bone.ParentBone);

                if (useCollisionMemory)
                    nextCapsuleAlongContactMemory.Add(new CapsuleAlongContactMemory
                    {
                        bone = bone,
                        collider = collider.source,
                        localNormal = collider.source.transform.InverseTransformDirection(normal)
                    });
            }
        }

        void AddCapsuleAlongNormalPlane(Vector3 offset, float radius)
        {
            float length = offset.magnitude;

            if (length > 1e-8f)
                capsuleAlongNormalPlanes.Add(new CapsuleAlongContactPlane { normal = offset / length, distance = radius / length });
        }

        static float CapsuleAlongSurfaceGap(Vector3 a, Vector3 b, float radiusA, float radiusB, CapsuleAlongCollider collider, out Vector3 normal)
        {
            float s, t;
            
            ClosestCapsuleAlongParameters(a, b, radiusA, radiusB, collider.start, collider.end, out s, out t);
            
            Vector3 separation = Vector3.LerpUnclamped(a, b, s) - Vector3.LerpUnclamped(collider.start, collider.end, t);
            
            float distance = separation.magnitude;

            if (distance > 1e-6f)
            {
                normal = separation / distance;
            }
            else
            {
                normal = CapsuleAlongPerpendicular(b - a);
            }
            
            return distance - Mathf.LerpUnclamped(radiusA, radiusB, s) - collider.radius;
        }

        static void ClosestCapsuleAlongParameters(Vector3 a, Vector3 b, float radiusA, float radiusB, Vector3 c, Vector3 d, out float s, out float t)
        {
            Vector3 side = b - a;
            Vector3 axis = d - c;

            float deltaRadius = radiusB - radiusA;
            s = MinimumTaperedDistance(a - c, side, deltaRadius, 0f, 1f);

            float axisLengthSq = axis.sqrMagnitude;
            t = 0f;

            if (axisLengthSq <= 1e-12f) return;

            float best = TaperedCapsuleAlongSeparation(a, side, radiusA, deltaRadius, c, axis, axisLengthSq, s, out t);
            float candidate = MinimumTaperedDistance(a - d, side, deltaRadius, 0f, 1f);
            float candidateT;
            float separation = TaperedCapsuleAlongSeparation(a, side, radiusA, deltaRadius, c, axis, axisLengthSq, candidate, out candidateT);
            
            if (separation < best) { best = separation; s = candidate; t = candidateT; }

            float startProjection = Vector3.Dot(a - c, axis) / axisLengthSq;
            float projectionStep = Vector3.Dot(side, axis) / axisLengthSq;
            float lower = 0f;
            float upper = 1f;

            if (Mathf.Abs(projectionStep) < 1e-8f)
            {
                if (startProjection < 0f || startProjection > 1f) return;
            }
            else
            {
                float first = -startProjection / projectionStep;
                float last = (1f - startProjection) / projectionStep;

                lower = Mathf.Max(0f, Mathf.Min(first, last));
                upper = Mathf.Min(1f, Mathf.Max(first, last));

                if (lower > upper) return;
            }

            candidate = MinimumTaperedDistance(a - c - axis * startProjection, side - axis * projectionStep, deltaRadius, lower, upper);
            
            separation = TaperedCapsuleAlongSeparation(a, side, radiusA, deltaRadius, c, axis, axisLengthSq, candidate, out candidateT);
            
            if (separation < best) { s = candidate; t = candidateT; }
        }

        static float TaperedCapsuleAlongSeparation(Vector3 start, Vector3 side, float radius, float deltaRadius, Vector3 center, Vector3 axis, float axisLengthSq, float s, out float t)
        {
            Vector3 point = start + side * s;

            t = Mathf.Clamp01(Vector3.Dot(point - center, axis) / axisLengthSq);

            return (point - center - axis * t).magnitude - (radius + deltaRadius * s);
        }

        static float MinimumTaperedDistance(Vector3 offset, Vector3 direction, float deltaRadius, float lower, float upper)
        {
            float lengthSq = direction.sqrMagnitude;

            if (lengthSq <= 1e-12f || deltaRadius * deltaRadius >= lengthSq)
            {
                if (deltaRadius > 0f)
                {
                    return upper;
                }
                else
                {
                    return lower;
                }
            }

            float dot = Vector3.Dot(offset, direction);
            float perpendicularSq = Mathf.Max(0f, offset.sqrMagnitude - dot * dot / lengthSq);
            
            float s = -dot / lengthSq + deltaRadius * Mathf.Sqrt(perpendicularSq / (lengthSq * (lengthSq - deltaRadius * deltaRadius)));
            
            return Mathf.Clamp(s, lower, upper);
        }

        static Vector3 ClampCapsuleAlongLength(Vector3 value, float minimum, float maximum, Vector3 fallback)
        {
            float length = value.magnitude;

            float target = Mathf.Clamp(length, minimum, maximum);

            if (target == length) return value;

            if (length > 1e-8f)
            {
                return (value / length) * target;
            }
            else
            {
                return fallback * target;
            }
        }

        static Vector3 CapsuleAlongPerpendicular(Vector3 direction)
        {
            if (direction.sqrMagnitude < 1e-12f) return Vector3.up;

            direction.Normalize();

            if (Mathf.Abs(direction.y) < 0.9f)
            {
                return Vector3.Cross(direction, Vector3.up).normalized;
            }
            else
            {
                return Vector3.Cross(direction, Vector3.right).normalized;
            }
        }

        static Vector3 SolveCapsuleAlongPlanes(Vector3 wanted, float minimum, float maximum, Vector3 fallback, List<CapsuleAlongContactPlane> planes)
        {
            Vector3 best = ClampCapsuleAlongLength(wanted, minimum, maximum, fallback);
            float tolerance = 1e-6f * Mathf.Max(1f, minimum);
            float bestViolation = CapsuleAlongPlaneViolation(best, planes);

            if (bestViolation <= tolerance) return best;

            float bestDistance = (best - wanted).sqrMagnitude;
            float minSq = minimum * minimum;
            float maxSq = maximum * maximum;

            for (int i = 0; i < planes.Count; i++)
            {
                CapsuleAlongContactPlane a = planes[i];
                Vector3 origin = a.normal * a.distance;
                float originSq = origin.sqrMagnitude;

                if (originSq <= maxSq)
                {
                    Vector3 tangent = wanted - a.normal * Vector3.Dot(wanted, a.normal);
                    
                    float lower = Mathf.Sqrt(Mathf.Max(0f, minSq - originSq));
                    float upper = Mathf.Sqrt(Mathf.Max(0f, maxSq - originSq));
                    
                    Vector3 projected = origin + ClampCapsuleAlongLength(tangent, lower, upper, CapsuleAlongPerpendicular(a.normal));
                    
                    ConsiderCapsuleAlongCandidate(projected, wanted, minimum, maximum, tolerance, planes, ref best, ref bestViolation, ref bestDistance);
                    
                    if (tangent.sqrMagnitude < 1e-12f && lower > 0f)
                        ConsiderCapsuleAlongCandidate(origin - CapsuleAlongPerpendicular(a.normal) * lower, wanted, minimum, maximum,
                            tolerance, planes, ref best, ref bestViolation, ref bestDistance);
                }

                for (int j = 0; j < i; j++)
                {
                    CapsuleAlongContactPlane b = planes[j];
                    Vector3 cross = Vector3.Cross(a.normal, b.normal);
                    
                    float determinant = cross.sqrMagnitude;
                    
                    if (determinant < 1e-12f) continue;
                    
                    float dot = Vector3.Dot(a.normal, b.normal);
                    origin = (a.normal * (a.distance - b.distance * dot) + b.normal * (b.distance - a.distance * dot)) / determinant;
                    originSq = origin.sqrMagnitude;

                    Vector3 line = cross / Mathf.Sqrt(determinant);

                    if (originSq <= maxSq)
                    {
                        float limit = Mathf.Sqrt(Mathf.Max(0f, maxSq - originSq));
                        float along = Mathf.Clamp(Vector3.Dot(wanted - origin, line), -limit, limit);
                        float lower = Mathf.Sqrt(Mathf.Max(0f, minSq - originSq));
                        
                        if (Mathf.Abs(along) < lower)
                        {
                            ConsiderCapsuleAlongCandidate(origin + line * lower, wanted, minimum, maximum, tolerance,
                                planes, ref best, ref bestViolation, ref bestDistance);
                            ConsiderCapsuleAlongCandidate(origin - line * lower, wanted, minimum, maximum, tolerance,
                                planes, ref best, ref bestViolation, ref bestDistance);
                        }
                        else
                        {
                            ConsiderCapsuleAlongCandidate(origin + line * along, wanted, minimum, maximum, tolerance,
                                planes, ref best, ref bestViolation, ref bestDistance);
                        }
                    }

                    for (int k = 0; k < j; k++)
                    {
                        CapsuleAlongContactPlane c = planes[k];
                        float volume = Vector3.Dot(cross, c.normal);

                        if (Mathf.Abs(volume) < 1e-8f) continue;

                        Vector3 vertex = (Vector3.Cross(b.normal, c.normal) * a.distance + Vector3.Cross(c.normal, a.normal) * b.distance + cross * c.distance) / volume;
                        
                        ConsiderCapsuleAlongCandidate(vertex, wanted, minimum, maximum, tolerance, planes, ref best, ref bestViolation, ref bestDistance);
                    }
                }
            }

            return best;
        }

        static float CapsuleAlongPlaneViolation(Vector3 value, List<CapsuleAlongContactPlane> planes)
        {
            float violation = 0f;
            
            for (int i = 0; i < planes.Count; i++) violation = Mathf.Max(violation, planes[i].distance - Vector3.Dot(value, planes[i].normal));
            
            return violation;
        }

        static float SmoothCapsuleAlongTangent(float squared, float scale)
        {
            float weight = Mathf.Clamp01(squared / Mathf.Max(scale * scale * 0.01f, 1e-12f));
            
            return Mathf.Sqrt(squared) * weight * weight * (3f - 2f * weight);
        }

        static void ConsiderCapsuleAlongCandidate(Vector3 candidate, Vector3 wanted, float minimum, float maximum,
            float tolerance, List<CapsuleAlongContactPlane> planes, ref Vector3 best, ref float bestViolation, ref float bestDistance)
        {
            float length = candidate.magnitude;
            if (float.IsNaN(length) || float.IsInfinity(length)
                || length < minimum - tolerance || length > maximum + tolerance) return;

            float violation = CapsuleAlongPlaneViolation(candidate, planes);
            float distance = (candidate - wanted).sqrMagnitude;
            bool feasible = violation <= tolerance;
            bool bestFeasible = bestViolation <= tolerance;
            if ((feasible && !bestFeasible)
                || (feasible && bestFeasible && distance < bestDistance)
                || (!feasible && !bestFeasible
                    && (violation < bestViolation || (violation == bestViolation && distance < bestDistance))))
            {
                best = candidate;
                bestViolation = violation;
                bestDistance = distance;
            }
        }
    }
}
