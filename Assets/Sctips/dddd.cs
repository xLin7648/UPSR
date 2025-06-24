using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class dddd : MonoBehaviour
{
    public CanvasScaler canvasScaler;

    void Update()
    {
        var ff = 1920f / 1080f;
        var dd = (float)Screen.width / (float)Screen.height;

        var v = 0f;

        if (dd == ff)
        {
            v = 0.6f;
        }
        else if (dd > ff)
        {
            v = 0.5f;
        }

        canvasScaler.matchWidthOrHeight = v;
    }
}
