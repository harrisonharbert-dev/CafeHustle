using FIMSpace.FEditor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace FIMSpace.FTail
{
    [CustomEditor(typeof( TurboTailAnimator ) )]
    [CanEditMultipleObjects]
    public partial class TurboTailAnimator_Editor : Editor
    {

#if FIMPOSSIBLE_JOBSANDBURST_READY
        public override bool RequiresConstantRepaint()
        {
            if (Get.IsInitialized == false) return false;
            var clock = TurboTailAnimatorUpdater.GetPerformanceClock_Update;
            if (clock == null) return false;
            return clock._foldout;
        }
#endif

        public override void OnInspectorGUI()
        {
#if FIMPOSSIBLE_JOBSANDBURST_READY
            serializedObject.Update();

            GUILayout.Space( 2 );
            EditorGUILayout.BeginHorizontal();
            DrawCategoryButton( TurboTailAnimator.ETailCategory.Setup, FGUI_Resources.Tex_GearSetup, "Setup" );
            DrawCategoryButton( TurboTailAnimator.ETailCategory.Tweak, FGUI_Resources.Tex_Sliders, "Tweak" );
            DrawCategoryButton( TurboTailAnimator.ETailCategory.Features, FGUI_Resources.Tex_Module, "Features" );
            EditorGUILayout.EndHorizontal();
            GUILayout.Space( 4 );

            var clock = TurboTailAnimatorUpdater.GetPerformanceClock_Update;
            if (Get.IsInitialized && clock != null) clock.Editor_DisplayFoldoutButton(5, -12);

            switch ( Get._Editor_Category )
            {
                case TurboTailAnimator.ETailCategory.Setup:
                    GUILayout.BeginVertical( FGUI_Resources.HeaderBoxStyle );
                    Tab_DrawSetup();
                    GUILayout.EndVertical();
                    break;

                case TurboTailAnimator.ETailCategory.Tweak:
                    GUILayout.BeginVertical( FGUI_Resources.HeaderBoxStyle );
                    Tab_DrawTweaking();
                    GUILayout.EndVertical();
                    break;

                case TurboTailAnimator.ETailCategory.Features:
                    GUILayout.BeginVertical( FGUI_Resources.HeaderBoxStyle );
                    Tab_DrawFeatures();
                    GUILayout.EndVertical();
                    break;
            }

            GUILayout.Space( 2f );

            serializedObject.ApplyModifiedProperties();

            if (Get.IsInitialized && clock != null && clock._foldout)
            {
                clock.Editor_DisplayAlways("All Turbo Tails ("+ TurboTailAnimatorUpdater.GetTurboTailsCount + "): ");
            }
#else
            serializedObject.Update();

            GUILayout.Space( 4 );
            EditorGUILayout.HelpBox( "If you want to use Turbo Tail Animator, you need to import few packages supplied by Unity", MessageType.Info );

            GUILayout.Space( 4 );

            string instButtTitle = "Install Burst, Collections and Mathematics";

            if ( installClicked || EditorApplication.isCompiling) 
            {
                GUI.enabled = false;
                instButtTitle = "Installing, please wait...";
            }

            if( GUILayout.Button( instButtTitle, FGUI_Resources.ButtonStyle ) )
            {
                Client.AddAndRemove(
                    new[]
                    {
                        "com.unity.mathematics",
                        "com.unity.collections",
                        "com.unity.burst",
                    },
                    null );

                installClicked = true;
            }

            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
#endif
        }

#if FIMPOSSIBLE_JOBSANDBURST_READY
        void DrawCategoryButton(TurboTailAnimator.ETailCategory target, Texture icon, string lang)
        {
            if (TAEditorUtils.DrawCategoryButton(Get._Editor_Category, target, icon, Lang(lang), EditorGUIUtility.currentViewWidth > (TAEditorUtils.SelectedLanguage == TAEditorUtils.Language.русский ? 390 : 360), 28))
            {
                Get._Editor_Category = target;
                SceneView.RepaintAll();
            }
        }
#else
        bool installClicked = false;    
#endif

    }
}
