using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    [CustomEditor(typeof(TailAnimatorWind))]
    public class TailAnimatorWindEditor : Editor
    {
        public TailAnimatorWind Get { get { if (_get == null) _get = (TailAnimatorWind)target; return _get; } }
        private TailAnimatorWind _get;

        public override bool UseDefaultMargins() => false;

        SerializedProperty sp_power;
        SerializedProperty sp_additionalTurbulence;
        SerializedProperty sp_additionalTurbSpeed;
        SerializedProperty sp_SyncWithUnityWindZone;
        SerializedProperty sp_UnityWindZonePowerMul;
        SerializedProperty sp_UnityWindZoneTurbMul;
        SerializedProperty sp_overrideWind;
        SerializedProperty sp_rapidness;
        SerializedProperty sp_changesPower;
        SerializedProperty sp_directionChangeSpeed;
        SerializedProperty sp_turbulenceSpeed;
        SerializedProperty sp_worldTurb;
        SerializedProperty sp_worldTurbScale;
        SerializedProperty sp_worldTurbSpeed;
        SerializedProperty sp_powerDependsOnTailLength;
        SerializedProperty sp_persistThroughAllScenes;

        private void OnEnable()
        {
            sp_power = serializedObject.FindProperty("power");
            sp_additionalTurbulence = serializedObject.FindProperty("additionalTurbulence");
            sp_additionalTurbSpeed = serializedObject.FindProperty("additionalTurbSpeed");
            sp_SyncWithUnityWindZone = serializedObject.FindProperty("SyncWithUnityWindZone");
            sp_UnityWindZonePowerMul = serializedObject.FindProperty("UnityWindZonePowerMul");
            sp_UnityWindZoneTurbMul = serializedObject.FindProperty("UnityWindZoneTurbMul");
            sp_overrideWind = serializedObject.FindProperty("overrideWind");
            sp_rapidness = serializedObject.FindProperty("rapidness");
            sp_changesPower = serializedObject.FindProperty("changesPower");
            sp_directionChangeSpeed = serializedObject.FindProperty("directionChangeSpeed");
            sp_turbulenceSpeed = serializedObject.FindProperty("turbulenceSpeed");
            sp_worldTurb = serializedObject.FindProperty("worldTurb");
            sp_worldTurbScale = serializedObject.FindProperty("worldTurbScale");
            sp_worldTurbSpeed = serializedObject.FindProperty("worldTurbSpeed");
            sp_powerDependsOnTailLength = serializedObject.FindProperty("powerDependsOnTailLength");
            sp_persistThroughAllScenes = serializedObject.FindProperty("persistThroughAllScenes");
        }

        bool expandMain = true;
        bool expandProc = true;
        bool expandTurb = true;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(TAEditorUtils.BGInBoxBlankStylePadded);

            serializedObject.Update();

            FGUI_Inspector.HeaderBox(ref expandMain, " Main Wind Settings", true, TAEditorUtils.WindIconBlue);

            bool isOverrideWind = sp_overrideWind.vector3Value != Vector3.zero;

            if (expandMain)
            {
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                EditorGUIUtility.labelWidth = 200;
                EditorGUILayout.PropertyField(sp_powerDependsOnTailLength);
                EditorGUILayout.PropertyField(sp_persistThroughAllScenes);

                GUILayout.Space(8);

                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.PropertyField(sp_power);
                EditorGUILayout.PropertyField(sp_additionalTurbulence);
                EditorGUILayout.PropertyField(sp_additionalTurbSpeed);

                GUILayout.Space(8);

                if (!isOverrideWind && sp_SyncWithUnityWindZone.objectReferenceValue != null)
                    EditorGUILayout.HelpBox("Wind Zone multiplies Tail Aniamtor Wind Settings above", MessageType.None);
                else if( isOverrideWind)
                    EditorGUILayout.HelpBox("Override Wind value is overriding all direction values", MessageType.None);

                EditorGUIUtility.labelWidth = 200;
                if (isOverrideWind) GUI.color = new Color(1f, 1f, 1f, 0.65f);

                    EditorGUILayout.PropertyField(sp_SyncWithUnityWindZone);

                if (sp_SyncWithUnityWindZone.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(sp_UnityWindZonePowerMul);
                    EditorGUILayout.PropertyField(sp_UnityWindZoneTurbMul);
                }

                GUI.color = Color.white;

                EditorGUIUtility.labelWidth = 0;
                GUILayout.Space(8);
                EditorGUILayout.PropertyField(sp_overrideWind);

                if (sp_overrideWind.vector3Value != Vector3.zero)
                {
                    if (sp_overrideWind.vector3Value.magnitude != 1f)
                        EditorGUILayout.HelpBox("Vector value is not just direction but also power multiplicator.\nCurrent Power:" + sp_overrideWind.vector3Value.magnitude, MessageType.None);
                }

                GUILayout.Space(4);
                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(8);

            if (isOverrideWind)
            {
                GUI.enabled = false;
            }

            FGUI_Inspector.HeaderBox(ref expandProc, "Procedural Wind Directions", true, FGUI_Resources.Tex_Rotation);

            if (expandProc && GUI.enabled)
            {
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                EditorGUILayout.PropertyField(sp_rapidness);
                EditorGUILayout.PropertyField(sp_changesPower);
                EditorGUIUtility.labelWidth = 160;
                EditorGUILayout.PropertyField(sp_directionChangeSpeed);
                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.PropertyField(sp_turbulenceSpeed);

                EditorGUILayout.EndVertical();
            }

            GUI.enabled = true;
            GUILayout.Space(8);

            FGUI_Inspector.HeaderBox(ref expandTurb, "World Position Turbulence", true, TAEditorUtils.WindIcon);

            if (expandTurb)
            {
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                EditorGUILayout.PropertyField(sp_worldTurb);
                EditorGUILayout.PropertyField(sp_worldTurbScale);
                EditorGUILayout.PropertyField(sp_worldTurbSpeed);
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();
        }

    }
}
