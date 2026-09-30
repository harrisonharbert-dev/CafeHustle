#if FIMPOSSIBLE_JOBSANDBURST_READY
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator
    {
        public float Delta => Time.deltaTime;
        // Separate from the user blend and retained when the updater rebuilds its buffers.
        internal float DistanceCullingBlend = 1f;
        public ETailCategory _Editor_Category = ETailCategory.Setup;
        public enum ETailCategory { Setup, Tweak, Features, Shaping }

        /// <summary> Getter which ensures fundamental transform object is defined </summary>
        public Transform BaseTransform
        {
            get
            {
                if (_baseTransform) return _baseTransform;

                if (TailSegments.Count > 0 && TailSegments[0] != null && TailSegments[0].transform) _baseTransform = TailSegments[0].transform;

                if (!_baseTransform && _TransformsGhostChain != null)
                {
                    for (int i = 0; i < _TransformsGhostChain.Count; i++)
                    {
                        if (!_TransformsGhostChain[i]) continue;
                        _baseTransform = _TransformsGhostChain[i];
                        break;
                    }
                }

                if (_baseTransform) return _baseTransform;

                if (StartBone) return StartBone;
                
                return transform;
            }
        }

        private Transform _baseTransform;

        [Tooltip( "First bone of tail motion chain" )]
        public Transform StartBone;
        [Tooltip( "Finish bone of tail motion chain" )]
        public Transform EndBone;

        /// <summary> Initialization method controll flag </summary>
        protected bool initialized = false;
        public bool IsInitialized { get { return initialized; } }

        public List<Transform> _TransformsGhostChain = new List<Transform>();
        public int _GhostChainInitCount = -1;

        /// <summary>
        /// Overrides the transform used for distance culling : set null to use Camera.main
        /// </summary>
        public static void SetDistanceCullingReference( Transform camera )
        {
            TurboTailAnimatorUpdater.SetDistanceCullingReference(camera);
        }

        /// <summary>
        /// Making sure ghost transform chain list is valid
        /// </summary>
        public void RefreshTransformsList()
        {
            if (_TransformsGhostChain == null)
            {
                _TransformsGhostChain = new List<Transform>();
            }
            else
            {
                for (int i = _TransformsGhostChain.Count - 1; i >= 0; i--)
                {
                    if (_TransformsGhostChain[i] == null) _TransformsGhostChain.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Getting list of transform for Tail Animator using Start and End Bone Transform guides
        /// </summary>
        public void GetGhostChain(Transform startBone)
        {
            if (_TransformsGhostChain == null) _TransformsGhostChain = new List<Transform>();

            Transform resolvedStartBone;

            if (startBone) 
                resolvedStartBone = startBone;
            else 
                resolvedStartBone = transform;


            if (EndBone == null)
            {
                // Just traight forward path through children
                _TransformsGhostChain.Clear();

                Transform tChild = resolvedStartBone;

                _TransformsGhostChain.Add(tChild);

                while (tChild.childCount > 0)
                {
                    tChild = tChild.GetChild(0);
                    if (!_TransformsGhostChain.Contains(tChild)) _TransformsGhostChain.Add(tChild);
                }

                _GhostChainInitCount = _TransformsGhostChain.Count;
            }
            else // Going through parents of 'End Bone' to 'Start Bone'
            {
                List<Transform> newTrs = new List<Transform>();
                Transform tParent = EndBone;

                newTrs.Add(tParent);

                while (tParent != null && tParent != resolvedStartBone)
                {
                    tParent = tParent.parent;
                    if (tParent != null && !newTrs.Contains(tParent)) newTrs.Add(tParent);
                }

                if (tParent == null) // No parent of startbone!
                {
                    _TransformsGhostChain.Clear();
                    _GhostChainInitCount = 0;
                    Debug.LogError("[Turbo Tail Animator] " + EndBone.name + " is not child of " + resolvedStartBone.name + "!", this);
                }
                else
                {
                    if (!newTrs.Contains(tParent)) newTrs.Add(tParent);
                    _TransformsGhostChain = newTrs;
                    _TransformsGhostChain.Reverse();
                    _GhostChainInitCount = _TransformsGhostChain.Count;
                }
            }

        }

    }
}
#endif
