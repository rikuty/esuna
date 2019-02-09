using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

[CustomEditor(typeof(BodyScale))]//拡張するクラスを指定
public class ExampleScriptEditor : Editor
{

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //targetを変換して対象を取得
        BodyScale bodyScale = target as BodyScale;

        //ボタンを表示
        if (GUILayout.Button("Button"))
        {
            bodyScale.CallbackFromButton();
        }
    }

}


public class BodyScale : UtilComponent {

    public Transform back;
    public Transform shoulder;
    public Transform hand;

    public Dictionary<int, Dictionary<string, Dictionary<string, Vector3>>> goalBodyTransformDictionary;

    /// <summary>
    /// 現在番号
    /// </summary>
    public Text index;
    /// <summary>
    /// 屈曲角度（伸展では使用しない）
    /// </summary>
    public InputField backRotationX;
    /// <summary>
    /// ユーザ肩位置
    /// </summary>
    public InputField shoulderPositionY;
    /// <summary>
    /// 回旋角度
    /// </summary>
    public InputField shoulderRotationY;
    /// <summary>
    /// 伸展角度（肩代償含む）
    /// </summary>
    public InputField shoulderRotationX;
    /// <summary>
    /// ユーザ腕リーチ
    /// </summary>
    public InputField handPositionZ;

    /// <summary>
    /// 屈曲角度（伸展では使用しない）
    /// </summary>
    public float strBackRotationX;
    /// <summary>
    /// ユーザ肩位置
    /// </summary>
    public float strShoulderPositionY;
    /// <summary>
    /// 回旋角度
    /// </summary>
    public float strShoulderRotationY;
    /// <summary>
    /// 伸展角度（肩代償含む）
    /// </summary>
    public float strShoulderRotationX;
    /// <summary>
    /// ユーザ腕リーチ
    /// </summary>
    public float strHandPositionZ;


    public void SetDisplay(int index)
    {
        SetLabel(this.index, index);
        strBackRotationX = goalBodyTransformDictionary[index]["back"]["rotation"].x;
        strShoulderPositionY = goalBodyTransformDictionary[index]["shoulder"]["position"].y;
        strShoulderRotationX = goalBodyTransformDictionary[index]["shoulder"]["rotation"].x;
        strShoulderRotationY = goalBodyTransformDictionary[index]["shoulder"]["rotation"].y;
        strHandPositionZ = goalBodyTransformDictionary[index]["hand"]["position"].z;

        backRotationX.text = strBackRotationX.ToString();
        shoulderPositionY.text = strShoulderPositionY.ToString();
        shoulderRotationX.text = strShoulderRotationX.ToString();
        shoulderRotationY.text = strShoulderRotationY.ToString();
        handPositionZ.text = strHandPositionZ.ToString();
    }

    public void CallbackFromButton()
    {
        Dictionary<string, Vector3> backTransform = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(strBackRotationX, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, strShoulderPositionY, 0f)},
            {"rotation", new Vector3(strShoulderRotationX, strShoulderRotationY, 0f)}
        };

        Dictionary<string, Vector3> handTransform = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, strHandPositionZ)},
            {"rotation", new Vector3(0f, 0f, 0f)}
        };

        goalBodyTransformDictionary[int.Parse(this.index.text)] = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            {"back", backTransform },
            {"shoulder", shoulderTransform },
            {"hand", handTransform }
        };

        SetTransform(int.Parse(this.index.text));
        SetDisplay(int.Parse(this.index.text));
    }


    public void SetTransform(int index)
    {
        back.localPosition = goalBodyTransformDictionary[index]["back"]["position"];
        back.localRotation = Quaternion.Euler(goalBodyTransformDictionary[index]["back"]["rotation"]);
        shoulder.localPosition = goalBodyTransformDictionary[index]["shoulder"]["position"];
        shoulder.localRotation = Quaternion.Euler(goalBodyTransformDictionary[index]["shoulder"]["rotation"]);
        hand.localPosition = goalBodyTransformDictionary[index]["hand"]["position"];
        // local でない
        hand.rotation = Quaternion.Euler(goalBodyTransformDictionary[index]["hand"]["rotation"]);
    }



    // Use this for initialization
    void Awake() {
        Dictionary<string, Vector3> backTransform1 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(0f, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform1 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0.5f, 0f)},
            {"rotation", new Vector3(10f, -90f, 0f)}
        };

        Dictionary<string, Vector3> handTransform1 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0.8f)},
            {"rotation", new Vector3(0f, 0f, -90f)}
        };

        Dictionary<string, Dictionary<string, Vector3>> bodyTransformDictionary1 = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            { "back", backTransform1 },
            { "shoulder", shoulderTransform1 },
            { "hand", handTransform1 },
        };

        Dictionary<string, Vector3> backTransform2 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(0f, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform2 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0.5f, 0f)},
            {"rotation", new Vector3(10f, 90f, 0f)}
        };

        Dictionary<string, Vector3> handTransform2 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0.8f)},
            {"rotation", new Vector3(0f, 0f, 90f)}
        };

        Dictionary<string, Dictionary<string, Vector3>> bodyTransformDictionary2 = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            { "back", backTransform2 },
            { "shoulder", shoulderTransform2 },
            { "hand", handTransform2 },
        };

        Dictionary<string, Vector3> backTransform3 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(0f, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform3 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0.5f, 0f)},
            {"rotation", new Vector3(-45f, -90f, 0f)}
        };

        Dictionary<string, Vector3> handTransform3 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0.8f)},
            {"rotation", new Vector3(0f, 0f, -135f)}
        };

        Dictionary<string, Dictionary<string, Vector3>> bodyTransformDictionary3 = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            { "back", backTransform3 },
            { "shoulder", shoulderTransform3 },
            { "hand", handTransform3 },
        };

        Dictionary<string, Vector3> backTransform4 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(0f, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform4 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0.5f, 0f)},
            {"rotation", new Vector3(-70f, 0f, 0f)}
        };

        Dictionary<string, Vector3> handTransform4 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0.8f)},
            {"rotation", new Vector3(-180f, 0f, 0f)}
        };

        Dictionary<string, Dictionary<string, Vector3>> bodyTransformDictionary4 = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            { "back", backTransform4 },
            { "shoulder", shoulderTransform4 },
            { "hand", handTransform4 },
        };

        Dictionary<string, Vector3> backTransform5 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(0f, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform5 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0.5f, 0f)},
            {"rotation", new Vector3(-45, 90f, 0f)}
        };

        Dictionary<string, Vector3> handTransform5 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0.8f)},
            {"rotation", new Vector3(0f, 0f, 135f)}
        };

        Dictionary<string, Dictionary<string, Vector3>> bodyTransformDictionary5 = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            { "back", backTransform5 },
            { "shoulder", shoulderTransform5 },
            { "hand", handTransform5 },
        };

        Dictionary<string, Vector3> backTransform6 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(70f, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform6 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0.5f, 0f)},
            {"rotation", new Vector3(-30f, -80f, 0f)}
        };

        Dictionary<string, Vector3> handTransform6 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0.6f)},
            {"rotation", new Vector3(-30f, 0f, -30f)}
        };

        Dictionary<string, Dictionary<string, Vector3>> bodyTransformDictionary6 = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            { "back", backTransform6 },
            { "shoulder", shoulderTransform6 },
            { "hand", handTransform6 },
        };

        Dictionary<string, Vector3> backTransform7 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(70f, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform7 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0.5f, 0f)},
            {"rotation", new Vector3(-30f, 0f, 0f)}
        };

        Dictionary<string, Vector3> handTransform7 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0.6f)},
            {"rotation", new Vector3(-30f, 0f, 0f)}
        };

        Dictionary<string, Dictionary<string, Vector3>> bodyTransformDictionary7 = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            { "back", backTransform7 },
            { "shoulder", shoulderTransform7 },
            { "hand", handTransform7 },
        };

        Dictionary<string, Vector3> backTransform8 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0f)},
            {"rotation", new Vector3(70f, 0f, 0f)}
        };
        Dictionary<string, Vector3> shoulderTransform8 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0.5f, 0f)},
            {"rotation", new Vector3(-30f, 80f, 0f)}
        };

        Dictionary<string, Vector3> handTransform8 = new Dictionary<string, Vector3>()
        {
            {"position", new Vector3(0f, 0f, 0.6f)},
            {"rotation", new Vector3(-30f, 0f, 30f)}
        };

        Dictionary<string, Dictionary<string, Vector3>> bodyTransformDictionary8 = new Dictionary<string, Dictionary<string, Vector3>>()
        {
            { "back", backTransform8 },
            { "shoulder", shoulderTransform8 },
            { "hand", handTransform8 },
        };

        goalBodyTransformDictionary = new Dictionary<int, Dictionary<string, Dictionary<string, Vector3>>>()
        {
            { 1, bodyTransformDictionary1 },
            { 2, bodyTransformDictionary2 },
            { 3, bodyTransformDictionary3 },
            { 4, bodyTransformDictionary4 },
            { 5, bodyTransformDictionary5 },
            { 6, bodyTransformDictionary6 },
            { 7, bodyTransformDictionary7 },
            { 8, bodyTransformDictionary8 }
        };


    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
