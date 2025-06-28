using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum TimerState
{
    Running = 0,
    Pause = 1,
    Stop = 2
}

public class ProgressControl : MonoBehaviour
{
    public float offset;
    public Slider slider;
    public AudioSource audioSource;
    public GameUpdateManager gameUpdateManager;
    public TimerState status { get; private set; } = TimerState.Stop;

    private float startTime = float.NaN;
    private float pausedTime = float.NaN;
    private float _speed = 1f;
    private float _lastSpeedChangedProgress = 0f;

    private Coroutine PlayCoroutine;

    public void Play()
    {
        if (!LevelInformation.chartLoaded)
        {
            return;
        }

        if (PlayCoroutine != null)
            StopCoroutine(PlayCoroutine);

        PlayCoroutine = StartCoroutine(PlayM());

        IEnumerator PlayM()
        {
            var aust = AudioSettings.dspTime + 3.0f;
            audioSource.PlayScheduled(aust);

            while (AudioSettings.dspTime < aust)
            {
                yield return null;
            }

            if (status == TimerState.Pause)
            {
                startTime = Time.time - (pausedTime - startTime);
            }
            else
            {
                startTime = Time.time;
            }

            status = TimerState.Running;
            pausedTime = float.NaN;
        }
    }

    public void Pause()
    {
        if (status == TimerState.Running)
        {
            pausedTime = Time.time;
            status = TimerState.Pause;
            audioSource.Pause();
        }
        else if (status == TimerState.Pause)
        {
            gameUpdateManager.RBK();

            startTime = Time.time - (pausedTime - startTime);
            pausedTime = float.NaN;
            status = TimerState.Running;
            audioSource.Play();
        }
        else
        {
            Play();
        }
    }

    public void Stop()
    {
        if (status == TimerState.Stop) return;

        startTime = float.NaN;
        pausedTime = float.NaN;
        _lastSpeedChangedProgress = 0f;
        status = TimerState.Stop;
        audioSource.Stop();
    }

    // �޸����Seek����
    public void Seek(float targetTime)
    {
        if (status == TimerState.Stop) return;

        // ���㵱ǰʱ����Ŀ��ʱ��Ĳ�ֵ
        float currentTime = this.time;
        float timeDifference = targetTime - currentTime;

        // ֱ�ӵ�����ʼʱ���
        if (status == TimerState.Running)
        {
            startTime -= timeDifference / _speed;
        }
        else if (status == TimerState.Pause)
        {
            // ������ͣ״̬�������ۼƽ���
            _lastSpeedChangedProgress += timeDifference;
        }

        // ������Ƶʱ��
        audioSource.time = targetTime;
    }

    public void SetSlider(float value)
    {
        if (status == TimerState.Stop || !LevelInformation.chartLoaded)
            return;

        pausedTime = Time.time;
        status = TimerState.Pause;
        audioSource.Pause();

        Seek(value * audioSource.clip.length);
    }

    private void Update()
    {
        if (status != TimerState.Running) return;

        var auTime = audioSource.time;
        var nowTime = this.time;

        // ���ʱ�����ֵ���
        if (Mathf.Abs(auTime - nowTime) >= 0.1f)
        {
            if (auTime == 0 && !audioSource.isPlaying)
            {
                Stop();
                Debug.Log("PLAY END");
            }
            else
            {
                Debug.Log($"����ʱ��: AuTime={auTime}, TimerTime={nowTime}");
                Seek(auTime);
            }
        }

        slider.SetValueWithoutNotify(auTime / audioSource.clip.length);
    }

    public float speed
    {
        get => _speed;
        set
        {
            if (status != TimerState.Stop)
            {
                float currentTime = (status == TimerState.Running) ? Time.time : pausedTime;
                _lastSpeedChangedProgress += (currentTime - startTime) * _speed;
            }

            startTime = Time.time;
            if (status == TimerState.Pause) pausedTime = Time.time;
            _speed = VerifyNum(value);
            audioSource.pitch = _speed;
        }
    }

    public float nowTime => time - offset;

    private float time
    {
        get
        {
            if (status == TimerState.Running)
            {
                return (Time.time - startTime) * _speed + _lastSpeedChangedProgress;
            }
            else if (status == TimerState.Pause)
            {
                return (pausedTime - startTime) * _speed + _lastSpeedChangedProgress;
            }
            else
            {
                return float.NaN;
            }
        }
    }

    private float VerifyNum(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            throw new System.ArgumentException("Invalid number value");
        }
        return value;
    }
}
