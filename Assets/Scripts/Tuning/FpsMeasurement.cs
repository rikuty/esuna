using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// ゲームオブジェクトにアタッチすることでFPSを計測します。
/// スペースキーを押下するとコンソールに直近のFPSを出力します。
/// </summary>
public class FpsMeasurement : MonoBehaviour
{
    private float lastUpdateTime;
    private int frameCount;

    private List<KeyValuePair<int, float>> fpsCache;

    private void Awake()
    {
        this.Prepare();
        this.fpsCache = new List<KeyValuePair<int, float>>();
    }

    private void Update()
    {
        this.frameCount++;

        float interval = Time.realtimeSinceStartup - this.lastUpdateTime;

        if (interval >= 0.5f) {
            float fps = (float)this.frameCount / interval;

            this.fpsCache.Add(new KeyValuePair<int, float>(Time.frameCount, fps));
            if (this.fpsCache.Count >= 100) {
                this.fpsCache.RemoveAt(0);
            }

            this.Prepare();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            this.Dump();
        }
    }

    private void Prepare()
    {
        this.lastUpdateTime = Time.realtimeSinceStartup;
        this.frameCount = 0;
    }

    private void Dump()
    {
        IEnumerable<float> fps = this.fpsCache.Select(x => x.Value);

        string log =
            string.Format("TargetFrameRate: {0} fps, ", Application.targetFrameRate) +
            string.Format("Average: {0}, ", fps.Average()) +
            string.Format("Min: {0} fps, Max: {1} fps\n", fps.Min(), fps.Max());

        List<KeyValuePair<int, float>> dumpList = new List<KeyValuePair<int, float>>(this.fpsCache);
        dumpList.Reverse();
        dumpList.ForEach(x => log += string.Format("[{0}] {1:F2} fps\n", x.Key, x.Value));

        Debug.Log(log);
    }
}