using LeTai.Asset.TranslucentImage;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class C : MonoBehaviour
{
    public TranslucentImage center;
    public TranslucentImage left, right;

    public float jg;

    public void Change(float value)
    {
        var min = (byte)(value * (255 - jg * 2f) + jg);
        var max = (byte)(min + jg);

        center.color = new Color32(max, max, max, 255);
        left.color = right.color = new Color32(min, min, min, 255);
    }

    float remap(float x, float t1, float t2, float s1, float s2)
    {
        return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
    }
}
