#if FIMPOSSIBLE_JOBSANDBURST_READY
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator_Editor
    {

        bool drawTailTransforms = false;
        private void Fold_TailChainSetup()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawTailTransforms, 10, "►") + "  " + Lang("Tail Chain") + " (" + (Get._TransformsGhostChain.Count) + ")", FGUI_Resources.Tex_Bone, "Adjust count of chain bones"), FGUI_Resources.FoldStyle, new GUILayoutOption[] { GUILayout.Height(24) })) drawTailTransforms = !drawTailTransforms;

            bool refreshChain = Get._GhostChainInitCount != Get._TransformsGhostChain.Count;
            if (refreshChain) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh /*"Refresh"*/, "Chain Bones count changed you can reset it"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(24), GUILayout.Height(22) })) { Get.GetGhostChain(startBone); serializedObject.ApplyModifiedProperties(); serializedObject.Update(); return; }

            EditorGUILayout.EndHorizontal();

            if (drawTailTransforms)
            {
                GUILayout.Space(3);

                if (Get._TransformsGhostChain.Count > 1)
                {
                    GUI.enabled = false;

                    GUILayout.Space(1);

                    for (int i = 1; i < Get._TransformsGhostChain.Count - 1; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(new GUIContent(""), Get._TransformsGhostChain[i].transform, typeof(Transform), true);

                        if (!Application.isPlaying) GUI.enabled = true;
                        if (GUILayout.Button(new GUIContent("X", "Remove bone from chain with this button"), new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(14) }))
                        {
                            Get._TransformsGhostChain.RemoveAt(i);
                            EditorUtility.SetDirty(target);
                            break;
                        }
                        GUI.enabled = false;

                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(1);
                    }

                    GUILayout.Space(2);
                    GUI.enabled = true;

                    if (!Application.isPlaying) GUI.enabled = true; else GUI.enabled = false;

                    GUI.color = Color.white;
                }
                else
                {
                    GUILayout.Space(-4f);
                    EditorGUILayout.LabelField("No bones found in list", FGUI_Resources.HeaderStyle);
                    GUILayout.Space(5f);
                }

                DrawEndBoneInChainSetup();

                GUILayout.Space(2f);
            }
            else
            {
                GUI.color = new Color(1f, 1f, 1f, 0.45f);
                GUILayout.Space(-11f);
                EditorGUILayout.LabelField("...", FGUI_Resources.HeaderStyleBig);
                GUILayout.Space(5f);
                GUI.color = Color.white;

                DrawEndBoneInChainSetup();
            }

            GUILayout.Space(9f);
        }




        void DrawEndBoneInChainSetup()
        {
            if (Get._TransformsGhostChain.Count == 0)
            {
                if (Get.StartBone) { Get.GetGhostChain(startBone); }
                return;
            }

            // End bone field -----------------
            Transform endField = Get.EndBone; if (Get.EndBone == null) { endField = Get._TransformsGhostChain[Get._TransformsGhostChain.Count - 1]; GUI.color = new Color(1f, 1f, 1f, 0.7f); }
            EditorGUIUtility.labelWidth = Get.EndBone == null ? 100 : 74;

            GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            Transform endB = (Transform)EditorGUILayout.ObjectField(new GUIContent(Get.EndBone == null ? "End Bone (Auto)" : "End Bone"), endField, typeof(Transform), true);
            if (EditorGUI.EndChangeCheck())
            {
                Get.EndBone = endB;
                Get.GetGhostChain(startBone);
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(Get);
            }

            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            Transform sBone = Get.StartBone; if (sBone == null) sBone = Get.transform;
            if (endB == null) { endB = TAEditorUtils.GetLastChild(Get._TransformsGhostChain[Get._TransformsGhostChain.Count - 1]); }

            GUILayout.Space(4);
            if (sBone) if (Get.EndBone && Get.DetachMode == TurboTailAnimator.EDetachChildrenMode.None) if (!TAEditorUtils.IsChildOf(Get.EndBone, sBone)) EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "'End Bone' is not child of 'Start Bone'\nStart Bones is: '" + sBone.name + "'"), new GUILayoutOption[] { GUILayout.Height(20), GUILayout.Width(22) });

            GUILayout.EndHorizontal();

            GUI.color = Color.white;
        }



        bool drawWaving = false;
        void Fold_ModuleWaving()
        {
            FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UseWaving, sp_useWav, ref drawWaving, Lang("Auto Waving"), null, TAEditorUtils.WavingIcon, 22, sp_useWav.tooltip, LangBig());

            if (drawWaving && Get.UseWaving)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_wavType);

                if (Get.WavingType == TurboTailAnimator.FEWavingType.Advanced) EditorGUILayout.PropertyField(sp_altWave);

                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_wavSp);
                EditorGUILayout.PropertyField(sp_wavRa);

                GUILayout.Space(5f);

                bool altWeak = false;

                if (Get.WavingType == TurboTailAnimator.FEWavingType.Advanced)
                {
                    int zeros = 0; if (Get.WavingAxis.x == 0) zeros++; if (Get.WavingAxis.y == 0) zeros++; if (Get.WavingAxis.z == 0) zeros++;
                    if (zeros > 1) { altWeak = true; GUI.color = new Color(1f, .95f, 0.65f, 0.9f); }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_wavAx);

                if (altWeak) EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "Advanced waving should use more than one axis to get better results"), GUILayout.Width(16));

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5f);

                if (Get.WavingType != TurboTailAnimator.FEWavingType.Advanced) EditorGUILayout.PropertyField(sp_cosAd);

                if (!Application.isPlaying)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (Get.FixedCycle == 0f) GUI.color = new Color(1f, 1f, 1f, 0.7f);
                    EditorGUILayout.PropertyField(sp_FixedCycle);

                    if (Get.FixedCycle == 0f)
                    {
                        EditorGUILayout.LabelField("", GUILayout.Width(6));
                        EditorGUILayout.LabelField("(random)", GUILayout.Width(70));
                    }

                    EditorGUILayout.EndHorizontal();
                }

                GUI.color = Color.white;

                GUILayout.Space(4f);
            }
        }


        bool drawCollisions = false;
        void Fold_ModuleCollissions()
        {
            FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UseCollision, sp_useCollision, ref drawCollisions, Lang("Collisions"), null, FGUI_Resources.Tex_Collider, 22, sp_useCollision.tooltip, LangBig());

            if (drawCollisions && Get.UseCollision)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_CollisionMode);
                
                if ( Get.CollisionMode == TurboTailAnimator.ECollisionMode.CapsulesAlongBones)
                    EditorGUILayout.HelpBox( "Capsules Along Bones currently supports Sphere Collider, Capsule Collider and Character Controller only. Other collider types are ignored or computed with Sphere Per Bone mode.", MessageType.Info);

                GUILayout.Space(8f);

                EditorGUIUtility.labelWidth = 115;
                El_DrawCollisionDamping();
                El_DrawSlippery();
                El_DrawCollisionReflection();
                EditorGUIUtility.labelWidth = 0;

                FGUI_Inspector.DrawUILineCommon(12);

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 166;
                EditorGUIUtility.fieldWidth = 44;
                EditorGUILayout.PropertyField(sp_CollidersScaleMul, new GUIContent("Collision Contact Radius:", "Set collision detection radius per tail segment. Use curve to spread this value over tail shape."));
                EditorGUIUtility.fieldWidth = 0;
                GUILayout.Space(6);
                EditorGUILayout.PropertyField(sp_CollidersScaleCurve, GUIContent.none, GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;

                GUILayout.Space(5f);
                El_DrawSelectiveCollisionBox();
                GUILayout.Space(5f);
            }
        }


        bool drawPhysEffectors = false;
        bool drawWindSettings = true;
        void Fold_ModulePhysEffectors()
        {
            FGUI_Inspector.FoldHeaderStart(ref drawPhysEffectors, new GUIContent(Lang("Physical Effectors"), "Simulating physical effects like gravity, wind"), FGUI_Resources.FoldStyle, null, TAEditorUtils.WindIcon, 22);

            if (drawPhysEffectors)
            {
                // Gravity
                GUILayout.Space(5f);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_gravity);

                if (Get.UseGravityCurve)
                {
                    EditorGUILayout.LabelField(new GUIContent("*", "Gravity value weight for tail segments multiplied by curve"), GUILayout.Width(9));
                    EditorGUILayout.PropertyField(sp_GravityCurve, new GUIContent("", sp_GravityCurve.tooltip), GUILayout.MaxWidth(32));
                }
                else
                    GUILayout.Space(4f);

                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseGravityCurve, "Spread gravity weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { EditorUtility.SetDirty(lastSelected[i]); lastSelected[i].UseGravityCurve = Get.UseGravityCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();


                // Wind Effector
                GUILayout.Space(5f);

                if (Get.UseWind) // When wind enabled foldable
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawWindSettings, 10, "►") + "  " + Lang("Wind"), TAEditorUtils.WindIcon), FGUI_Resources.HeaderStyle, GUILayout.Height(22))) drawWindSettings = !drawWindSettings;
                    GUILayout.Space(8);

                    if (GUILayout.Button("Use TailAnimator Wind component for more settings", FGUI_Resources.BGInBoxStyle))
                    {
                        TailAnimatorWind wind = Application.isPlaying ? TailAnimatorWind.Instance : FUVH.FindSceneObject<TailAnimatorWind>();
                        if (wind != null) Selection.activeObject = wind.gameObject;
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Wind feature is under developement stage"), GUILayout.Width(16));
                    GUILayout.Space(5);
                    EditorGUILayout.PropertyField(sp_UseWind, GUIContent.none, GUILayout.Width(16));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("   " + Lang("Wind"), TAEditorUtils.WindIcon), FGUI_Resources.HeaderStyle, GUILayout.Height(22))) { Get.UseWind = true; }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(sp_UseWind, GUIContent.none, GUILayout.Width(16));
                    EditorGUILayout.EndHorizontal();
                }

                if (Get.UseWind && drawWindSettings)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(sp_WindEffectPower, new GUIContent("Effect Power", sp_WindEffectPower.tooltip));
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(sp_WindTurbulencePower, new GUIContent("Turbulence Power", sp_WindTurbulencePower.tooltip));
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(sp_WindWorldNoisePower, new GUIContent("World Noise Power", sp_WindWorldNoisePower.tooltip));

                    if (Application.isPlaying == false)
                    {
                        TailAnimatorWind sceneWind = FUVH.FindSceneObject<TailAnimatorWind>();
                        if (sceneWind == null)
                        {
                            if (GUILayout.Button("Add TailAnimatorWind to the Scene"))
                            {
                                sceneWind = new GameObject("Tail Animator Wind").AddComponent<TailAnimatorWind>();
                                sceneWind.transform.SetAsFirstSibling();
                            }
                        }
                    }
                }

                GUILayout.Space(1f);
            }
        }


        bool drawAdditionalSetup = true;
        private void Fold_DrawAdditionalSetup()
        {
            FGUI_Inspector.FoldHeaderStart(ref drawAdditionalSetup, Lang("Optimization And More"), FGUI_Resources.BGInBoxStyle, FGUI_Resources.TexAddIcon, 24);

            if (drawAdditionalSetup)
            {
                GUILayout.Space(4f);

                EditorGUIUtility.labelWidth = 80;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_DeltaType);
                GUILayout.Space(8f);
                EditorGUIUtility.labelWidth = 30;
                EditorGUIUtility.fieldWidth = 30;
                EditorGUILayout.PropertyField(sp_UpdateRate, new GUIContent("Rate", sp_UpdateRate.tooltip));
                EditorGUIUtility.fieldWidth = 0;
                EditorGUILayout.EndHorizontal();

                if (sp_UpdateRate.intValue > 0f)
                {
                    GUILayout.Space(4f);
                    EditorGUIUtility.labelWidth = 200;
                    EditorGUILayout.PropertyField(sp_MaxSimulationSteps);
                }

                GUILayout.Space(4f);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 140;
                EditorGUILayout.PropertyField(sp_AnimatePhysics);
                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth = 70;
                if( !sp_Calibrate.boolValue ) GUI.color = new Color(1f,1f,0.3f, 1f);
                EditorGUILayout.PropertyField( sp_Calibrate );
                GUI.color = Color.white;
                GUILayout.Space( 8 );
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;

                if( !sp_Calibrate.boolValue )
                    EditorGUILayout.HelpBox( "Beware disabled Calibration! It can be disabled only on the bones which are fully animated by keyframe animation or animated by other system.", MessageType.Warning );

                GUILayout.Space(4f);
                EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_PreferRotationOnly);
                if (sp_PreferRotationOnly.boolValue) EditorGUILayout.HelpBox("No stretch motion!", MessageType.None);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                GUILayout.Space(4f);
                EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                GUI.enabled = !Application.isPlaying;
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 180;
                EditorGUILayout.PropertyField( sp_DetachChildren, new GUIContent( "Detach Mode (Experimental):", sp_DetachChildren.tooltip ), GUILayout.MinWidth(300));

                if( Get.DetachMode != TurboTailAnimator.EDetachChildrenMode.None )
                {
                    EditorGUIUtility.labelWidth = 60;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField( sp_HideDetached, new GUIContent( " Hide", FGUI_Resources.Tex_Bone, sp_HideDetached.tooltip ), GUILayout.Height(18) );
                }

                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;

                if (Get.DetachMode == TurboTailAnimator.EDetachChildrenMode.DetachStatic)
                {
                    EditorGUILayout.HelpBox("Static mode: tail-bone animation is not supported. The hierarchy is restored when this component is disabled.", MessageType.None);
                }

                EditorGUILayout.EndVertical();

                GUILayout.Space(4f);
                EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                El_DrawOptimizeWithMeshes();
                EditorGUILayout.EndVertical();

                GUILayout.Space(4f);

                EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 110;
                EditorGUILayout.PropertyField(sp_CullAtDistance);

                if (sp_CullAtDistance.floatValue > 0f)
                {
                    EditorGUIUtility.labelWidth = 64;
                    GUILayout.Space(8);
                    EditorGUILayout.PropertyField(sp_DistanceCullingTransitionDuration, new GUIContent("Transition", sp_DistanceCullingTransitionDuration.tooltip), GUILayout.MaxWidth(110));
                    EditorGUILayout.LabelField("sec", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(22));
                    EditorGUIUtility.labelWidth = 0;
                }
                else
                {
                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("(Not Using)", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(60));
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Space(4f);
            }
            else
            {
                GUILayout.Space(2f);
            }

            GUILayout.EndVertical();
        }

    }

}
#endif
