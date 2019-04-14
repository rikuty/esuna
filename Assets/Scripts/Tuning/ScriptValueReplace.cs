using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ゲームオブジェクトにアタッチして実行ボタンを押下すると、Processブロックの処理が実行されます。
/// isChildrenがtrueの場合、子オブジェクトも含めて処理されます。
/// </summary>
public class ScriptValueReplace : MonoBehaviour
{
    [SerializeField] private bool isChildren = true;

    #if UNITY_EDITOR

    private void Execute()
    {
        if (!Application.isPlaying) {
            this.Execute(this.transform);
        }
    }

    private void Execute(Transform target)
    {
        #region Process
        /*
        Component component = target.GetComponent<Component>();
        if (component != null) {
            Debug.Log("[ScriptValueReplace] " + target.name);
        }
        */
        #endregion

        foreach (Transform child in target) {
            this.Execute(child);
        }
    }

    [CustomEditor(typeof(ScriptValueReplace))]
    private class InspecterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("実行")) {
                ScriptValueReplace scriptValueReplace = this.target as ScriptValueReplace;
                if (scriptValueReplace != null) {
                    scriptValueReplace.Execute();
                }
            }
        }
    }
    #endif
}