#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UltimateTerrainsEditor
{
    public class ListInspector<T> where T : class
    {
        private const string Confirmation = "Confirmation";
        private const string Yes = "Yes";
        private const string No = "No";

        public delegate bool CanRemoveItemDelegate(int index);

        public delegate T DisplayItemDelegate(T item, int index);

        public delegate T AddItemDelegate();

        private readonly Texture2D deleteButtonIcon;
        private readonly GUIStyle deleteButtonStyle;
        private readonly Texture2D upButtonIcon;
        private readonly GUIStyle upButtonStyle;
        private readonly string addButtonText;
        private readonly Color? addButtonColor;
        private readonly string removeText;
        private readonly bool alignRemoveButtonHorizontaly;
        private readonly DisplayItemDelegate displayItemDelegate;
        private readonly AddItemDelegate addItemDelegate;
        private readonly CanRemoveItemDelegate canRemoveItemDelegate;
        private readonly bool isUpEnabled;
        private readonly string foldoutLabelFieldName;

        public ListInspector(string addButtonText, 
                             Color? addButtonColor, 
                             string removeText, 
                             bool alignRemoveButtonHorizontaly, 
                             DisplayItemDelegate displayItemDelegate, 
                             AddItemDelegate addItemDelegate, 
                             CanRemoveItemDelegate canRemoveItemDelegate,
                             bool isUpEnabled = false,
                             string foldoutLabelFieldName = null)
        {
            deleteButtonIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/remove");
            deleteButtonStyle = new GUIStyle();
            deleteButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            deleteButtonStyle.margin = new RectOffset(0, 0, 4, 0);

            upButtonIcon = Resources.Load<Texture2D>("uTerrainsEditorResources/up");
            upButtonStyle = new GUIStyle();
            upButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            upButtonStyle.margin = new RectOffset(4, 0, 4, 0);
            upButtonStyle.alignment = TextAnchor.MiddleCenter;

            this.addButtonText = addButtonText;
            this.addButtonColor = addButtonColor;
            this.removeText = removeText;
            this.alignRemoveButtonHorizontaly = alignRemoveButtonHorizontaly;
            this.displayItemDelegate = displayItemDelegate;
            this.addItemDelegate = addItemDelegate;
            this.canRemoveItemDelegate = canRemoveItemDelegate;
            this.isUpEnabled = isUpEnabled;
            this.foldoutLabelFieldName = foldoutLabelFieldName;
        }

        public void DisplayArrayInspector(ref T[] arrayObj)
        {
            DisplayArrayInspector(ref arrayObj, null);
        }

        public void DisplayArrayInspector(ref T[] arrayObj, List<bool> foldout)
        {
            List<T> list;
            if (arrayObj != null) {
                list = new List<T>(arrayObj);
            } else {
                list = new List<T>();
            }

            DisplayListInspector(list, foldout);
            arrayObj = list.ToArray();
        }

        public void DisplayListInspector(List<T> list)
        {
            DisplayListInspector(list, null);
        }

        public void DisplayListInspector(List<T> list, List<bool> foldout)
        {
            PrepareFoldout(list, foldout);
            var indexToRemove = -1;
            var indexToUp = -1;

            for (var i = 0; i < list.Count; ++i) {
                if (!HandleFoldout(list, foldout, i, ref indexToRemove, ref indexToUp)) continue;

                if (alignRemoveButtonHorizontaly) {
                    EditorGUILayout.BeginHorizontal();
                } else if (foldout == null) {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                }

                if (foldout == null) {
                    EditorGUILayout.BeginHorizontal();
                    HandleRemoveUpButtons(i, ref indexToRemove, ref indexToUp);
                    EditorGUILayout.EndHorizontal();
                }

                list[i] = displayItemDelegate(list[i], i);

                if (alignRemoveButtonHorizontaly) {
                    EditorGUILayout.EndHorizontal();
                } else {
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
            }

            if (indexToRemove >= 0) {
                list.RemoveAt(indexToRemove);
                if (foldout != null) foldout.RemoveAt(indexToRemove);
            } else if (indexToUp > 0) {
                var swap = list[indexToUp - 1];
                list[indexToUp - 1] = list[indexToUp];
                list[indexToUp] = swap;
                if (foldout != null) {
                    var swapFoldout = foldout[indexToUp - 1];
                    foldout[indexToUp - 1] = foldout[indexToUp];
                    foldout[indexToUp] = swapFoldout;
                }
            }

            var guiEnabled = EditorUtils.BeginGUIEnable(GUI.enabled && (list.Count == 0 || list[list.Count - 1] != null));
            
            var bkgColor = GUI.backgroundColor;
            if (addButtonColor.HasValue) GUI.backgroundColor = addButtonColor.Value;
            if (GUILayout.Button(addButtonText)) {
                list.Add(addItemDelegate());
                if (foldout != null) foldout.Add(false);
            }

            EditorUtils.EndGUIEnable(guiEnabled);

            GUI.backgroundColor = bkgColor;
        }

        private void HandleRemoveUpButtons(int i, ref int indexToRemove, ref int indexToUp)
        {
            if (canRemoveItemDelegate(i) &&
                GUILayout.Button(deleteButtonIcon, deleteButtonStyle, GUILayout.Width(12), GUILayout.Height(12)) &&
                (string.IsNullOrEmpty(removeText) || EditorUtility.DisplayDialog(Confirmation, removeText, Yes, No))) {
                indexToRemove = i;
            }

            if (isUpEnabled && i > 0 && GUILayout.Button(upButtonIcon, upButtonStyle, GUILayout.Width(12), GUILayout.Height(12))) {
                indexToUp = i;
            }
        }

        private bool HandleFoldout(List<T> list, List<bool> foldout, int i, ref int indexToRemove, ref int indexToUp)
        {
            if (foldout == null) return true;
            
            var name = foldoutLabelFieldName != null ? EditorUtils.GetFieldValue<string>(list[i], foldoutLabelFieldName) : null;
            
            if (foldout[i])
                EditorGUILayout.Space();

            var style = new GUIStyle(EditorStyles.helpBox);
            var margin = EditorStyles.helpBox.margin;
            margin.top = 2;
            margin.bottom = 0;
            style.margin = margin;
            EditorGUILayout.BeginVertical(style);
            
            EditorGUILayout.BeginHorizontal();
            foldout[i] = EditorGUILayout.Foldout(foldout[i], name ?? "Element " + i, EditorUtils.FoldoutGUIStyle(foldout[i]));
            HandleRemoveUpButtons(i, ref indexToRemove, ref indexToUp);
            EditorGUILayout.EndHorizontal();
            
            if (!foldout[i])
                EditorGUILayout.EndVertical();
            
            return foldout[i];
        }

        private static void PrepareFoldout(List<T> list, List<bool> foldout)
        {
            if (foldout != null && foldout.Count < list.Count) {
                for (var i = foldout.Count; i < list.Count; ++i) {
                    foldout.Add(false);
                }
            }
        }
    }
}
#endif