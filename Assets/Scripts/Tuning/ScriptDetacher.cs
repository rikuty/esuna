using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ゲームオブジェクトにアタッチして実行ボタンを押下すると、指定の名前のスクリプトをデタッチします。
/// isChildrenがtrueの場合、子オブジェクトも含めて処理されます。
/// </summary>
public class ScriptDetacher : MonoBehaviour
{
    [SerializeField] private string targetScriptName;
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
        Component component = target.GetComponent(this.targetScriptName);
        if (component != null) {
            UnityEngine.Object.DestroyImmediate(component);
            Debug.Log("[ScriptDetacher] " + string.Format("Detach Script: {0}, Object Name: {1}", this.targetScriptName, target.name));
        }
        foreach (Transform child in target) {
            this.Execute(child);
        }
    }

    [CustomEditor(typeof(ScriptDetacher))]
    private class InspecterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("実行")) {
                ScriptDetacher scriptDetacher = this.target as ScriptDetacher;
                if (scriptDetacher != null) {
                    scriptDetacher.Execute();
                }
            }
        }
    }
    #endif
}