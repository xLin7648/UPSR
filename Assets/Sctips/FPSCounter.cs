using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI; // 如果使用传统Text

public class FPSCounter : MonoBehaviour
{
    // 根据使用的组件类型选择其中一个：
    [SerializeField] private Text fpsTextLegacy; // 传统UI版本

    private float currentFPS;

    void Start()
    {
        Application.runInBackground = true;
        // Application.targetFrameRate = int.Parse(File.ReadAllText("C://s.txt"));

         if (fpsTextLegacy == null)
            fpsTextLegacy = GetComponent<Text>();
    }

    void Update()
    {
        currentFPS = 1f / Time.deltaTime;

        // 更新UI显示
        if (fpsTextLegacy != null)
            fpsTextLegacy.text = FormatFPS(currentFPS);
    }

    private string FormatFPS(float fps)
    {
        // 根据帧率设置颜色
        string colorHex = fps > 50 ? "#00ff00" :  // 绿色
                          fps > 30 ? "#ffff00" :  // 黄色
                          "#ff0000";              // 红色

        return $"<color={colorHex}>FPS: {fps:F1}</color>";
    }
}
