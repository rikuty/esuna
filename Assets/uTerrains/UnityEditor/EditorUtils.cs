#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using UltimateTerrains;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    public static class EditorUtils
    {
        public static readonly Color MaterialYellow = new Color32(255, 237, 99, 90);
        public static readonly Color GrassGreen = new Color32(111, 222, 126, 90);
        public static readonly Color VoxelBlue = new Color32(128, 208, 255, 90);
        
        public static class EditorZoomArea
        {
            private const float kEditorWindowTabHeight = 21.0f;
            private static Matrix4x4 _prevGuiMatrix;
 
            public static Rect Begin(float zoomScale, Rect screenCoordsArea)
            {
                GUI.EndGroup();        // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.
 
                Rect clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.TopLeft());
                clippedArea.y += kEditorWindowTabHeight;
                GUI.BeginGroup(clippedArea);
 
                _prevGuiMatrix = GUI.matrix;
                Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
                Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
                GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
 
                return clippedArea;
            }
 
            public static void End()
            {
                GUI.matrix = _prevGuiMatrix;
                GUI.EndGroup();
                GUI.BeginGroup(new Rect(0.0f, kEditorWindowTabHeight, Screen.width, Screen.height));
            }
        }

        public static void CenteredLabelField(string text, params GUILayoutOption[] options)
        {
            var style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
            EditorGUILayout.LabelField(text, style, options);
        }

        public static void CenteredBoxedLabelField(string text, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            CenteredLabelField(text, options);
            EditorGUILayout.EndVertical();
        }


        /// <summary>
        ///   <para>Make an X &amp; Y field for entering a Vector2.</para>
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the field.</param>
        /// <param name="label">Label to display above the field.</param>
        /// <param name="value">The value to edit.</param>
        /// <returns>
        ///   <para>The value entered by the user.</para>
        /// </returns>
        public static Vector2 MinMaxField(string label, Vector2 value, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(label, options);
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 28;
            EditorGUIUtility.fieldWidth = 30;
            value.x = EditorGUILayout.FloatField("Min", value.x, GUILayout.MaxWidth(60));
            value.y = EditorGUILayout.FloatField("Max", value.y, GUILayout.MaxWidth(60));
            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return value;
        }

        private static Vector2 MinMaxField(Rect position, Vector2 value)
        {
            var sVector2Floats = new[] {value.x, value.y};
            position.height = 16f;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MultiFloatField(position, new[] {new GUIContent("Min"), new GUIContent("Max")}, sVector2Floats);
            if (EditorGUI.EndChangeCheck()) {
                value.x = sVector2Floats[0];
                value.y = sVector2Floats[1];
            }

            return value;
        }


        public static MonoScript GetMonoScriptOf(ScriptableObject script)
        {
            if (script != null) {
                return MonoScript.FromScriptableObject(script);
            }

            return null;
        }

        public static bool BeginGUIEnable(bool enable)
        {
            var guiEnabled = GUI.enabled;
            GUI.enabled = enable;
            return guiEnabled;
        }

        public static void EndGUIEnable(bool guiEnabled)
        {
            GUI.enabled = guiEnabled;
        }

        public static void LabelWordWrap(string text)
        {
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.wordWrap = true;
            EditorGUILayout.LabelField(text, labelStyle);
        }

        public static int VoxelTypeField(string label, int currentIndex, VoxelTypeSet set)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(label);
            }

            var options = new List<string>(set.VoxelTypeNames);
            options.Insert(0, " ");
            var newIndex = EditorGUILayout.Popup(currentIndex + 1, options.ToArray()) - 1;
            EditorGUILayout.EndHorizontal();
            return newIndex;
        }

        public static int VoxelTypeFieldMandatory(string label, int currentIndex, VoxelTypeSet set)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(label);
            }

            var options = new List<string>(set.VoxelTypeNames);
            var newIndex = EditorGUILayout.Popup(currentIndex, options.ToArray());
            EditorGUILayout.EndHorizontal();
            return newIndex;
        }
        
        public static int BiomeFieldMandatory(string label, int currentIndex, GeneratorModulesComponent generatorModulesComponent)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(label, GUILayout.ExpandWidth(false));
            }

            var options = new List<string>(generatorModulesComponent.Biomes.Count);
            foreach (var biome in generatorModulesComponent.Biomes) {
                options.Add(biome.Name);
            }
            var newIndex = EditorGUILayout.Popup(currentIndex, options.ToArray(), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            return newIndex;
        }

        public static VoxelType GetVoxelTypeFromIndex(int index, VoxelTypeSet set)
        {
            VoxelType voxelType = null;
            if (index >= 0) {
                voxelType = set.GetVoxelType((ushort) index);
            }

            return voxelType;
        }

        public static T GetFieldValue<T>(object instance, string fieldName) where T : class
        {
            if (instance == null) return null;

            var field = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            return field != null ? (T) field.GetValue(instance) : null;
        }

        public static GUIStyle FoldoutGUIStyle(bool foldout)
        {
            var style = new GUIStyle(EditorStyles.foldout);
            style.margin = new RectOffset(16, 0, 0, 0);
            style.fontStyle = foldout ? FontStyle.Bold : FontStyle.Normal;
            return style;
        }

        public static void BeginColoredVerticalBox(Color color)
        {
            var bkgColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            EditorGUILayout.BeginVertical("Box");
            GUI.backgroundColor = bkgColor;
        }

        public static void EndColoredVerticalBox()
        {
            EditorGUILayout.EndVertical();
        }

        public static Type TypeOf(object currentObject)
        {
            return currentObject != null ? currentObject.GetType() : null;
        }

        public static Type CustomTypeFieldMandatory(string label, Type currentType, Type[] availableTypes)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(label);
            }

            var currentIndex = Array.IndexOf(availableTypes, currentType);
            var availableTypesString = new string[availableTypes.Length];
            for (var i = 0; i < availableTypes.Length; ++i) {
                availableTypesString[i] = GetPrettyName(availableTypes[i]);
            }

            var newIndex = EditorGUILayout.Popup(currentIndex, availableTypesString);
            EditorGUILayout.EndHorizontal();
            return newIndex >= 0 && newIndex < availableTypes.Length ? availableTypes[newIndex] : null;
        }

        public static Type CustomTypeField(string label, string emptyLabel, Type currentType, Type[] availableTypes)
        {
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(label)) {
                EditorGUILayout.LabelField(label);
            }

            var currentIndex = Array.IndexOf(availableTypes, currentType) + 1;
            var availableTypesString = new string[availableTypes.Length + 1];
            availableTypesString[0] = emptyLabel;
            for (var i = 0; i < availableTypes.Length; ++i) {
                availableTypesString[i + 1] = GetPrettyName(availableTypes[i]);
            }

            var newIndex = EditorGUILayout.Popup(currentIndex, availableTypesString);
            EditorGUILayout.EndHorizontal();
            return newIndex >= 1 && newIndex < availableTypes.Length + 1 ? availableTypes[newIndex - 1] : null;
        }

        public static Type[] GetAvailableTypes(Type baseType)
        {
            var availableTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => baseType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract).ToArray();
            // Sort by alphabetical order
            Array.Sort(availableTypes, (t1, t2) => string.Compare(t1.Name, t2.Name, StringComparison.Ordinal));
            return availableTypes;
        }

        public static string GetPrettyName(Type type)
        {
            var attrs = type.GetCustomAttributes(true);
            foreach (var attr in attrs) {
                var nameAttr = attr as PrettyTypeName;
                if (nameAttr != null) {
                    return nameAttr.Name;
                }
            }

            return type.Name;
        }
    }
}
#endif