using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class FTailAnimator2_Editor
    {
        // RESOURCES ----------------------------------------

        private static Texture curveIcon { get { return TAEditorUtils.CurveIcon; } }

        private static UnityEngine.Object _manualFile;

        private static GUIStyle smallStyle { get { return TAEditorUtils.SmallStyle; } }

        // HELPER VARIABLES ----------------------------------------

        private TailAnimator2 Get { get { if (_get == null) _get = target as TailAnimator2; return _get; } }
        private TailAnimator2 _get;

        private string topWarning = "";
        private float topWarningAlpha = 0f;

        static bool drawDefaultInspector = false;
        //private Color limitsC = new Color(1f, 1f, 1f, 0.88f);
        private Color c;
        private Color bc;
        private Color defaultValC = new Color(1f, 1f, 1f, 0.825f);


        public List<SkinnedMeshRenderer> skins;
        SkinnedMeshRenderer largestSkin;
        Animator animator;
        Animation animation;


        /// <summary>
        /// Trying to deep find skinned mesh renderer
        /// </summary>
        private void FindComponents()
        {
            if (skins == null) skins = new List<SkinnedMeshRenderer>();

            foreach (var t in Get.transform.GetComponentsInChildren<Transform>())
            {
                SkinnedMeshRenderer s = t.GetComponent<SkinnedMeshRenderer>(); if (s) skins.Add(s);
                if (!animator) animator = t.GetComponent<Animator>();
                if (!animator) if (!animation) animation = t.GetComponent<Animation>();
            }

            if ((skins != null && largestSkin != null) && (animator != null || animation != null)) return;

            if (Get.transform != Get.transform)
            {
                foreach (var t in Get.transform.GetComponentsInChildren<Transform>())
                {
                    SkinnedMeshRenderer s = t.GetComponent<SkinnedMeshRenderer>(); if (!skins.Contains(s)) if (s) skins.Add(s);
                    if (!animator) animator = t.GetComponent<Animator>();
                    if (!animator) if (!animation) animation = t.GetComponent<Animation>();
                }
            }

            // Searching in parent
            if (skins.Count == 0)
            {
                Transform lastParent = Get.transform;

                while (lastParent != null)
                {
                    if (lastParent.parent == null) break;
                    lastParent = lastParent.parent;
                }

                foreach (var t in lastParent.GetComponentsInChildren<Transform>())
                {
                    SkinnedMeshRenderer s = t.GetComponent<SkinnedMeshRenderer>(); if (!skins.Contains(s)) if (s) skins.Add(s);
                    if (!animator) animator = t.GetComponent<Animator>();
                    if (!animator) if (!animation) animation = t.GetComponent<Animation>();
                }
            }

            if (skins.Count > 1)
            {
                largestSkin = skins[0];
                for (int i = 1; i < skins.Count; i++)
                    if (skins[i].bones.Length > largestSkin.bones.Length)
                        largestSkin = skins[i];
            }
            else
                if (skins.Count > 0) largestSkin = skins[0];

        }


        private List<TailAnimator2> GetSelectedTailAnimators()
        {
            lastSelected = TAEditorUtils.GetSelectedComponents<TailAnimator2>();
            return lastSelected;
        }
        List<TailAnimator2> lastSelected;

    }
}
