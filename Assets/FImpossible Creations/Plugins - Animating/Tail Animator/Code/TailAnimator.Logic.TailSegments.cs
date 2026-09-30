using UnityEngine;

namespace FIMSpace.FTail
{
    /// <summary>
    /// FC: Calculations focused on single tail bone segments operations
    /// </summary>
    public partial class TailAnimator2
    {
        // Motion controll variables
        float _sg_springVelo = 0.5f;
        //float _sg_curl = 0.5f;
        float _sg_curly = 0.5f;

        // Motion calculation variables
        Vector3 _sg_push;
        Vector3 _sg_targetPos;
        Vector3 _sg_targetChildWorldPosInParentFront;
        Vector3 _sg_dirToTargetParentFront;
        Quaternion _sg_orientation;
        float _sg_slitFactor = 0.5f;

        Vector3 TailSegment_GetLocalPosition( TailSegment child, bool initial = false )
        {
            // A parentless start bone uses a virtual parent behind its source pose, not its world position as a local offset.
            if( IncludeMode == EBoneInclusion.Full && child.Index == 0 && child.ParentBone.transform == child.transform ) return -child.ParentBone.LocalOffset;
            return initial ? child.InitialLocalPosition : child.LastKeyframeLocalPosition;
        }

        void TailSegment_PrepareBoneLength(TailSegment child)
        {
            // Remember bone scale referenced from initial position
            child.BoneDimensionsScaled = Vector3.Scale( child.ParentBone.transform.lossyScale * child.LengthMultiplier, TailSegment_GetLocalPosition( child ) );
            child.BoneLengthScaled = child.BoneDimensionsScaled.magnitude; //(child.ParentBone.transform.position - child.transform.position).magnitude * child.LengthMultiplier;
        }

        /// <summary>
        /// Preparing motion parameters for individual segment settings
        /// </summary>
        void TailSegment_PrepareMotionParameters(TailSegment child)
        {
            //// Non-Slithery
            //_sg_curly = Mathf.LerpUnclamped(0.6f, 0.15f, child.Curling);
            //_sg_springVelo = Mathf.LerpUnclamped(0.65f, 0.9f, child.Springiness);

            //// Slithery Blend
            //_sg_curly = Mathf.Lerp(_sg_curly, Mathf.LerpUnclamped(0.99f, 0.135f, child.Curling), child.Slithery);
            //_sg_springVelo = Mathf.Lerp(_sg_springVelo, Mathf.LerpUnclamped(0.0f, 0.8f, child.Springiness), child.Slithery);

            // Non-Slithery
            _sg_curly = Mathf.LerpUnclamped(0.5f, 0.125f, child.Curling);
            _sg_springVelo = Mathf.LerpUnclamped(0.65f, 0.9f, child.Springiness);

            // Slithery Blend
            _sg_curly = Mathf.Lerp(_sg_curly, Mathf.LerpUnclamped(0.95f, 0.135f, child.Curling), child.Slithery);
            _sg_springVelo = Mathf.Lerp(_sg_springVelo, Mathf.LerpUnclamped(0.1f, 0.85f, child.Springiness), child.Slithery);
        }


        /// <summary>
        /// Preparing segment positioning parameters
        /// </summary>
        void TailSegment_PrepareVelocity(TailSegment child)
        {
            child.CollisionFramePosition = UsesSelectiveCollisionMemory(child) ? child.ProceduralPosition : Vector3.zero;

            // Velocity change check
            _sg_push = (child.ProceduralPosition - child.PreviousPosition);

            // Remember actual position after using previous position memory
            child.PreviousPosition = child.ProceduralPosition;

            // Collision slippery modify
            float swinginess = _sg_springVelo;
            if (child.CollisionContactFlag) swinginess *= child.Slippery;

            // Tail Motion velocity motion base calculations
            child.ProceduralPosition += _sg_push * swinginess;

            // Remember previous push for sustain feature
            child.PreviousPush = _sg_push;
        }


        void TailSegment_PrepareRotation(TailSegment child)
        {
            float contactResponse = TailSegment_PrepareCollisionResponse(child);
            _sg_targetChildWorldPosInParentFront = child.ParentBone.ProceduralPosition + TailSegment_GetSwingRotation(child, _sg_slitFactor) * child.BoneDimensionsScaled; // Local offset scaled with custom rotation
            _sg_targetChildWorldPosInParentFront = CalculateGravityPositionOffsetForSegment(child, _sg_targetChildWorldPosInParentFront);
            TailSegment_ApplyCollisionMemory(child, contactResponse);
        }

        void TailSegment_PrepareRotationDetached(TailSegment child)
        {
            float contactResponse = TailSegment_PrepareCollisionResponse(child);
            _sg_targetChildWorldPosInParentFront = child.ParentBone.ProceduralPosition + TailSegment_GetSwingRotationDetached(child, _sg_slitFactor) * child.BoneDimensionsScaled; // Local offset scaled with custom rotation
            _sg_targetChildWorldPosInParentFront = CalculateGravityPositionOffsetForSegment(child, _sg_targetChildWorldPosInParentFront);
            TailSegment_ApplyCollisionMemory(child, contactResponse);
        }

        float TailSegment_PrepareCollisionResponse(TailSegment child)
        {
            _sg_slitFactor = child.Slithery;
            bool useCollisionMemory = UsesSelectiveCollisionMemory(child);
            float contactResponse = 1f;

            if (useCollisionMemory)
            {
                float damping = GetCollisionDampingFor(child);
                float target = child.CollisionContactRelevancy > 0f ? 1f : 0f;
                contactResponse = 1f - Mathf.Exp(-justDelta * 20f / damping);
                child.CollisionContactBlend = Mathf.Lerp(child.CollisionContactBlend, target, contactResponse);
                _sg_slitFactor = Mathf.Lerp(child.Slithery, child.CollisionReflect, child.CollisionContactBlend);
            }
            else
            {
                child.CollisionContactBlend = 0f;
                if (UseCollision && child.CollisionContactRelevancy > 0f) _sg_slitFactor = child.CollisionReflect;
            }

            return contactResponse;
        }

        void TailSegment_ApplyCollisionMemory(TailSegment child, float contactResponse)
        {
            if (!UsesSelectiveCollisionMemory(child))
            {
                child.CapsulePushPoseOffset = Vector3.zero;
                child.CollisionActiveContactBlend = 0f;
                child.CollisionInheritedContact = 0f;
                return;
            }

            if (UseSelectiveCapsuleAlongCollision)
            {
                float damping = GetCollisionDampingFor(child);
                child.CapsulePushPoseOffset *= Mathf.Exp(-justDelta * 8f / damping);
                _sg_targetChildWorldPosInParentFront += child.CapsulePushPoseOffset;
            }
            else child.CapsulePushPoseOffset = Vector3.zero;

            child.CollisionActiveContactBlend = Mathf.Lerp(child.CollisionActiveContactBlend,
                child.CollisionContactFlag ? 1f : 0f, contactResponse);
            child.CollisionInheritedContact = child.Index > 0
                ? Mathf.Max(child.ParentBone.CollisionContactBlend, child.ParentBone.CollisionInheritedContact) : 0f;

            float followContact = Mathf.Max(child.CollisionInheritedContact, child.CollisionContactBlend)
                * (1f - child.CollisionActiveContactBlend);
            if (child.Index <= 0 || followContact <= 0f) return;

            Vector3 previousDirection = child.CollisionFramePosition - child.ParentBone.CollisionFramePosition;
            Vector3 targetOffset = _sg_targetChildWorldPosInParentFront - child.ParentBone.ProceduralPosition;
            if (previousDirection.sqrMagnitude <= 1e-12f || targetOffset.sqrMagnitude <= 1e-12f) return;

            float response = Mathf.Lerp(1f, contactResponse, followContact);
            _sg_targetChildWorldPosInParentFront = child.ParentBone.ProceduralPosition
                + Vector3.Slerp(previousDirection.normalized, targetOffset.normalized, response) * targetOffset.magnitude;
        }

        /// <summary>
        /// Base position processing for swingy parameters animation
        /// Changing UnprocessedPosition value in child argument
        /// </summary>
        void TailSegment_BaseSwingProcessing(TailSegment child)
        {
            _sg_dirToTargetParentFront = _sg_targetChildWorldPosInParentFront - child.ProceduralPosition;

            // Unifying bendiness parameters on tail length and segments count then translating
            if (UnifyBendiness > 0f)
                child.ProceduralPosition += _sg_dirToTargetParentFront * secPeriodDelta *
                    _sg_curly * TailSegment_GetUnifiedBendinessMultiplier(child);
            else
                // Translating toward target position without restricitons
                child.ProceduralPosition += _sg_dirToTargetParentFront * _sg_curly * secPeriodDelta;

            if (Tangle != 0f)
                if (child.Slithery >= 1f)
                    child.ProceduralPosition = Vector3.LerpUnclamped(child.ProceduralPosition, _sg_targetChildWorldPosInParentFront, _tc_tangle);
        }


        /// <summary>
        /// Processing position for blending weight
        /// </summary>
        void TailSegment_PreRotationPositionBlend(TailSegment child)
        {
            if (child.BlendValue * conditionalWeight < 1f)
                child.ProceduralPositionWeightBlended = Vector3.LerpUnclamped(
                    child.transform.position, child.ProceduralPosition, child.BlendValue * conditionalWeight);
            //child.ParentBone.transform.TransformVector(child.LastFinalLocalPosition), child.ProceduralPosition, child.BlendValue);
            else
                child.ProceduralPositionWeightBlended = child.ProceduralPosition;
        }


        /// <summary>
        /// Calculations for slithery tail motion reference parent rotation
        /// </summary>
        Quaternion TailSegment_RotationSlithery(TailSegment child)
        {

            if (!FEngineering.QIsZero(child.Curving))
            {
                return GetSlitheryReferenceRotation(child)           // Back rotation for parent of parent bone
                    * child.Curving                              // Curving 
                    * child.ParentBone.LastKeyframeLocalRotation;   // Sync with animator
            }
            else
            {
                return GetSlitheryReferenceRotation(child)           // Back rotation for parent of parent bone
                    * child.ParentBone.LastKeyframeLocalRotation;    // Sync with animator
            }
        }

        /// <summary>
        /// Calculations for slithery tail motion reference parent rotation
        /// </summary>
        Quaternion TailSegment_RotationSlitheryDetached(TailSegment child)
        {
            Quaternion sourceRotation = AnimateDetachedChildren ? child.ParentBone.LastKeyframeLocalRotation : child.ParentBone.InitialLocalRotation;

            if (!FEngineering.QIsZero(child.Curving))
            {
                return GetSlitheryReferenceRotation(child)           // Back rotation for parent of parent bone
                    * child.Curving                              // Curving 
                    * sourceRotation;                           // Sync with animator when requested
            }
            else
            {
                return GetSlitheryReferenceRotation(child)           // Back rotation for parent of parent bone
                    * sourceRotation;                          // Sync with animator when requested
            }
        }

        Quaternion GetSlitheryReferenceRotation(TailSegment child)
        {
            if (child.Slithery <= 1f)
                return child.ParentBone.ParentBone.PosRefRotation;
            else
            {
                return Quaternion.LerpUnclamped(child.ParentBone.ParentBone.PosRefRotation,
                    child.ParentBone.ParentBone.PreviousPosReferenceRotation, (child.Slithery - 1f) * 5f);
            }
        }

        /// <summary>
        /// Calculations for stiff tail motion reference parent rotation
        /// </summary>
        Quaternion TailSegment_RotationStiff(TailSegment child)
        {
            // Curving feature
            if (!FEngineering.QIsZero(child.Curving))
            {
                return child.ParentBone.transform.rotation * // Parent bone rotation
                    MultiplyQ(child.Curving, child.Index * 2f)   // Curving multiplier
                    ;
            }
            else // No Curving
            {
                return child.ParentBone.transform.rotation;      // Parent bone rotation
            }
        }

        /// <summary>
        /// Defining style of tail motion
        /// </summary>
        Quaternion TailSegment_GetSwingRotation(TailSegment child, float curlFactor)
        {
            if (curlFactor >= 1f) // Slithery == 1
            {
                return TailSegment_RotationSlithery(child);
            }
            else if (curlFactor > Mathf.Epsilon) // Blend non slithery with slithery feature
            {
                return Quaternion.LerpUnclamped(
                    TailSegment_RotationStiff(child),           // A - Stiff
                    TailSegment_RotationSlithery(child),        // B - Slithery
                    curlFactor); // Lerp
            }
            else // Slithery == 0
                return TailSegment_RotationStiff(child);
        }


        Quaternion TailSegment_GetSwingRotationDetached(TailSegment child, float curlFactor)
        {
            if (curlFactor >= 1f) // Slithery == 1
            {
                return TailSegment_RotationSlitheryDetached(child);
            }
            else if (curlFactor > Mathf.Epsilon) // Blend non slithery with slithery feature
            {
                return Quaternion.LerpUnclamped(
                    TailSegment_RotationStiff(child),           // A - Stiff
                    TailSegment_RotationSlitheryDetached(child),        // B - Slithery
                    curlFactor); // Lerp
            }
            else // Slithery == 0
                return TailSegment_RotationStiff(child);
        }


        /// <summary>
        /// Calculating multiplier value for translation based on tail length and segments count
        /// To make different length and segment count tails behave in similar way
        /// </summary>
        float TailSegment_GetUnifiedBendinessMultiplier(TailSegment child)
        {
            // When short tail -> 0.2f / 1.25f -> 0.16              -> We want here small mul value
            // When long and many segments -> 0.1f / 8f -> 0.0125   -> We want here high mul value
            float segmentRatio = (child.BoneLength / _TC_TailLength);
            segmentRatio = Mathf.Pow(segmentRatio, 0.5f);

            if (segmentRatio == 0f) segmentRatio = 1f; // No dividing by zero ¯\_(ツ)_/¯

            float unifyMul = (_sg_curly / segmentRatio) / 2f;
            unifyMul = Mathf.LerpUnclamped(_sg_curly, unifyMul, UnifyBendiness);

            // Clamp extreme values
            if (unifyMul < 0.15f) unifyMul = 0.15f; else if (unifyMul > 1.4f) unifyMul = 1.4f;

            //Debug.Log(System.Math.Round( unifyMul, 3) + " Count = " + TailSegments.Count + " child.BoneLength " + child.BoneLength + "_tc_tailLength " + _TC_TailLength  + "curl = " + _sg_curly + "  Mul " + unifyMul + " segmentRatio " + segmentRatio + " mixed = " + (_sg_curly * unifyMul) );
            return unifyMul;
        }


        /// <summary>
        /// Updateing root parent bone with component parameters and features
        /// </summary>
        public void TailSegments_UpdateCoordsForRootBone(TailSegment parent)
        {
            TailSegment root = TailSegments[0];
            root.transform.localRotation = ( IncludeMode == EBoneInclusion.Full ? _tc_startBoneSourceRotation : root.LastKeyframeLocalRotation ) * _tc_startBoneRotOffset;
            //parent.transform.localRotation = parent.LastKeyframeLocalRotation * _tc_startBoneRotOffset;

            parent.PreviousPosReferenceRotation = parent.PosRefRotation;
            parent.PosRefRotation = parent.transform.rotation;

            parent.PreviousPosition = parent.ProceduralPosition;
            parent.ProceduralPosition = parent.transform.position;

            if( DetachChildren && IncludeMode != EBoneInclusion.Full ) root.TrueTargetRotation = root.transform.rotation;

            if( IncludeMode == EBoneInclusion.Full )
            {
                parent.ProceduralPosition += parent.transform.TransformVector( parent.LocalOffset );
                parent.TrueTargetRotation = parent.PosRefRotation;
                parent.RefreshKeyLocalPositionAndRotation();

                if( parent.transform == root.transform )
                {
                    // The virtual parent and its safety reference share the start bone's source rotation.
                    parent.LastKeyframeLocalRotation = Quaternion.identity;
                    parent.InitialLocalRotation = Quaternion.identity;
                }
            }

            parent.RefreshFinalPos(parent.transform.position);

            parent.ProceduralPositionWeightBlended = parent.ProceduralPosition;

            if (parent.ParentBone.transform != null)
            {
                // Update of ghost parent for slithery motion
                parent.ParentBone.PreviousPosReferenceRotation = parent.ParentBone.PosRefRotation;
                parent.ParentBone.PreviousPosition = parent.ParentBone.ProceduralPosition;

                parent.ParentBone.ProceduralPosition = parent.ParentBone.transform.position;
                parent.ParentBone.PosRefRotation = parent.ParentBone.transform.rotation;

                parent.ParentBone.ProceduralPositionWeightBlended = parent.ParentBone.ProceduralPosition;
            }

            if( IncludeMode == EBoneInclusion.Full )
                parent.ChildBone.PreviousPosition += _waving_sustain;
            else
                TailSegments[_tc_startI].ChildBone.PreviousPosition += _waving_sustain;

            root.RefreshKeyLocalPosition();
        }


        /// <summary>
        /// Begin update operations for additionaly genrated child bone of chain
        /// </summary>
        public void TailCalculations_UpdateArtificialChildBone(TailSegment child)
        {
            // Pre processing with limiting, gravity etc.
            //TailCalculations_SegmentPreProcessingStack(lastChild);

            //// Blending animation weight
            //TailSegment_PreRotationPositionBlend(lastChild);

            if (DetachChildren) TailSegment_PrepareRotationDetached(child); else TailSegment_PrepareRotation(child);
            TailSegment_BaseSwingProcessing(child);

            if (child.PositionSpeed < 1f) child.ProceduralPosition = TailCalculations_SmoothPosition(child.PreviousPosition /*+ _limiting_influenceOffset*/, child.ProceduralPosition, child);

            if (MaxStretching < 1f) StretchingLimiting(child);

            if (Axis2D > 0) Axis2DLimit(child);

            child.CollisionContactRelevancy = -1f;

            // Blending or just setting target position
            if (child.BlendValue * conditionalWeight < 1f)
                child.ProceduralPositionWeightBlended = Vector3.LerpUnclamped(
                    child.ParentBone.transform.TransformPoint(child.LastKeyframeLocalPosition), child.ProceduralPosition, child.BlendValue * conditionalWeight);
            else
                child.ProceduralPositionWeightBlended = child.ProceduralPosition;
        }


        /// <summary>
        /// Refreshing position for additionaly generated parent bone
        /// </summary>
        public void Editor_TailCalculations_RefreshArtificialParentBone()
        {
            GhostParent.ProceduralPosition = GhostParent.transform.position + FEngineering.TransformVector(GhostParent.transform.rotation, GhostParent.transform.lossyScale, GhostParent.LocalOffset);
        }
    }
}
