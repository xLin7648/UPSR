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
                    return (GetCurrentUnixTimeMillis() - this.startTime.Value) * 1 / 1000f;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    public long? startTime;
    public long? pauseTime;


    public AudioSource asu;
    public GameUpdateManager gum;
    public Slider slider;

    public bool isPaused;

    private bool PreviDirty;

    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long GetCurrentUnixTimeMillis()
    {
        // 计算当前 UTC 时间与 Unix 纪元的时间差
        TimeSpan timeSpan = DateTime.UtcNow - UnixEpoch;
        return (long)timeSpan.TotalMilliseconds;
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

        startTime = GetCurrentUnixTimeMillis() + Moffset - e * 1000;
        isPaused = false;
        PreviDirty = false;
    }

    private void Check()
    {
        if (isPaused) return;

        /*var AuTime = asu.time;

        if (Mathf.Abs(AuTime - CurTime) >= 0.1f) // 100ms
        {
            Debug.Log($"AuTime: {AuTime}, Time: {CurTime}");

            // 计算时间差值
            TimeDiff = Time.time - AuTime;

            timeBgm = (DateTime.UtcNow.Millisecond - TimeDiff) / 1e3f + curTime;

            CurTime = AuTime;
        }
        else
        {
            CurTime = Time.time - TimeDiff;
        }*/

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
            this.pauseTime = GetCurrentUnixTimeMillis();
            asu.Pause();
        }
        else
        {
            this.startTime = GetCurrentUnixTimeMillis() - (this.pauseTime - this.startTime) + this.Moffset;
            this.pauseTime = null;

            asu.Play();
        }
    }
}
