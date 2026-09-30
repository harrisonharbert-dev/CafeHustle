#if FIMPOSSIBLE_JOBSANDBURST_READY
using System.Collections;
using System.Xml;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator_Editor
    {
        private void SetupLangs() { TAEditorUtils.SetupLanguage(); }
        private string Lang(string title) { return TAEditorUtils.Localize(title); }
        private bool LangBig() { return TAEditorUtils.UsesLargeLanguageLabels(); }

    }

}
#endif
