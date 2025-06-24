using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressControl : MonoBehaviour
{
    public float Offset;

    public float TimeDiff;
    public float nowTime { get => CurTime - Offset; }
    public float CurTime;
    public AudioSource asu;
    public GameUpdateManager gum;
    public Slider slider;

    public float e;

    public bool isPlay;

    private bool PreviDirty;

    public void Play()
    {
        StartCoroutine(PlayM());
    }

    IEnumerator PlayM()
    {
        gum.BackupControls();
        var startTime = AudioSettings.dspTime + 3.0f;
        asu.PlayScheduled(startTime);

        while (AudioSettings.dspTime < startTime) {
            yield return null;
        }

        isPlay = true;
        PreviDirty = false;
        TimeDiff = Time.time - asu.time;
    }

    private void Check()
    {
        if (!isPlay) return;

        var AuTime = asu.time;

        if (Mathf.Abs(AuTime - CurTime) >= 0.1f) // 100ms
        {
            Debug.Log($"AuTime: {AuTime}, Time: {CurTime}");

            // 计算时间差值
            TimeDiff = Time.time - AuTime;

            CurTime = AuTime;
        }
        else
        {
            CurTime = Time.time - TimeDiff;
        }

        slider.SetValueWithoutNotify(CurTime / asu.clip.length);
    }

    public void SetSlider(float value)
    {
        isPlay = false;
        asu.Pause();

        var newTime = value * asu.clip.length;

        if (newTime <= CurTime && !PreviDirty)
        {
            gum.ResetControls();
            gum.ResetNotesScore();
            PreviDirty = true;
        }
        else
        {
            PreviDirty = false;
        }

        asu.time = CurTime = newTime;
    }

    private void Update()
    {
        Check();
    }
}
