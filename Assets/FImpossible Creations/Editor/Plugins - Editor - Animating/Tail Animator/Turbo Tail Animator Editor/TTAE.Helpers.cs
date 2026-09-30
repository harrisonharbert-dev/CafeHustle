#if FIMPOSSIBLE_JOBSANDBURST_READY
using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator_Editor
    {
        // RESOURCES ----------------------------------------

        private static Texture curveIcon { get { return TAEditorUtils.CurveIcon; } }

        private static GUIStyle smallStyle { get { return TAEditorUtils.SmallStyle; } }

        // HELPER VARIABLES ----------------------------------------

        private TurboTailAnimator Get { get { if( _get == null ) _get = target as TurboTailAnimator; return _get; } }
        private TurboTailAnimator _get;
        public List<SkinnedMeshRenderer> skins;

        private List<TurboTailAnimator> GetSelectedTailAnimators()
        {
            lastSelected = TAEditorUtils.GetSelectedComponents<TurboTailAnimator>();
            return lastSelected;
        }

        List<TurboTailAnimator> lastSelected;

    }
}
#endif
