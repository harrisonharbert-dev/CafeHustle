using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TailAnimator2
    {
        /// <summary> Procedural tail bone list </summary>
        public List<TailSegment> TailSegments;

        /// <summary> Artificial or existing parent bone instance for slithery motion support </summary>
        [SerializeField] TailSegment GhostParent;
        [SerializeField] TailSegment GhostChild;


        /// <summary>
        /// Method to initialize component, to have more controll than waiting for Start() method, init can be executed before or after start, as programmer need it.
        /// </summary>
        protected virtual void Init()
        {
            if (initialized) return;


            // Checking if we have transform to create tail chain from
            if (_TransformsGhostChain == null || _TransformsGhostChain.Count == 0)
            {
                _TransformsGhostChain = new List<Transform>();
                GetGhostChain();
            }


            // Generating tail instances for procedural animation
            TailSegments = new List<TailSegment>();

            for (int i = 0; i < _TransformsGhostChain.Count; i++)
            {
                if (_TransformsGhostChain[i] == null)
                {
                    Debug.Log("[Tail Animator] Null bones in " + name + " !");
                    continue;
                }

                TailSegment b = new TailSegment(_TransformsGhostChain[i]);
                b.SetIndex(i, _TransformsGhostChain.Count);
                TailSegments.Add(b);
            }


            // Checking correctness
            if (TailSegments.Count == 0)
            {
                Debug.Log("[Tail Animator] Could not create tail bones chain in " + name + " !");
                return;
            }


            _TC_TailLength = 0f;
            _baseTransform = _TransformsGhostChain[0];

            //if (_baseTransform.parent)
            //    _baseTransform = _baseTransform.parent;
            //else
            //    IncludeParent = false;


            // Setting parent-child relation for tail logics
            for (int i = 0; i < TailSegments.Count; i++)
            {
                TailSegment current = TailSegments[i];
                TailSegment parent;

                #region Defining Parent Bones

                if (i == 0)
                {
                    if (current.transform.parent)
                    {
                        // Creating parent and setting safety parent
                        parent = new TailSegment(current.transform.parent);
                        parent.SetParentRef(new TailSegment(parent.transform.parent));
                    }
                    else
                    #region If first bone is parentless
                    {
                        parent = new TailSegment(current.transform);

                        Vector3 toStartDir;

                        if (_TransformsGhostChain.Count > 1)
                        {
                            toStartDir = _TransformsGhostChain[0].position - _TransformsGhostChain[1].position;
                            if (toStartDir.magnitude == 0) toStartDir = transform.position - _TransformsGhostChain[1].position;
                        }
                        else
                        {
                            toStartDir = current.transform.position - _TransformsGhostChain[0].position;
                        }

                        if (toStartDir.magnitude == 0) toStartDir = transform.position - _TransformsGhostChain[0].position;
                        if (toStartDir.magnitude == 0) toStartDir = transform.forward;

                        parent.LocalOffset = parent.transform.InverseTransformPoint(parent.transform.position + toStartDir);
                        parent.SetParentRef(new TailSegment(current.transform));

                        if( IncludeMode == EBoneInclusion.Full )  parent.ProceduralPosition = parent.transform.TransformPoint( parent.LocalOffset );
                    }
                    #endregion

                    //current.InitialLocalRotation = Quaternion.Inverse(current.transform.localRotation);
                    GhostParent = parent;
                    GhostParent.Validate();
                    current.SetParentRef(GhostParent);
                }
                else // i != 0
                {
                    parent = TailSegments[i - 1];
                    // If bones are removed manually from chain we support custom length of bone undependent from transform parenting chain structure
                    current.ReInitializeLocalPosRot(parent.transform.InverseTransformPoint(current.transform.position), current.transform.localRotation);
                }


                #endregion


                #region Defining Last Child Bone

                if (i == TailSegments.Count - 1)
                {
                    Transform childT = null;
                    if (current.transform.childCount > 0) childT = current.transform.GetChild(0);

                    GhostChild = new TailSegment(childT);

                    // Scale ref for ghosting object position offset
                    Vector3 scaleDir;

                    if (FEngineering.VIsZero(EndBoneJointOffset))
                    {
                        if (current.transform.parent)
                        { scaleDir = current.transform.position - current.transform.parent.position; }
                        else
                            if (current.transform.childCount > 0)
                        { scaleDir = current.transform.GetChild(0).position - current.transform.position; }
                        else
                        { scaleDir = current.transform.TransformDirection(Vector3.forward) * 0.05f; }
                    }
                    else
                        scaleDir = current.transform.TransformVector(EndBoneJointOffset);


                    GhostChild.ProceduralPosition = current.transform.position + scaleDir;
                    GhostChild.ProceduralPositionWeightBlended = GhostChild.ProceduralPosition;
                    GhostChild.PreviousPosition = GhostChild.ProceduralPosition;
                    GhostChild.PosRefRotation = Quaternion.identity;
                    GhostChild.PreviousPosReferenceRotation = Quaternion.identity;
                    GhostChild.ReInitializeLocalPosRot(current.transform.InverseTransformPoint(GhostChild.ProceduralPosition), Quaternion.identity);
                    GhostChild.RefreshFinalPos(GhostChild.ProceduralPosition);
                    GhostChild.RefreshFinalRot(GhostChild.PosRefRotation);
                    GhostChild.TrueTargetRotation = GhostChild.PosRefRotation;
                    current.TrueTargetRotation = current.transform.rotation;

                    current.SetChildRef(GhostChild);
                    GhostChild.SetParentRef(current);
                }
                else
                {
                    current.SetChildRef(TailSegments[i + 1]);
                }

                #endregion


                current.SetParentRef(parent);

                _TC_TailLength += Vector3.Distance(current.ProceduralPosition, parent.ProceduralPosition);

                if (current.transform != _baseTransform) current.AssignDetachedRootCoords(BaseTransform);
            }


            // List with ghosts for curves etc.
            GhostParent.SetIndex(-1, TailSegments.Count);
            GhostChild.SetIndex(TailSegments.Count, TailSegments.Count);
            GhostParent.SetChildRef(TailSegments[0]);

            previousWorldPosition = BaseTransform.position;
            WavingRotationOffset = Quaternion.identity;

            if (CollidersDataToCheck == null) CollidersDataToCheck = new List<FImp_ColliderData_Base>();

            DynamicAlwaysInclude = new List<Component>();
            if (UseCollision) SetupSphereColliders();

            // List instance for deflection feature
            if (_defl_source == null) _defl_source = new List<TailSegment>();

            Waving_Initialize();

            if (DetachChildren) DetachChildrenTransforms();

            initialized = true;

            if (TailSegments.Count == 1)
            {
                if (TailSegments[0].transform.parent == null && IncludeMode != EBoneInclusion.Full )
                {
                    Debug.Log("[Tail Animator] Can't initialize one-bone length chain on bone which don't have any parent!");
                    Debug.LogError("[Tail Animator] Can't initialize one-bone length chain on bone which don't have any parent!");
                    TailAnimatorAmount = 0f;
                    initialized = false;
                    return;
                }
            }

            if (UseWind) TailAnimatorWind.Refresh();

            if (PostProcessingNeeded()) if (!_pp_initialized) InitializePostProcessing();

            // Collision damping is stored per segment, so initialize it even when prewarming is disabled.
            Expert_UpdateCollisionDamping();

            #region Prewarming tail to target state

            if (Prewarm)
            {
                ShapingParamsUpdate();
                ExpertParamsUpdate();

                Update();
                LateUpdate();

                justDelta = rateDelta;
                secPeriodDelta = 1f;
                deltaForLerps = secPeriodDelta;
                rateDelta = 1f / 60f;

                CheckIfTailAnimatorShouldBeUpdated();

                if (updateTailAnimator)
                {
                    int loopCount = 60 + TailSegments.Count / 2;

                    for (int d = 0; d < loopCount; d++)
                    {
                        PreCalibrateBones();
                        LateUpdate();
                    }
                }
            }

            #endregion

        }


        /// <summary>
        /// Detaching children for optimized work of Unity transform matrixes when tail is very long
        /// </summary>
        public void DetachChildrenTransforms()
        {
            int to = IncludeMode == EBoneInclusion.Exclude ? 1 : 0;
            for (int i = TailSegments.Count - 1; i >= to; i--)
            {
                if (TailSegments[i].transform)
                    TailSegments[i].transform.DetachChildren();
            }
        }


        /// <summary>
        /// Rebuilds the source pose which the regular hierarchy would normally provide.
        /// The fast path uses cached bind-pose coordinates. The animated path first reads
        /// Animator-authored local coordinates, then composes them from root to tip.
        /// </summary>
        void PrepareDetachedSourcePose()
        {
            if (AnimateDetachedChildren)
            {
                PrepareAnimatedDetachedSourcePose();
                return;
            }

            TailSegment child = TailSegments[0];
            child.RefreshKeyLocalPositionAndRotation(child.InitialLocalPosition, child.InitialLocalRotation);
            child.PreCalibrate();

            child = child.ChildBone;
            if (IncludeMode == EBoneInclusion.Exclude && child != GhostChild)
            {
                child.RefreshKeyLocalPositionAndRotation(child.InitialLocalPosition, child.InitialLocalRotation);
                child.PreCalibrate();
                child = child.ChildBone;
            }

            Matrix4x4 rootMatrix = _baseTransform.localToWorldMatrix;
            Quaternion rootRotation = _baseTransform.rotation;

            while (child != GhostChild)
            {
                child.RefreshKeyLocalPositionAndRotation(child.InitialLocalPosition, child.InitialLocalRotation);
                Vector3 worldPosition = rootMatrix.MultiplyPoint3x4(child.InitialLocalPositionInRoot);
                Quaternion worldRotation = rootRotation * child.InitialLocalRotationInRoot;
                SetWorldPositionAndRotation(child.transform, worldPosition, worldRotation);
                child = child.ChildBone;
            }
        }


        void PrepareAnimatedDetachedSourcePose()
        {
            int firstDetached = IncludeMode == EBoneInclusion.Exclude ? 2 : 1;
            if (firstDetached > TailSegments.Count) firstDetached = TailSegments.Count;

            // These transforms are still in their original hierarchy, so their local pose is already valid.
            for (int i = 0; i < firstDetached; i++)
                CaptureDetachedAnimationLocalPose(TailSegments[i]);

            // Detached transforms temporarily contain Animator-authored local values. Read each value
            // before replacing it with the world pose composed from its logical parent.
            for (int i = firstDetached; i < TailSegments.Count; i++)
            {
                TailSegment child = TailSegments[i];
                CaptureDetachedAnimationLocalPose(child);

                TailSegment parent = child.ParentBone;
                Vector3 worldPosition = parent.transform.TransformPoint(child.LastKeyframeLocalPosition);
                Quaternion worldRotation = parent.transform.rotation * child.LastKeyframeLocalRotation;
                SetWorldPositionAndRotation(child.transform, worldPosition, worldRotation);
            }
        }


        void CaptureDetachedAnimationLocalPose(TailSegment child)
        {
            Vector3 localPosition = child.transform.localPosition;
            Quaternion localRotation = child.transform.localRotation;

            if (OverrideKeyframeAnimation > 0f)
            {
                localPosition = Vector3.LerpUnclamped(localPosition, child.InitialLocalPosition, OverrideKeyframeAnimation);
                localRotation = Quaternion.LerpUnclamped(localRotation, child.InitialLocalRotation, OverrideKeyframeAnimation);
            }

            child.RefreshKeyLocalPositionAndRotation(localPosition, localRotation);
        }


        static void SetWorldPositionAndRotation(Transform target, Vector3 position, Quaternion rotation)
        {
#if UNITY_2022_3_OR_NEWER
            target.SetPositionAndRotation(position, rotation);
#else
            target.position = position;
            target.rotation = rotation;
#endif
        }



        /// <summary>
        /// Initialize post processing reference points
        /// </summary>
        void InitializePostProcessing()
        {
            _pp_reference = new List<TailSegment>();

            // Generating copy of whole tail processing chain
            _pp_ref_rootParent = new TailSegment(GhostParent);
            _pp_ref_rootParent.LocalOffset = GhostParent.LocalOffset;
            for (int i = 0; i < TailSegments.Count; i++) { TailSegment bone = new TailSegment(TailSegments[i]); _pp_reference.Add(bone); }
            _pp_ref_lastChild = new TailSegment(GhostChild);


            // Setting child parent relation
            _pp_ref_rootParent.SetChildRef(_pp_reference[0]); // root have just child
            _pp_ref_rootParent.SetParentRef(new TailSegment(GhostParent.ParentBone.transform)); // Safety parent 

            for (int i = 0; i < _pp_reference.Count; i++)
            {
                TailSegment bone = _pp_reference[i];
                bone.SetIndex(i, TailSegments.Count);

                if (i == 0) // First bone have ghost parent
                {
                    bone.SetParentRef(_pp_ref_rootParent);
                    bone.SetChildRef( _pp_reference.Count > 1 ? _pp_reference[i + 1] : _pp_ref_lastChild );
                }
                else if (i == _pp_reference.Count - 1) // last bone have ghost child
                {
                    bone.SetParentRef(_pp_reference[i - 1]);
                    bone.SetChildRef(_pp_ref_lastChild);
                }
                else // Default bone from chain middle points
                {
                    bone.SetParentRef(_pp_reference[i - 1]);
                    bone.SetChildRef(_pp_reference[i + 1]);
                }
            }

            _pp_ref_lastChild.SetParentRef(_pp_reference[_pp_reference.Count - 1]); // end have just parent


            _pp_initialized = true;
        }


    }
}
