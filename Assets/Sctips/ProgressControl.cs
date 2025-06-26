using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressControl : MonoBehaviour
{
    public int e;
    public float offset;
    public int Moffset { get => (int)(offset * 1000); }

    public float nowTime {
        get {
            if (this.startTime.HasValue)
            {
                if (this.pauseTime.HasValue)
                {
                    return (this.pauseTime.Value - this.startTime.Value) * 1 / 1000f;
                }
                else
                {
                    return (GetTime() - this.startTime.Value) * 1 / 1000f;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    public float? startTime;
    public float? pauseTime;


    public AudioSource asu;
    public GameUpdateManager gum;
    public Slider slider;

    public bool isPaused;

    private bool PreviDirty;

    public static float GetTime()
    {
        return Time.time * 1000;
    }

    public void Play()
    {
        if (startTime.HasValue) return;
        StartCoroutine(PlayM());
    }

    IEnumerator PlayM()
    {
        gum.BackupControls();
        var aust = AudioSettings.dspTime + 3.0f;
        asu.PlayScheduled(aust);

        while (AudioSettings.dspTime < aust) {
            yield return null;
        }

        asu.time = e;

        startTime = GetTime() + Moffset - e * 1000;
        isPaused = false;
        PreviDirty = false;
    }

    private void Check()
    {
        if (isPaused) return;

        var AuTime = asu.time;

        var nowTime = this.nowTime;

        if (Mathf.Abs(AuTime - nowTime) >= 0.1f) // 100ms
        {
            Debug.Log($"AuTime: {AuTime}, Time: {nowTime}");

            Pause();
            Pause();
        }

        slider.SetValueWithoutNotify(nowTime / asu.clip.length);
    }

    public void SetSlider(float value)
    {
        Pause();

        var newTime = value * asu.clip.length;

        if (newTime <= nowTime && !PreviDirty)
        {
            gum.ResetControls();
            gum.ResetNotesScore();
            PreviDirty = true;
        }
        else
        {
            PreviDirty = false;
        }

        asu.time = newTime;
    }

    private void Update()
    {
        Check();
    }

    public void Pause()
    {
        if (!startTime.HasValue) return;

        this.isPaused = !this.isPaused;

        if (this.isPaused)
        {
            // 时间 + 音频和谱面的偏移
            this.pauseTime = GetTime() + Mathf.Abs(asu.time - nowTime) * 1000;
            asu.Pause();
        }
        else
        {
            this.startTime = GetTime() - (this.pauseTime - this.startTime) + this.Moffset;
            this.pauseTime = null;

            asu.Play();
        }
    }
}
