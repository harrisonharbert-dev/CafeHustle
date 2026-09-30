#if FIMPOSSIBLE_JOBSANDBURST_READY
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class TurboTailAnimator_Editor
    {
        void El_DrawLimitingAngle()
        {
            EditorGUILayout.PropertyField(sp_AngleLimit);

            if (Get.AngleLimit >= 181f) return;

            EditorGUILayout.PropertyField(sp_AngleLimitAxis);

            if (Get.AngleLimitAxis != Vector3.zero)
            {
                EditorGUILayout.PropertyField(sp_LimitAxisRange,
                    new GUIContent("Axis Range", "Offsets the negative and positive twist limits. Leave both values at zero for a symmetric limit."));
            }

            EditorGUILayout.PropertyField(sp_LimitSmoothing);
        }

        void El_DrawOptimizeWithMeshes()
        {
            bool hasRenderer = sp_OptimizeWithMeshes.hasMultipleDifferentValues;

            for (int i = 0; i < sp_OptimizeWithMeshes.arraySize; i++)
            {
                if (hasRenderer) break;

                hasRenderer = sp_OptimizeWithMeshes.GetArrayElementAtIndex(i).objectReferenceValue != null;
            }

            if (hasRenderer)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(sp_OptimizeWithMeshes, true);
                EditorGUI.indentLevel -= 1;
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            Renderer renderer = (Renderer)EditorGUILayout.ObjectField( new GUIContent(sp_OptimizeWithMeshes.displayName, sp_OptimizeWithMeshes.tooltip), null, typeof(Renderer), true);

            if (EditorGUI.EndChangeCheck() && renderer != null)
            {
                sp_OptimizeWithMeshes.arraySize = 1;
                sp_OptimizeWithMeshes.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                sp_OptimizeWithMeshes.isExpanded = true;
            }

            if (GUILayout.Button("Find", GUILayout.Width(44)))
            {
                serializedObject.ApplyModifiedProperties();

                foreach (Object selected in targets)
                {
                    TurboTailAnimator tail = (TurboTailAnimator)selected;
                    Renderer found = tail.GetComponent<Renderer>();
                    Transform searchRoot = tail.transform;

                    for (int depth = 0; depth <= 3; depth++)
                    {
                        if (searchRoot == null) break;
                        if (found != null) break;

                        found = searchRoot.GetComponentInChildren<Renderer>();
                        searchRoot = searchRoot.parent;
                    }

                    if (found == null) continue;

                    SerializedObject tailObject = new SerializedObject(tail);
                    SerializedProperty meshes = tailObject.FindProperty("OptimizeWithMeshes");
                    meshes.arraySize = 1;
                    meshes.GetArrayElementAtIndex(0).objectReferenceValue = found;
                    tailObject.ApplyModifiedProperties();
                }

                serializedObject.Update();
                sp_OptimizeWithMeshes.isExpanded = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void El_DrawSlithery()
        {
            GUI.color = new Color( 1f, 1f, 1f, 0.7f );
            EditorGUILayout.BeginHorizontal();
            if( Get.UseSlitheryCurve )
                GUILayout.Label( "                                      Tail Start", smallStyle );
            else
                GUILayout.Label( "                                       Stiff", smallStyle );

            GUILayout.FlexibleSpace();

            if( Get.UseSlitheryCurve )
                GUILayout.Label( "Tail End         ", smallStyle );
            else
                GUILayout.Label( "Smooth                      ", smallStyle );

            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;

            if( Get.UseSlitheryCurve )
                GUILayout.Space( 0f );
            else
                GUILayout.Space( -3f );


            if( Get.UseSlitheryCurve )
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField( sp_SlitCurve, new GUIContent( sp_Slithery.displayName, sp_Slithery.tooltip ), GUILayout.MaxHeight( 18 ) ); GUILayout.Space( 3f );
                EditorGUI.BeginChangeCheck();
                SwitchButton( ref Get.UseSlitheryCurve, "Spread sensitivity speed parameter weight over tail segments", curveIcon );
                if( EditorGUI.EndChangeCheck() ) { GetSelectedTailAnimators(); for( int i = 0; i < lastSelected.Count; i++ ) { lastSelected[i].UseSlitheryCurve = Get.UseSlitheryCurve; new SerializedObject( lastSelected[i] ).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField( sp_Slithery ); EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton( ref Get.UseSlitheryCurve, "Spread sensitivity speed parameter weight over tail segments", curveIcon );
                if( EditorGUI.EndChangeCheck() ) { GetSelectedTailAnimators(); for( int i = 0; i < lastSelected.Count; i++ ) { lastSelected[i].UseSlitheryCurve = Get.UseSlitheryCurve; new SerializedObject( lastSelected[i] ).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = Color.white;
        }


        void El_DrawCurling()
        {
            GUI.color = new Color(1f, 1f, 1f, 0.7f);

            EditorGUILayout.BeginHorizontal();

            string straiAdd = "";
            string straiTip = "";

            if (Get.UseCurlingCurve)
                GUILayout.Label("                                      Tail Start", smallStyle);
            else
                GUILayout.Label(new GUIContent("                                       Straightened " + straiAdd, straiTip), smallStyle);

            GUILayout.FlexibleSpace();

            if (Get.UseCurlingCurve)
                GUILayout.Label("Tail End         ", smallStyle);
            else
                GUILayout.Label("Tangled                      ", smallStyle);

            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;

            if (Get.UseCurlingCurve)
                GUILayout.Space(0f);
            else
                GUILayout.Space(-2f);

            if (Get.UseCurlingCurve)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_CurlingCurve, new GUIContent(sp_Curling.displayName, sp_Curling.tooltip), GUILayout.MaxHeight(18)); GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseCurlingCurve, "Spread curling over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseCurlingCurve = Get.UseCurlingCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField(sp_Curling); EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseCurlingCurve, "Spread curling parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseCurlingCurve = Get.UseCurlingCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = Color.white;
        }


        void El_DrawSpringiness()
        {
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            EditorGUILayout.BeginVertical();

            string balAdd = "";
            string balTip = "";

            if (!Get.UseCurlingCurve && !Get.UseSpringCurve && !Get.UseSlitheryCurve)
                if (Get.Curling > 0.3f)
                {
                    if (Get.Springiness > Mathf.Epsilon)
                    {
                        float treshold = Mathf.Lerp(0.3f, 0.08f, Get.Slithery);

                        if (Get.Springiness + treshold < Get.Curling)
                        {
                            balAdd = " ► ";
                            balTip = "Springiness should be set higher to notice bouncy motion";
                        }
                        else
                            if (Get.Springiness - treshold * 0.7f > Get.Curling)
                            {
                                balAdd = " ◄ ";
                                balTip = "Springiness should be set lower to avoid too rapid bounces";
                            }
                    }
                }

            // Tooltip texts
            EditorGUILayout.BeginHorizontal();

            if (Get.UseSpringCurve)
                GUILayout.Label("                                      Tail Start", smallStyle);
            else
                GUILayout.Label(new GUIContent("                                       Balanced" + " " + balAdd, balTip), smallStyle);

            GUILayout.FlexibleSpace();

            if (Get.UseSpringCurve)
                GUILayout.Label("Tail End         ", smallStyle);
            else
                GUILayout.Label(new GUIContent(balAdd + " " + "Bouncy                      ", balTip), smallStyle);

            EditorGUILayout.EndHorizontal();

            if (Get.UseSpringCurve)
                GUILayout.Space(-0f);
            else
                GUILayout.Space(-4f);

            GUI.color = Color.white;

            if (Get.UseSpringCurve)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_SpringCurve, new GUIContent(sp_Springiness.displayName, sp_Springiness.tooltip), GUILayout.MaxHeight(18)); GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSpringCurve, "Spread springiness speed parameter weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++)
                    {
                        lastSelected[i].UseSpringCurve = Get.UseSpringCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField(sp_Springiness); EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSpringCurve, "Spread springiness speed parameter weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseSpringCurve = Get.UseSpringCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = Color.white;

            EditorGUILayout.EndVertical();
        }


        static bool drawInclud = true;
        void El_DrawSelectiveCollisionBox()
        {
            if (Get.IncludedColliders == null) Get.IncludedColliders = new System.Collections.Generic.List<Collider>();

            GUILayout.Space(1f);
            GUI.color = new Color(0.85f, 1f, 0.85f, 1f);
            EditorGUILayout.BeginHorizontal(FGUI_Resources.HeaderBoxStyleH);
            string foldSymbol = FGUI_Resources.GetFoldSimbol(drawInclud);
            int includedCount = Get.IncludedColliders.Count;
            GUI.color = Color.white;

            GUILayout.Label(new GUIContent(" "), GUILayout.Width(1));
            string title = Lang("Collide With") + " (" + (includedCount == 0 ? "0 !!!" : includedCount.ToString()) + ")";

            if (GUILayout.Button(new GUIContent(" " + foldSymbol + "  " + title, FGUI_Resources.TexBehaviourIcon), FGUI_Resources.FoldStyle, GUILayout.Height(24)))
                drawInclud = !drawInclud;
            Rect collideWithHeaderRect = GUILayoutUtility.GetLastRect();

            EditorGUILayout.EndHorizontal();
            El_HandleDragAndDropColliders(collideWithHeaderRect);

            if (!drawInclud) return;

            FGUI_Inspector.VSpace(-3, -5);
            GUI.color = new Color(0.6f, .9f, 0.6f, 1f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
            GUI.color = Color.white;
            GUILayout.Space(5f);

            if (Get.IncludedColliders.Count == 0)
            {
                EditorGUILayout.LabelField("Please add here colliders", FGUI_Resources.HeaderStyle);
                GUILayout.Space(2f);
            }
            else
            {
                Get.CheckForColliderDuplicatesAndNulls();

                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < Get.IncludedColliders.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    Collider collider = Get.IncludedColliders[i];
                    if (collider != null && !collider.gameObject.activeInHierarchy) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    Get.IncludedColliders[i] = (Collider)EditorGUILayout.ObjectField(collider, typeof(Collider), true);
                    GUI.color = Color.white;

                    if (GUILayout.Button("X", GUILayout.MaxWidth(22), GUILayout.MaxHeight(16)))
                    {
                        EditorGUI.EndChangeCheck();
                        Get.IncludedColliders.RemoveAt(i);
                        EditorUtility.SetDirty(Get);
                        serializedObject.Update();
                        serializedObject.ApplyModifiedProperties();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        return;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Get.CheckForColliderDuplicatesAndNulls();
                    EditorUtility.SetDirty(Get);
                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();
                }
            }

            GUILayout.Space(6f);

            GUILayout.BeginVertical();
            GUI.color = ActiveEditorTracker.sharedTracker.isLocked
                ? new Color(0.44f, 0.44f, 0.44f, 0.8f)
                : new Color(0.95f, 0.95f, 0.99f, 0.9f);
            if (GUILayout.Button(new GUIContent("Lock Inspector for Drag & Drop Colliders", "Drag & drop colliders to 'Included Colliders' List from the hierarchy"), FGUI_Resources.ButtonStyle, GUILayout.Height(18)))
                ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
            GUI.color = Color.white;
            GUILayout.EndVertical();

            El_DrawDragAndDropCollidersBox();

            GUILayout.Space(3f);

            EditorGUIUtility.labelWidth = 190;
            EditorGUILayout.PropertyField(sp_CollideWithDisabledColliders);
            EditorGUIUtility.labelWidth = 0;

            if (sp_CollideWithDisabledColliders.boolValue)
            {
                EditorGUILayout.HelpBox("You can disable collider components on the objects - tail animator will still detect collision. If you deactivate the Game Object with collider - tail animator will not detect collision with it.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }


        void El_DrawDragAndDropCollidersBox()
        {
            GUILayout.Space(3);

            Rect dropArea = GUILayoutUtility.GetRect(0f, 38f, GUILayout.ExpandWidth(true));
            GUI.color = new Color(0.5f, 1f, 0.5f, 0.9f);
            GUI.Box(dropArea, "Drag & Drop New Colliders Here", new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter, fixedHeight = 38 });
            GUI.color = Color.white;
            El_HandleDragAndDropColliders(dropArea);
        }

        void El_HandleDragAndDropColliders(Rect dropArea)
        {
            Event dropEvent = Event.current;
            switch (dropEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(dropEvent.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (dropEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        var tails = GetSelectedTailAnimators();
                        if (!tails.Contains(Get)) tails.Add(Get);

                        foreach (Object dragged in DragAndDrop.objectReferences)
                        {
                            GameObject draggedObject = dragged as GameObject;
                            if (draggedObject == null) continue;

                            Collider[] colliders = draggedObject.GetComponents<Collider>();
                            for (int tailIndex = 0; tailIndex < tails.Count; tailIndex++)
                            {
                                TurboTailAnimator tail = tails[tailIndex];
                                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                                {
                                    tail.AddCollider(colliders[colliderIndex]);
                                    drawInclud = true;
                                }

                                EditorUtility.SetDirty(tail);
                            }
                        }
                    }

                    Event.current.Use();
                    break;
            }
        }


        void El_DrawCollisionDamping()
        {
            EditorGUILayout.BeginVertical();

            if (Get.UseCollDampingCurve)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = new Color(1f, 1f, 1f, 0.7f);
                GUILayout.Label("                              Tail Start", smallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Tail End         ", smallStyle);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_CollDampingCurve,
                    new GUIContent("Collision Damping", sp_CollDampingCurve.tooltip), GUILayout.MaxHeight(18));
                GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseCollDampingCurve, "Spread collision damping parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    GetSelectedTailAnimators();
                    for (int i = 0; i < lastSelected.Count; i++)
                    {
                        lastSelected[i].UseCollDampingCurve = Get.UseCollDampingCurve;
                        EditorUtility.SetDirty(lastSelected[i]);
                        lastSelected[i].RefreshRuntimeParameters();
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField(sp_CollisionDamping);
                EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseCollDampingCurve, "Spread collision damping parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    GetSelectedTailAnimators();
                    for (int i = 0; i < lastSelected.Count; i++)
                    {
                        lastSelected[i].UseCollDampingCurve = Get.UseCollDampingCurve;
                        EditorUtility.SetDirty(lastSelected[i]);
                        lastSelected[i].RefreshRuntimeParameters();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }


        void El_DrawSlippery()
        {
            EditorGUILayout.BeginVertical();

            if (Get.UseSlipperyCurve)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = new Color(1f, 1f, 1f, 0.7f);
                GUILayout.Label("                              Tail Start", smallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Tail End         ", smallStyle);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_SlipperyCurve, new GUIContent("Collision Slippery", sp_SlipperyCurve.tooltip), GUILayout.MaxHeight(18));
                GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSlipperyCurve, "Spread collision slippery parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    GetSelectedTailAnimators();
                    for (int i = 0; i < lastSelected.Count; i++)
                    {
                        lastSelected[i].UseSlipperyCurve = Get.UseSlipperyCurve;
                        EditorUtility.SetDirty(lastSelected[i]);
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField(sp_CollisionSlippery);
                EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSlipperyCurve, "Spread collision slippery parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    GetSelectedTailAnimators();
                    for (int i = 0; i < lastSelected.Count; i++)
                    {
                        lastSelected[i].UseSlipperyCurve = Get.UseSlipperyCurve;
                        EditorUtility.SetDirty(lastSelected[i]);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }


        void El_DrawCollisionReflection()
        {
            EditorGUILayout.BeginVertical();

            if (Get.UseCollisionReflectCurve)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = new Color(1f, 1f, 1f, 0.7f);
                GUILayout.Label("                              Tail Start", smallStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Tail End         ", smallStyle);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_CollisionReflectCurve, new GUIContent("Collision Reflection", sp_CollisionReflectCurve.tooltip), GUILayout.MaxHeight(18));
                GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseCollisionReflectCurve, "Spread collision reflection parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    GetSelectedTailAnimators();
                    for (int i = 0; i < lastSelected.Count; i++)
                    {
                        lastSelected[i].UseCollisionReflectCurve = Get.UseCollisionReflectCurve;
                        EditorUtility.SetDirty(lastSelected[i]);
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.fieldWidth = 38;
                if (Get.Slithery < 0.1f) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                EditorGUILayout.PropertyField(sp_ReflectCollision);
                EditorGUIUtility.fieldWidth = 0;
                GUI.color = Color.white;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseCollisionReflectCurve, "Spread collision reflection parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    GetSelectedTailAnimators();
                    for (int i = 0; i < lastSelected.Count; i++)
                    {
                        lastSelected[i].UseCollisionReflectCurve = Get.UseCollisionReflectCurve;
                        EditorUtility.SetDirty(lastSelected[i]);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

    }
}
#endif
