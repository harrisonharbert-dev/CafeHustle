using FIMSpace.FEditor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class FTailAnimator2_Editor
    {
        private void Tab_DrawSetup()
        {

            FGUI_Inspector.VSpace(-2, -4);
            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
            GUILayout.Space(7f);

            Transform startField = Get.StartBone;
            if (Get.StartBone == null) { startField = Get.transform; GUI.color = new Color(1f, 1f, 1f, 0.7f); }


            if (topWarningAlpha > 0f)
                if (topWarning != "")
                {
                    GUI.color = new Color(c.r, c.g, c.b, c.a * Mathf.Min(1f, topWarningAlpha));
                    //EditorGUILayout.HelpBox(topWarning, MessageType.Info);
                    if (GUILayout.Button(topWarning, FGUI_Inspector.Style(new Color(0.8f, 0.8f, 0f, 0.1f), 0), GUILayout.ExpandWidth(true))) { topWarningAlpha = 0f; }
                    GUI.color = c;
                    topWarningAlpha -= 0.05f;
                }


            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 82;

            Transform preStart = Get.StartBone;

            if (Application.isPlaying) GUI.enabled = false;

            try
            {
                GUILayout.BeginHorizontal();
            }
            catch (System.Exception)
            {
                GUILayout.BeginHorizontal();
            }

            Transform startB = (Transform)EditorGUILayout.ObjectField(new GUIContent(sp_StartBone.displayName), startField, typeof(Transform), true);

            if (startB != preStart)
            {
                Get.EndBone = null;
                serializedObject.ApplyModifiedProperties();
            }

            if (Application.isPlaying) GUI.enabled = true;

            bool boneInSkin = true; if (skins.Count != 0) { boneInSkin = false; for (int s = 0; s < skins.Count; s++) { if (boneInSkin) break; for (int i = 0; i < skins[s].bones.Length; i++) { if (startB == skins[s].bones[i]) { boneInSkin = true; break; } } } }

            if (!boneInSkin)
            {
                GUILayout.Space(4);
                EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "'Start Bone' was not found in mesh renderer of this object, are you sure you assigned correct 'Start Bone'?"), new GUILayoutOption[] { GUILayout.Height(18), GUILayout.Width(20) });
            }

            EditorGUIUtility.labelWidth = 2;
            EditorGUILayout.PropertyField( sp_IncludeMode, new GUIContent( " ", sp_IncludeMode.tooltip ), GUILayout.Width( 72 ) );
            EditorGUIUtility.labelWidth = 0;

            GUILayout.EndHorizontal();


            if (EditorGUI.EndChangeCheck()) { Get.StartBone = startB; serializedObject.ApplyModifiedProperties(); Get.GetGhostChain(); serializedObject.Update(); }

            GUI.color = c;

            GUILayout.Space(4f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            Fold_TailChainSetup();

            GUILayout.Space(-5f);

            GUILayout.EndVertical();

            //Fold_DrawCoreAnimatorSetup();
            Fold_DrawAdditionalSetup();

            GUILayout.EndVertical();
        }



        private void Tab_DrawTweaking()
        {
            FGUI_Inspector.VSpace(-2, -4);
            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);

            GUILayout.Space(4);
            EditorGUIUtility.labelWidth = 160f;
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
            EditorGUILayout.PropertyField(sp_TailAnimatorAmount); EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndVertical();
            GUI.color = c;
            GUILayout.Space(-1f);

            Fold_TweakingBending();
            Fold_TweakingLimiting();
            Fold_TweakingSmoothing();
            Fold_TweakingAdditional();

            GUILayout.Space(-5f);

            GUILayout.EndVertical();
        }


        private void Tab_DrawAdditionalFeatures()
        {
            FGUI_Inspector.VSpace(-2, -3);
            EditorGUILayout.BeginHorizontal(FGUI_Resources.HeaderBoxStyle);

            DrawCategoryButton(TailAnimator2.ETailFeaturesCategory.Main, FGUI_Resources.Tex_Movement, "Main");
            DrawCategoryButton(TailAnimator2.ETailFeaturesCategory.Collisions, FGUI_Resources.Tex_Collider, "Collisions");
            DrawCategoryButton(TailAnimator2.ETailFeaturesCategory.IK, TAEditorUtils.IKIcon, "IK");
            DrawCategoryButton(TailAnimator2.ETailFeaturesCategory.Experimental, TAEditorUtils.DeflectionIcon, "Experimental");

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);

            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);

            if (Get._Editor_FeaturesCategory == TailAnimator2.ETailFeaturesCategory.Main)
            {
                // Waving
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle); GUILayout.Space(5f);
                Fold_ModuleWaving();

                GUILayout.Space(3f);
                GUILayout.EndVertical();

                // Partial Blend
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle); GUILayout.Space(2f);
                Fold_ModulePartialBlend();

                GUILayout.Space(2f);
                GUILayout.EndVertical();

                // Max Distance
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle); GUILayout.Space(2f);
                Fold_ModuleMaxDistance();

                GUILayout.Space(2f);
                GUILayout.EndVertical();
            }
            else if (Get._Editor_FeaturesCategory == TailAnimator2.ETailFeaturesCategory.Collisions)
            {
                // Module Collision
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle); GUILayout.Space(2f);
                Fold_ModuleCollisions();

                GUILayout.Space(2f);
                GUILayout.EndVertical();
            }
            else if (Get._Editor_FeaturesCategory == TailAnimator2.ETailFeaturesCategory.IK)
            {
                // IK
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle); GUILayout.Space(2f);
                Fold_ModuleIK();

                GUILayout.Space(2f);
                GUILayout.EndVertical();
            }
            else if (Get._Editor_FeaturesCategory == TailAnimator2.ETailFeaturesCategory.Experimental)
            {
                // Deflection Module
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle); GUILayout.Space(2f);
                Fold_ModuleDeflection();

                GUILayout.Space(2f);
                GUILayout.EndVertical();

                // Physical effectors
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle); GUILayout.Space(2f);
                Fold_ModulePhysEffectors();

                GUILayout.Space(2f);
                GUILayout.EndVertical();
            }

            GUILayout.Space(-5f);

            GUILayout.EndVertical();
        }




        private void Tab_DrawShaping()
        {
            FGUI_Inspector.VSpace(-2, -4);
            GUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            GUILayout.Space(2f);
            EditorGUIUtility.labelWidth = 120;

            if (FEngineering.QIsZero(Get.RotationOffset)) GUI.color = defaultValC; else GUI.color = c;

            EditorGUI.BeginChangeCheck();
            Get.RotationOffset = Quaternion.Euler(EditorGUILayout.Vector3Field(new GUIContent(sp_tailRotOff.displayName, sp_tailRotOff.tooltip), FEngineering.WrapVector(Get.RotationOffset.eulerAngles)));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                GetSelectedTailAnimators();
                for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].RotationOffset = Get.RotationOffset; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); }
            }

            // Curving Tail
            GUILayout.Space(2f);
            if (FEngineering.QIsZero(Get.Curving) && !Get.UseCurlingCurve) GUI.color = defaultValC; else GUI.color = c;

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            Get.Curving = Quaternion.Euler(EditorGUILayout.Vector3Field(new GUIContent(sp_curving.displayName, sp_curving.tooltip), FEngineering.WrapVector(Get.Curving.eulerAngles)));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                GetSelectedTailAnimators();
                for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].Curving = Get.Curving; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); }
            }

            if (Get.UseCurvingCurve)
            {
                EditorGUILayout.LabelField(new GUIContent("*", "Curving offset value weight for tail segments multiplied by curve"), GUILayout.Width(9));
                EditorGUILayout.PropertyField(sp_CurvCurve, new GUIContent("", sp_curving.tooltip), GUILayout.MaxWidth(32));
            }
            else
                GUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            SwitchButton(ref Get.UseCurvingCurve, "Spread curving rotation offset weight over tail segments", curveIcon);
            if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseCurvingCurve = Get.UseCurvingCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(3f);

            // Length Stretch
            if (Get.LengthMultiplier == 1f && !Get.UseLengthMulCurve) GUI.color = defaultValC; else GUI.color = c;

            EditorGUILayout.BeginHorizontal();

            if (!Get.UseLengthMulCurve)
            {
                EditorGUILayout.PropertyField(sp_LengthMultiplier);
                GUILayout.Space(4f); EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseLengthMulCurve, "Spread length multiplier weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseLengthMulCurve = Get.UseLengthMulCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
            }
            else
            {
                EditorGUILayout.PropertyField(sp_LengthMulCurve, new GUIContent(sp_LengthMultiplier.displayName, sp_LengthMultiplier.tooltip));
                GUILayout.Space(4f); EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseLengthMulCurve, "Spread length multiplier weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseLengthMulCurve = Get.UseLengthMulCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            if (Get.OverrideKeyframeAnimation > 0f)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 180;
                EditorGUILayout.PropertyField(sp_OverrideKeyframeAnimation);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "You must know what you're doing with this parameter! You making bones being not affected by keyframed animation"), GUILayout.Width(16), GUILayout.Height(16));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUI.color = new Color(1f, 1f, 1f, 0.7f);
                EditorGUIUtility.labelWidth = 210;
                EditorGUILayout.PropertyField(sp_OverrideKeyframeAnimation, new GUIContent(sp_OverrideKeyframeAnimation.displayName+ " (Off)", sp_OverrideKeyframeAnimation.tooltip));
                EditorGUIUtility.labelWidth = 0;
                GUI.color = c;
            }

            GUILayout.Space(1);

//#if FIMPOSSIBLE_JOBSANDBURST_READY
//            //EditorGUILayout.HelpBox( "Turbo Tail Animator packs are installed", MessageType.None );
//#else
//            GUILayout.Space(8);
//            EditorGUILayout.HelpBox( "If you want to use Turbo Tail Animator, you need to import few packages supplied by Unity", MessageType.None );
            
//            GUILayout.Space(2);
//            if (GUILayout.Button( "Install Burst, Collections and Mathematics", FGUI_Resources.ButtonStyle))
//            {
//                Client.AddAndRemove(
//                    new[]
//                    {
//                        "com.unity.mathematics",
//                        "com.unity.collections",
//                        "com.unity.burst",
//                    },
//                    null );
//            }
//#endif

            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }


        private void SwitchButton(ref bool enable, string tooltip, Texture icon)
        {
            TAEditorUtils.DrawSwitchButton(ref enable, tooltip, icon, serializedObject, c);
        }


    }
}
