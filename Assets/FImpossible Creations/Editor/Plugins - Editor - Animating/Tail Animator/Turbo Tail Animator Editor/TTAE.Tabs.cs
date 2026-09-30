#if FIMPOSSIBLE_JOBSANDBURST_READY
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator_Editor
    {
        private void Tab_DrawSetup()
        {
            GUILayout.BeginVertical( FGUI_Resources.ViewBoxStyle );
            GUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );
            GUILayout.Space( 2f );

            Transform startField = Get.StartBone;
            if( Get.StartBone == null ) { startField = Get.transform; GUI.color = new Color( 1f, 1f, 1f, 0.7f ); }

            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 82;

            Transform preStart = Get.StartBone;

            if( Application.isPlaying ) GUI.enabled = false;

            try
            {
                GUILayout.BeginHorizontal();
            }
            catch( System.Exception )
            {
                GUILayout.BeginHorizontal();
            }

            Transform startB = (Transform)EditorGUILayout.ObjectField( new GUIContent( sp_StartBone.displayName ), startField, typeof( Transform ), true );

            if( startB != preStart )
            {
                Get.EndBone = null;
                serializedObject.ApplyModifiedProperties();
            }

            if( Application.isPlaying ) GUI.enabled = true;

            bool boneInSkin = true; if( skins != null && skins.Count != 0 ) { boneInSkin = false; for( int s = 0; s < skins.Count; s++ ) { if( boneInSkin ) break; for( int i = 0; i < skins[s].bones.Length; i++ ) { if( startB == skins[s].bones[i] ) { boneInSkin = true; break; } } } }

            if( !boneInSkin )
            {
                GUILayout.Space( 4 );
                EditorGUILayout.LabelField( new GUIContent( FGUI_Resources.Tex_Warning, "'Start Bone' was not found in mesh renderer of this object, are you sure you assigned correct 'Start Bone'?" ), new GUILayoutOption[] { GUILayout.Height( 18 ), GUILayout.Width( 20 ) } );
            }

            GUILayout.EndHorizontal();


            EditorGUIUtility.labelWidth = 0;
            if( EditorGUI.EndChangeCheck() ) { Get.StartBone = startB; serializedObject.ApplyModifiedProperties();/* Get.GetGhostChain();*/ serializedObject.Update(); }

            GUI.color = Color.white;

            GUILayout.EndVertical();

            GUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );
            Fold_TailChainSetup();

            GUILayout.Space( -5f );

            GUILayout.EndVertical();

            GUILayout.Space( 5f );
            Fold_DrawAdditionalSetup();

            GUILayout.EndVertical();
        }


        private void Tab_DrawTweaking()
        {
            FGUI_Inspector.VSpace( -2, -4 );
            GUILayout.BeginVertical( FGUI_Resources.ViewBoxStyle );

            GUILayout.Space( 4 );
            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );
            EditorGUIUtility.labelWidth = 160f;
            EditorGUILayout.BeginHorizontal(  );
            EditorGUILayout.PropertyField(sp_TailAnimatorAmount); EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.PropertyField(sp_TailAnimatorAmountCurve, GUIContent.none, GUILayout.MaxWidth(40));
            EditorGUILayout.EndHorizontal(  );
            GUI.color = Color.white;

            GUILayout.Space( 8 );

            EditorGUIUtility.labelWidth = 115;

            El_DrawSlithery();
            GUILayout.Space( 2 );
            El_DrawCurling();
            GUILayout.Space(2);
            El_DrawSpringiness();

            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space( 3 );
            FGUI_Inspector.DrawUILineCommon( 12 );

            EditorGUILayout.PropertyField( sp_MaxStretching );
            GUILayout.Space( 5f );
            El_DrawLimitingAngle();
            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( sp_MotionInfluence );
            if( Get.MotionInfluence != 1f ) { var spp = sp_MotionInfluence.Copy(); spp.NextVisible( false ); EditorGUILayout.PropertyField( spp ); }

            FGUI_Inspector.DrawUILineCommon( 12 );

            // Position Speed
            if( Get.UsePosSpeedCurve )
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField( sp_PosCurve, new GUIContent( sp_ReactSpeed.displayName, sp_ReactSpeed.tooltip ), GUILayout.MaxHeight( 18 ) ); GUILayout.Space( 3f );
                SwitchButton( ref Get.UsePosSpeedCurve, "Spread position speed parameter weight over tail segments", curveIcon );
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField( sp_ReactSpeed ); EditorGUIUtility.fieldWidth = 0;
                SwitchButton( ref Get.UsePosSpeedCurve, "Spread position speed parameter weight over tail segments", curveIcon );
                EditorGUILayout.EndHorizontal();
            }

            // Rotation Speed
            if( Get.UseRotSpeedCurve )
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField( sp_RotCurve, new GUIContent( sp_RotRelev.displayName, sp_RotRelev.tooltip ), GUILayout.MaxHeight( 18 ) ); GUILayout.Space( 3f );
                SwitchButton( ref Get.UseRotSpeedCurve, "Spread rotation speed parameter weight over tail segments", curveIcon );
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField( sp_RotRelev ); EditorGUIUtility.fieldWidth = 0;
                SwitchButton( ref Get.UseRotSpeedCurve, "Spread rotation speed parameter weight over tail segments", curveIcon );
                EditorGUILayout.EndHorizontal();
            }


            GUILayout.Space( 3 );
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( sp_AnimateRoll );

            if( Get.AnimateRoll )
            {
                float val = EditorGUILayout.Slider( GUIContent.none, 1f - Get.RotationRelevancy, 0f, 1f );
                Get.RotationRelevancy = 1f - val;
            }

            EditorGUILayout.EndHorizontal();

            if( Get.AnimateRoll )
                if( Get.RotationRelevancy > 0.5f ) EditorGUILayout.HelpBox( "Set Roll higher than 0.6 to see results more clearly", MessageType.None );

            GUILayout.Space( 2f );


            GUILayout.Space( -4);
            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }



        private void Tab_DrawFeatures()
        {
            GUILayout.BeginVertical( FGUI_Resources.ViewBoxStyle );
            GUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

            GUILayout.Space( 4f );
            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxLightStyle );
            Fold_ModuleWaving();
            EditorGUILayout.EndVertical();

            GUI.color = new Color(0.25f, 1f, 0.4f, 1f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle );
            GUI.color = Color.white;
            Fold_ModuleCollissions();
            EditorGUILayout.EndVertical();

            GUI.color = new Color(0.65f, 0.87f, 1f, 1f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle );
            GUI.color = Color.white;
            Fold_ModulePhysEffectors();
            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }


        private void SwitchButton( ref bool enable, string tooltip, Texture icon )
        {
            TAEditorUtils.DrawSwitchButton(ref enable, tooltip, icon, serializedObject, Color.white);
        }


    }
}
#endif
