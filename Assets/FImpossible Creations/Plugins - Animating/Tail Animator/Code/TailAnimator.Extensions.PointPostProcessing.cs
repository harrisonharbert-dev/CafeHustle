using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    // Positions Post Processing support calculations for effects like deflection
    // which needs baked position to correctly detect needed stuff for
    // every frame update
    public partial class TailAnimator2
    {
        /// <summary> Always return chain with coordinates without any POST processes as reference for POST processing algorithms. Separated list of tail bones operating on the same transforms but without post processed coords </summary>
        private List<TailSegment> _pp_reference;

        // Artificial bone points bakery
        private TailSegment _pp_ref_rootParent;
        private TailSegment _pp_ref_lastChild;

        private bool _pp_initialized = false;


        /// <summary>
        /// Return true if Tail Aniamtor is using feature which requires post processing support
        /// </summary>
        bool PostProcessingNeeded()
        {
            if (Deflection > Mathf.Epsilon) return true;
            else return false;
        }


        /// <summary>
        /// Post processing start frame calculations
        /// </summary>
        void PostProcessing_Begin()
        {
            TailSegments_UpdateCoordsForRootBone( IncludeMode == EBoneInclusion.Full ? _pp_ref_rootParent : _pp_reference[_tc_startI] );

            // Deflection support
            if (Deflection > Mathf.Epsilon) Deflection_BeginUpdate();
        }


        /// <summary>
        /// Computing reference coordinates for POST processing
        /// </summary>
        void PostProcessing_ReferenceUpdate()
        {
            TailSegment child = _pp_reference[_tc_startI];

            #region Prepare base positions calculation for tail segments to use in coords calculations and as reference


            while (child != _pp_ref_lastChild)
            {
                child.ParamsFrom(TailSegments[child.Index]); // Copying parameters settings from true bones

                if( IncludeMode == EBoneInclusion.Full )
                {
                    child.RefreshKeyLocalPositionAndRotation( TailSegments[child.Index].LastKeyframeLocalPosition, TailSegments[child.Index].LastKeyframeLocalRotation );
                    TailSegment_PrepareBoneLength( child );
                    TailSegment_PrepareMotionParameters( child );
                }

                TailSegment_PrepareVelocity(child);
                child = child.ChildBone;
            }

            // Udpate for artificial end bone

            if( IncludeMode == EBoneInclusion.Full )
            {
                _pp_ref_lastChild.ParamsFrom( GhostChild );
                _pp_ref_lastChild.RefreshKeyLocalPositionAndRotation( GhostChild.LastKeyframeLocalPosition, GhostChild.LastKeyframeLocalRotation );
                TailSegment_PrepareBoneLength( _pp_ref_lastChild );
            }

            TailSegment_PrepareMotionParameters(_pp_ref_lastChild);
            TailSegment_PrepareVelocity(_pp_ref_lastChild);

            #endregion


            #region Processing segments, calculating full target coords and apply to transforms

            child = _tc_startPositionI > -1 ? _pp_reference[_tc_startPositionI] : _pp_ref_lastChild;

            if (!DetachChildren)
            {
                while (child != _pp_ref_lastChild)
                {
                    TailSegment_PrepareRotation(child);
                    TailSegment_BaseSwingProcessing(child);
                    TailCalculations_SegmentPreProcessingStack(child);
                    TailSegment_PreRotationPositionBlend(child);
                    child = child.ChildBone;
                }
            }
            else
            {
                while (child != _pp_ref_lastChild)
                {
                    TailSegment_PrepareRotationDetached(child);
                    TailSegment_BaseSwingProcessing(child);
                    TailCalculations_SegmentPreProcessingStack(child);
                    TailSegment_PreRotationPositionBlend(child);
                    child = child.ChildBone;
                }
            }

            // Applying processing for artificial child bone without transform
            TailCalculations_UpdateArtificialChildBone(_pp_ref_lastChild);

            #endregion


            child = _tc_startII > -1 ? _pp_reference[_tc_startII] : _pp_ref_lastChild;
            while (child != _pp_ref_lastChild)
            {
                // Calculate rotation
                TailCalculations_SegmentRotation(child, child.LastKeyframeLocalPosition);
                child = child.ChildBone;
            }

            // If ghost child has transform let's apply motion too (change rotation of last bone)
            TailCalculations_SegmentRotation(child, child.LastKeyframeLocalPosition);
            child.ParentBone.RefreshFinalRot(child.ParentBone.TrueTargetRotation);
        }

    }
}