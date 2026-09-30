#if FIMPOSSIBLE_JOBSANDBURST_READY
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator
    {
        public readonly List<TailSegment> TailSegments = new List<TailSegment>();
        TailSegment GhostParent;
        TailSegment GhostChild;

        struct DetachedChildLink
        {
            public Transform Child;
            public Transform Parent;
            public int SiblingIndex;
        }

        readonly List<DetachedChildLink> detachedChildLinks = new List<DetachedChildLink>();
        bool childrenDetached;

        /// <summary>
        /// Method to initialize component, to have more controll than waiting for Start() method, init can be executed before or after start, as programmer need it
        /// </summary>
        protected void Init()
        {
            if (initialized) return;

            if (StartBone == null) StartBone = transform;

            _baseTransform = null;
            GhostParent = null;
            GhostChild = null;

            bool hadMissingTransforms = _TransformsGhostChain == null;
            
            if (!hadMissingTransforms)
            {
                for (int i = 0; i < _TransformsGhostChain.Count; i++)
                {
                    if (_TransformsGhostChain[i]) continue;
                    hadMissingTransforms = true;
                    break;
                }
            }

            RefreshTransformsList();

            // Checking if we have transform to create tail chain from
            if (hadMissingTransforms || _TransformsGhostChain.Count == 0 || _TransformsGhostChain[0] != StartBone || (EndBone != null && _TransformsGhostChain[_TransformsGhostChain.Count - 1] != EndBone))
            {
                GetGhostChain(StartBone);
                RefreshTransformsList();
            }

            // Generating tail instances for procedural animation
            TailSegments.Clear();

            for (int i = 0; i < _TransformsGhostChain.Count; i++)
            {
                TailSegment b = new TailSegment(_TransformsGhostChain[i]);
                TailSegments.Add(b);
            }

            for (int i = 0; i < TailSegments.Count; i++) TailSegments[i].SetIndex(i, TailSegments.Count);


            // Checking correctness
            if (TailSegments.Count == 0)
            {
                Debug.LogError("[Turbo Tail Animator] Could not create a valid tail bones chain in " + name + "!", this);
                return;
            }

            _baseTransform = _TransformsGhostChain[0];
            previousWorldPosition = BaseTransform.position;
            TailLengthForWind = 0f;

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
                    }
                    #endregion

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

                    #region Automatic offset for the end joint

                    if (current.transform.parent)
                    {
                        scaleDir = current.transform.position - current.transform.parent.position;
                    }
                    else
                    {
                        if (current.transform.childCount > 0)
                        { 
                            scaleDir = current.transform.GetChild(0).position - current.transform.position; 
                        }
                        else
                        { 
                            scaleDir = current.transform.TransformDirection(Vector3.forward) * 0.05f; 
                        }
                    }

                    #endregion

                    GhostChild.ProceduralPosition = current.transform.position + scaleDir;
                    GhostChild.ProceduralPositionWeightBlended = GhostChild.ProceduralPosition;
                    GhostChild.PreviousPosition = GhostChild.ProceduralPosition;
                    GhostChild.PosRefRotation = Quaternion.identity;
                    GhostChild.PreviousPosReferenceRotation = Quaternion.identity;
                    GhostChild.ReInitializeLocalPosRot(current.transform.InverseTransformPoint(GhostChild.ProceduralPosition), Quaternion.identity);
                    GhostChild.TrueTargetRotation = GhostChild.PosRefRotation;
                    current.TrueTargetRotation = current.transform.rotation;

                    GhostChild.SetParentRef(current);
                }

                #endregion

                current.SetParentRef(parent);

                TailLengthForWind += Vector3.Distance(current.ProceduralPosition, parent.ProceduralPosition);
            }

            for (int i = 0; i < TailSegments.Count; i++)
                TailSegments[i].AssignDetachedRootCoords(BaseTransform);

            // List with ghosts for curves etc.
            GhostParent.SetIndex(-1, TailSegments.Count);
            GhostChild.SetIndex(TailSegments.Count, TailSegments.Count);

            WavingRotationOffset = Quaternion.identity;

            if (TailSegments.Count == 1)
            {
                if (TailSegments[0].transform.parent == null)
                {
                    Debug.LogError("[Turbo Tail Animator] Can't initialize a one-bone chain when its bone has no parent!", this);
                    TailAnimatorAmount = 0f;
                    initialized = false;
                    return;
                }
            }

            USER_RefreshTailBonesMotionParametersAfterManualChanges();

            if (DetachMode != EDetachChildrenMode.None) DetachChildrenTransforms();

            initialized = true;
            TurboTailAnimatorUpdater.AddToUpdate(this);

            if (UseWind) TailAnimatorWind.Refresh();
        }

        /// <summary> Removes the selected tail bones from their parent chain after their logical bind pose has been captured </summary>
        void DetachChildrenTransforms()
        {
            if (childrenDetached || TailSegments.Count < 2) return;

            detachedChildLinks.Clear();

            for (int i = 1; i < TailSegments.Count; i++)
            {
                Transform child = TailSegments[i].transform;
                if (child == null) continue;

                if( HideDetachedBones ) child.hideFlags = HideFlags.HideInHierarchy;

                detachedChildLinks.Add(new DetachedChildLink
                {
                    Child = child,
                    Parent = child.parent,
                    SiblingIndex = child.GetSiblingIndex()
                });
            }

            // Detach from the tip to preserve each transform's world pose while the chain is flattened
            for (int i = detachedChildLinks.Count - 1; i >= 0; i--)
                detachedChildLinks[i].Child.SetParent(null, true);

            childrenDetached = true;
        }

        /// <summary> Restores the hierarchy when this component is disabled or destroyed </summary>
        void RestoreChildrenHierarchy()
        {
            if (!childrenDetached) return;

            // Restore from root to tip so parents that are part of this tail exist before their children
            for (int i = 0; i < detachedChildLinks.Count; i++)
            {
                DetachedChildLink link = detachedChildLinks[i];
                if (link.Child == null || link.Parent == null) continue;

                if( link.Child.hideFlags == HideFlags.HideInHierarchy ) link.Child.hideFlags = HideFlags.None;

                link.Child.SetParent(link.Parent, true);
            }

            for (int i = 0; i < detachedChildLinks.Count; i++)
            {
                DetachedChildLink link = detachedChildLinks[i];
                if (link.Child == null || link.Parent == null || link.Child.parent != link.Parent) continue;

                link.Child.SetSiblingIndex(Mathf.Min(link.SiblingIndex, link.Parent.childCount - 1));
            }

            detachedChildLinks.Clear();
            childrenDetached = false;
        }

        /// <summary>
        /// Helper class to animate tail bones freely
        /// </summary>
        public class TailSegment
        {
            public TailSegment ParentBone { get; private set; }

            /// <summary> All references must have transform reference except GhostChild bone, GhostParent can have same transform as first bone in chain </summary>
            public Transform transform { get; private set; }

            /// <summary> Index of bone in Tail Animator list </summary>
            public int Index { get; private set; }

            /// <summary> For quicker value getting from curves = bone index / bones count so this value goes from 0 to 1 </summary>
            public float IndexOverlLength { get; private set; }

            /// <summary> Procedural position for tail motion without weight blending etc. </summary>
            public Vector3 ProceduralPosition = Vector3.zero;
            /// <summary> Final procedural position for tail transforms so weight blended </summary>
            public Vector3 ProceduralPositionWeightBlended = Vector3.zero;

            /// <summary> Final rotation for tail transform so rotated towards blended position </summary>
            public Quaternion TrueTargetRotation = Quaternion.identity;
            /// <summary> Reference rotation for position offset in parent orientation used in rotation smoothing </summary>
            public Quaternion PosRefRotation = Quaternion.identity;
            /// <summary> Memory for slithery motion </summary>
            public Quaternion PreviousPosReferenceRotation = Quaternion.identity;

            /// <summary> Memory for velocity (PreviousProceduralPosition) </summary>
            public Vector3 PreviousPosition;

            /// <summary> Blend with tail animator motion value used in partial blending </summary>
            public float BlendValue = 1f;

            /// <summary> Length of the bone in initial world space - distance to next bone transform </summary>
            public float BoneLength { get; private set; }

            public Vector3 InitialLocalPosition = Vector3.zero;
            public Vector3 LocalOffset = Vector3.zero;
            public Quaternion InitialLocalRotation = Quaternion.identity;

            // Cached before hierarchy flattening
            // The updater uses these values to reconstruct the source pose without asking Unity to evaluate the original child chain
            public Vector3 InitialLocalPositionInRoot = Vector3.zero;
            public Quaternion InitialLocalRotationInRoot = Quaternion.identity;
            public Vector3 InitialScaleInRoot = Vector3.one;

            // Animation styles helper variables
            public Vector3 VelocityHelper = Vector3.zero;
            public Quaternion QVelocityHelper = Quaternion.identity;

            // Expert
            public float PositionSpeed = 1f;
            public float RotationSpeed = 1f;
            public float Slithery = 1f;
            public float Curling = 1f;
            public float Springiness = 1f;

            // Collision curves are sampled on the main thread and copied into the native job inputs
            internal float ColliderRadius = 1f;
            internal float CollisionSlippery = 1f;
            internal float CollisionDamping = 0f;
            internal float CollisionReflect = 0f;

            // Curves are sampled on the main thread when motion parameters change
            internal float GravityWeight = 1f;

            public TailSegment(Transform transform)
            {
                if (transform == null) return;

                this.transform = transform;

                // Init position setup
                ProceduralPosition = transform.position;
                ProceduralPositionWeightBlended = ProceduralPosition;
                PreviousPosition = transform.position;
                PosRefRotation = transform.rotation;
                PreviousPosReferenceRotation = PosRefRotation;
                TrueTargetRotation = PosRefRotation;
                ReInitializeLocalPosRot(transform.localPosition, transform.localRotation);
                BoneLength = 0.1f;
            }

            public void ReInitializeLocalPosRot(Vector3 initLocalPos, Quaternion initLocalRot)
            {
                InitialLocalPosition = initLocalPos;
                InitialLocalRotation = initLocalRot;
            }

            public void SetIndex(int i, int tailSegments)
            {
                Index = i;
                if (i < 0) IndexOverlLength = 0f;
                else IndexOverlLength = (float)i / (float)tailSegments;
            }

            public void SetParentRef(TailSegment parent)
            { 
                ParentBone = parent; 
                BoneLength = (ProceduralPosition - ParentBone.ProceduralPosition).magnitude; 
            }

            public void AssignDetachedRootCoords(Transform root)
            {
                if (transform == null || root == null) return;

                InitialLocalPositionInRoot = root.InverseTransformPoint(transform.position);
                InitialLocalRotationInRoot = Quaternion.Inverse(root.rotation) * transform.rotation;

                Vector3 rootScale = root.lossyScale;
                Vector3 segmentScale = transform.lossyScale;

                InitialScaleInRoot = new Vector3
                    (
                        DivideScale(segmentScale.x, rootScale.x),
                        DivideScale(segmentScale.y, rootScale.y),
                        DivideScale(segmentScale.z, rootScale.z)
                    );
            }

            static float DivideScale(float value, float divisor)
            {
                return Mathf.Abs(divisor) > Mathf.Epsilon ? value / divisor : 1f;
            }

            /// <summary> Used for gizmos draw </summary>
            public float GetRadiusScaled()
            {
                Transform scaleReference;

                if (transform)
                    scaleReference = transform;
                else
                    scaleReference = ParentBone.transform;

                return ColliderRadius * Mathf.Abs(scaleReference.lossyScale.x);
            }

            internal void Validate()
            {
                if (BoneLength == 0f) BoneLength = 0.001f;
            }
        }

        void USER_RefreshTailBonesMotionParametersAfterManualChanges()
        {
            // The artificial tip drives the last bone's rotation and needs the same settings
            for (int i = 0; i <= TailSegments.Count; i++)
            {
                TailSegment bone;

                if (i < TailSegments.Count)
                    bone = TailSegments[i];
                else
                    bone = GhostChild;

                if (UseSlitheryCurve == false) bone.Slithery = Slithery;
                else bone.Slithery = SlitheryCurve.Evaluate(bone.IndexOverlLength);

                if (UseCurlingCurve == false) bone.Curling = ComputeStateCuringValue(Curling, bone.Slithery);
                else bone.Curling = ComputeStateCuringValue(CurlingCurve.Evaluate(bone.IndexOverlLength), bone.Slithery);

                if (UseSpringCurve == false) bone.Springiness = ComputeStateSpringinessValue(Springiness, bone.Slithery);
                else bone.Springiness = ComputeStateSpringinessValue(SpringCurve.Evaluate(bone.IndexOverlLength), bone.Slithery);

                if (UsePosSpeedCurve == false) bone.PositionSpeed = ReactionSpeed;
                else bone.PositionSpeed = PosCurve.Evaluate(bone.IndexOverlLength);

                if (UseRotSpeedCurve == false) bone.RotationSpeed = RotationRelevancy;
                else bone.RotationSpeed = RotCurve.Evaluate(bone.IndexOverlLength);

                if (UseCollDampingCurve) bone.CollisionDamping = CollDampingCurve.Evaluate(bone.IndexOverlLength);
                else bone.CollisionDamping = CollisionDamping;

                if (UseSlipperyCurve) bone.CollisionSlippery = SlipperyCurve.Evaluate(bone.IndexOverlLength);
                else bone.CollisionSlippery = CollisionSlippery;

                if (UseCollisionReflectCurve) bone.CollisionReflect = CollisionReflectCurve.Evaluate(bone.IndexOverlLength);
                else bone.CollisionReflect = ReflectCollision;

                if (i < TailSegments.Count) bone.ColliderRadius = GetColliderSphereRadiusFor(i);
                else bone.ColliderRadius = 0f;

                if (UseGravityCurve) bone.GravityWeight = GravityCurve.Evaluate(bone.IndexOverlLength);
                else bone.GravityWeight = 1f + (bone.Index / 2f) * (1f - bone.Slithery);

                bone.BlendValue = TailAnimatorAmountCurve.Evaluate(bone.IndexOverlLength) * TailAnimatorAmount;
            }
        }

        float ComputeStateCuringValue(float inputCurling, float inputSlithery)
        {
            float _sg_curly = Mathf.LerpUnclamped(0.5f, 0.125f, inputCurling);

            // Slithery-affected blend
            _sg_curly = Mathf.Lerp(_sg_curly, Mathf.LerpUnclamped(0.95f, 0.135f, inputCurling), inputSlithery);

            return _sg_curly;
        }

        float ComputeStateSpringinessValue(float inputSpringiness, float inputSlithery)
        {
            float _sg_springVelo = Mathf.LerpUnclamped(0.65f, 0.9f, inputSpringiness);

            // Slithery-affected blend
            _sg_springVelo = Mathf.Lerp(_sg_springVelo, Mathf.LerpUnclamped(0.1f, 0.85f, inputSpringiness), inputSlithery);

            return _sg_springVelo;
        }
    }
}
#endif
