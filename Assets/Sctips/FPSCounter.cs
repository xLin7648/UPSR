using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI; // ���ʹ�ô�ͳText

public class FPSCounter : MonoBehaviour
{
    // ����ʹ�õ��������ѡ������һ����
    [SerializeField] private Text fpsTextLegacy; // ��ͳUI�汾

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

        // ����UI��ʾ
        if (fpsTextLegacy != null)
            fpsTextLegacy.text = FormatFPS(currentFPS);
    }

    private string FormatFPS(float fps)
    {
        // ����֡��������ɫ
        string colorHex = fps > 50 ? "#00ff00" :  // ��ɫ
                          fps > 30 ? "#ffff00" :  // ��ɫ
                          "#ff0000";              // ��ɫ

        return $"<color={colorHex}>FPS: {fps:F1}</color>";
    }
}
