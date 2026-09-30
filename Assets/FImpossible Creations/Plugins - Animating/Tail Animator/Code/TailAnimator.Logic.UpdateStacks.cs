using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {

        void SimulateTailMotionFrame(bool pp)
        {
            #region Prepare base positions calculation for tail segments to use in coords calculations and as reference

            TailSegments_UpdateRootFeatures();

            TailSegments_UpdateCoordsForRootBone( IncludeMode == EBoneInclusion.Full ? GhostParent : _tc_rootBone );

            if (pp) PostProcessing_ReferenceUpdate();


            if (_tc_startI > -1)
            {
                TailSegment child = TailSegments[_tc_startI]; // Used in while() loops below
                                                              // Going in while is 2x faster than for(i;i;i) loop

                if (!DetachChildren)
                {
                    while (child != GhostChild)
                    {
                        // Preparing parameter values adapted to stiff and slithery character and blended
                        TailSegment_PrepareBoneLength(child);
                        TailSegment_PrepareMotionParameters(child);

                        // Velocity changes detection
                        TailSegment_PrepareVelocity(child);

                        child = child.ChildBone;
                    }
                }
                else
                {
                    while (child != GhostChild)
                    {
                        if (AnimateDetachedChildren)
                            TailSegment_PrepareBoneLength(child);
                        else
                        {
                            // Remember bone scale referenced from initial position
                            child.BoneDimensionsScaled = Vector3.Scale(child.ParentBone.transform.lossyScale * child.LengthMultiplier, child.InitialLocalPosition);
                            child.BoneLengthScaled = child.BoneDimensionsScaled.magnitude; //(child.ParentBone.transform.position - child.transform.position).magnitude * child.LengthMultiplier;
                        }

                        TailSegment_PrepareMotionParameters(child);
                        TailSegment_PrepareVelocity(child);
                        child = child.ChildBone;
                    }
                }
            }

            // Udpate for artificial end bone
            TailSegment_PrepareBoneLength(GhostChild);
            TailSegment_PrepareMotionParameters(GhostChild);
            TailSegment_PrepareVelocity(GhostChild);

            #endregion

            #region Processing segments, calculating full target coords and apply to transforms

            if (UseSelectiveCapsuleAlongCollision && _tc_startPositionI > -1)
            {
                // Resolve each edge immediately so the next bone follows its parent's corrected
                // position during the same simulation pass.
                TailCalculations_ProcessCapsuleAlong(pp);
            }
            else if( _tc_startPositionI > -1 )
            {
                // IncludeMode.Full includes the first position.
                // Rotation application still starts at the next bone so that the transform outside the tail chain is never rotated.
                TailSegment child = TailSegments[_tc_startPositionI];

                if (!DetachChildren)
                {
                    while (child != GhostChild)
                    {
                        TailSegment_PrepareRotation(child);
                        TailSegment_BaseSwingProcessing(child);

                        // Pre processing with limiting, gravity etc.
                        TailCalculations_SegmentPreProcessingStack(child);

                        if (pp) TailCalculations_SegmentPostProcessing(child);

                        // Blending animation weight
                        TailSegment_PreRotationPositionBlend(child);

                        child = child.ChildBone;
                    }
                }
                else
                {
                    while (child != GhostChild)
                    {
                        TailSegment_PrepareRotationDetached(child);
                        TailSegment_BaseSwingProcessing(child);
                        TailCalculations_SegmentPreProcessingStack(child);
                        if (pp) TailCalculations_SegmentPostProcessing(child);
                        TailSegment_PreRotationPositionBlend(child);
                        child = child.ChildBone;
                    }
                }
            }

            // Applying processing for artificial child bone without transform
            TailCalculations_UpdateArtificialChildBone(GhostChild);

            #endregion

        }

        void UpdateTailAlgorithm()
        {
            TailCalculations_Begin(); // Root definition

            if (DeltaType != EFDeltaType.UnscaledDeltaTime && Time.timeScale == 0f) // Pause motion
            {
                if (_tc_startI > -1)
                {
                    TailSegment segment = TailSegments[_tc_startI];

                    while (segment != null)
                    {
                        if (segment.transform)
                        {
#if UNITY_6_0_OR_NEWER
                            segment.transform.SetPositionAndRotation(segment.LastFinalPosition, segment.LastFinalRotation);
#else
                            segment.transform.position = segment.LastFinalPosition;
                            segment.transform.rotation = segment.LastFinalRotation;
#endif
                        }
                        else break;

                        segment = segment.ChildBone;
                    }
                }

                return;
            }

            if (framesToSimulate != 0) // If framerate not defined then framesToSimulate is always == 1
            {

                if (!UseSelectiveCollisionMemory) ClearCollisionContactMemory();
                else if (!UseSelectiveCapsuleAlongCollision) ClearCapsuleAlongContactMemory();
                if (UseCollision) BeginCollisionsUpdate(); // Updating colliders to collide with

                // If post processing is needed we computing reference coordinates
                bool postProcesses = PostProcessingNeeded();

                MotionInfluenceLimiting();

                for (int i = 0; i < framesToSimulate; i++) // Simulating update frames
                    SimulateTailMotionFrame(postProcesses);

                // Updating root bone position
                TailSegments[_tc_startI].transform.position = TailSegments[_tc_startI].ProceduralPositionWeightBlended;
                TailSegments[_tc_startI].RefreshFinalPos(TailSegments[_tc_startI].ProceduralPositionWeightBlended);
                //TailSegments[_tc_startI].RefreshFinalLocalPos(TailSegments[_tc_startI].transform.localPosition);

                if (!DetachChildren) // When using common algorithm
                {
                    // Applying calculated coords to transforms
                    if (_tc_startII > -1)
                    {
                        TailSegment child = TailSegments[_tc_startII]; // Used in while() loops below
                        while (child != GhostChild)
                        {
                            // Calculate rotation
                            TailCalculations_SegmentRotation(child, child.LastKeyframeLocalPosition);

                            // Apply coords to segments
                            TailCalculations_ApplySegmentMotion(child);
                            child = child.ChildBone;
                        }
                    }
                }
                else // Detached mode needs some changes
                {
                    #region Detached Mode

                    if (_tc_startII > -1)
                    {
                        TailSegment child = TailSegments[_tc_startII]; // Used in while() loops below
                        while (child != GhostChild)
                        {
                            Vector3 sourceLocalPosition = AnimateDetachedChildren ? child.LastKeyframeLocalPosition : child.InitialLocalPosition;
                            TailCalculations_SegmentRotation(child, sourceLocalPosition);
                            //TailCalculations_SegmentRotationDetached(child, child.InitialLocalPosition);
                            TailCalculations_ApplySegmentMotion(child);
                            child = child.ChildBone;
                        }
                    }

                    #endregion
                }

                // If ghost child has transform let's apply motion too (change rotation of last bone)
                TailCalculations_SegmentRotation(GhostChild, GhostChild.LastKeyframeLocalPosition);
                GhostChild.ParentBone.transform.rotation = GhostChild.ParentBone.TrueTargetRotation;
                GhostChild.ParentBone.RefreshFinalRot(GhostChild.ParentBone.TrueTargetRotation);
                
                if (GhostChild.transform)
                {
                    GhostChild.RefreshFinalPos(GhostChild.transform.position);
                    GhostChild.RefreshFinalRot(GhostChild.transform.rotation);
                }
            }
            else // Skipping tail motion simulation and just applying coords computed lately
            // Executed only when using target UpdateRate
            {

                if (InterpolateRate)
                {
                    secPeriodDelta = rateDelta / 24f;
                    deltaForLerps = secPeriodDelta; // Unify delta value not amplified -> 1f / rate

                    SimulateTailMotionFrame(PostProcessingNeeded());

                    if( IncludeMode == EBoneInclusion.Full )
                    {
                        TailSegments[0].transform.position = TailSegments[0].ProceduralPositionWeightBlended;
                        TailSegments[0].RefreshFinalPos( TailSegments[0].ProceduralPositionWeightBlended );
                    }

                    if (_tc_startII > -1)
                    {
                        TailSegment child = TailSegments[_tc_startII];
                        while (child != GhostChild)
                        {
                            TailCalculations_SegmentRotation(child, child.LastKeyframeLocalPosition);
                            TailCalculations_ApplySegmentMotion(child);
                            child = child.ChildBone;
                        }
                    }

                    TailCalculations_SegmentRotation(GhostChild, GhostChild.LastKeyframeLocalPosition);
                    GhostChild.ParentBone.transform.rotation = GhostChild.ParentBone.TrueTargetRotation;
                    GhostChild.ParentBone.RefreshFinalRot(GhostChild.ParentBone.TrueTargetRotation);
                }
                else
                {
                    if (_tc_startI > -1)
                    {
                        TailSegment segment = TailSegments[_tc_startI];

                        while (segment != null)
                        {
                            if (segment.transform)
                            {
#if UNITY_6_0_OR_NEWER
                                segment.transform.SetPositionAndRotation(segment.LastFinalPosition, segment.LastFinalRotation);
#else
                                segment.transform.position = segment.LastFinalPosition;
                                segment.transform.rotation = segment.LastFinalRotation;
#endif
                            }
                            else break;

                            segment = segment.ChildBone;
                        }
                    }
                    else
                    {

                    }
                }
            }

        }


    }
}
