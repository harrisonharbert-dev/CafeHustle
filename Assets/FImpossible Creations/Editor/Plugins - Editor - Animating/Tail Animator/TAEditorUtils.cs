using System.Collections;
using System.Collections.Generic;
using System.Xml;
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    /// <summary>
    /// Shared editor-only helpers used by Tail Animator and Turbo Tail Animator inspectors
    /// </summary>
    public static class TAEditorUtils
    {
        public enum Language { English, Polski, русский, 中文, 日本語, 한국어 }

        private static TextAsset languageFile;
        private static Hashtable languageTexts;
        public static Language SelectedLanguage { get; set; }

        public static Texture2D TailAnimatorIcon { get { return LoadTexture(ref tailAnimatorIcon, "Tail Animator/Tail Animator Icon Small"); } }
        public static Texture2D WavingIcon { get { return LoadTexture(ref wavingIcon, "Tail Animator/WavingIcon"); } }
        public static Texture2D PartialBlendIcon { get { return LoadTexture(ref partialBlendIcon, "Tail Animator/PartialBlendIcon"); } }
        public static Texture2D IKIcon { get { return LoadTexture(ref ikIcon, "Tail Animator/IKIcon"); } }
        public static Texture2D DeflectionIcon { get { return LoadTexture(ref deflectionIcon, "Tail Animator/Deflection"); } }
        public static Texture WindIcon { get { return LoadTexture(ref windIcon, "Tail Animator/Wind"); } }
        public static Texture WindIconBlue { get { return LoadTexture(ref windIconBlue, "Tail Animator/TailAnimatorWindIconSmall"); } }
        public static Texture CurveIcon { get { return curveIcon ?? (curveIcon = FGUI_Resources.Tex_Curve); } }
        public static GUIStyle SmallStyle { get { return smallStyle ?? (smallStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic }); } }

        private static Texture2D tailAnimatorIcon;
        private static Texture2D wavingIcon;
        private static Texture2D partialBlendIcon;
        private static Texture2D ikIcon;
        private static Texture2D deflectionIcon;
        private static Texture windIcon;
        private static Texture windIconBlue;
        private static Texture curveIcon;
        private static GUIStyle smallStyle;

        public static GUIStyle BGInBoxBlankStylePadded { get { if (__inBoxBlankStylePad != null) return __inBoxBlankStylePad; __inBoxBlankStylePad = new GUIStyle(); __inBoxBlankStylePad.padding = new RectOffset(16, 12, 10, 10); __inBoxBlankStylePad.margin = new RectOffset(0, 0, 0, 0); return __inBoxBlankStylePad; } }
        private static GUIStyle __inBoxBlankStylePad = null;

        public static void SetupLanguage()
        {
            SelectedLanguage = (Language)PlayerPrefs.GetInt("FimposLang", 0);

            if (languageFile == null) languageFile = Resources.Load<TextAsset>("Tail Animator/TailAnimator_Langs");
            if (languageFile == null)
            {
                Debug.LogError("No lang file! You moved it from Editor/Resources/Tail Animator/ ???");
                languageTexts = null;
                return;
            }

            var xml = new XmlDocument();
            xml.LoadXml(languageFile.text);

            languageTexts = new Hashtable();
            var element = xml.DocumentElement[SelectedLanguage.ToString()];
            if (element == null)
            {
                Debug.LogError("The specified language does not exist: " + SelectedLanguage);
                return;
            }

            var elementEnumerator = element.GetEnumerator();
            while (elementEnumerator.MoveNext())
            {
                var item = (XmlElement)elementEnumerator.Current;
                languageTexts[item.GetAttribute("name")] = item.InnerText;
            }
        }

        public static string Localize(string title)
        {
            if (languageTexts == null || !languageTexts.ContainsKey(title)) return title;

            string translated = languageTexts[title] as string;
            return string.IsNullOrEmpty(translated) ? title : translated;
        }

        public static bool UsesLargeLanguageLabels()
        {
            return SelectedLanguage == Language.中文 || SelectedLanguage == Language.日本語;
        }

        public static bool DrawCategoryButton<TCategory>(TCategory selectedCategory, TCategory targetCategory, Texture icon, string label, bool useWideLabel, int height)
            where TCategory : struct
        {
            Color previousBackgroundColor = GUI.backgroundColor;
            if (EqualityComparer<TCategory>.Default.Equals(selectedCategory, targetCategory)) GUI.backgroundColor = new Color(0.1f, 1f, 0.2f, 1f);

            GUIContent content = useWideLabel ? new GUIContent("  " + label, icon) : new GUIContent(icon, label);
            bool clicked = GUILayout.Button(content, FGUI_Resources.ButtonStyle, GUILayout.Height(height));
            GUI.backgroundColor = previousBackgroundColor;
            return clicked;
        }

        public static void DrawSwitchButton(ref bool enabled, string tooltip, Texture icon, SerializedObject serializedObject, Color disabledColor)
        {
            EditorGUI.BeginChangeCheck();
            Color previousColor = GUI.color;
            GUI.color = enabled ? new Color(0.9f, 0.9f, 0.9f, 1f) : disabledColor;

            if (GUILayout.Button(new GUIContent(icon, tooltip), EditorStyles.miniButtonRight, GUILayout.Width(20), GUILayout.Height(16))) enabled = !enabled;

            GUI.color = previousColor;
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }

        public static bool IsChildOf(Transform child, Transform rootParent)
        {
            for (Transform parent = child; parent != null; parent = parent.parent)
                if (parent == rootParent) return true;

            return false;
        }

        public static Transform GetLastChild(Transform rootParent)
        {
            Transform child = rootParent;
            while (child.childCount > 0) child = child.GetChild(0);
            return child;
        }

        public static List<TComponent> GetSelectedComponents<TComponent>() where TComponent : Component
        {
            var selectedComponents = new List<TComponent>();
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                TComponent component = selectedObject.GetComponent<TComponent>();
                if (component != null && !selectedComponents.Contains(component)) selectedComponents.Add(component);
            }

            return selectedComponents;
        }

        private static Texture2D LoadTexture(ref Texture2D texture, string path)
        {
            return texture ?? (texture = Resources.Load<Texture2D>(path));
        }

        private static Texture LoadTexture(ref Texture texture, string path)
        {
            return texture ?? (texture = Resources.Load<Texture2D>(path));
        }
    }
}
