using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeLineControl : MonoBehaviour
{
    public int index;
    public ChartLoad chartLoad;

    public ProgressControl progressControl;
    public LevelInformation levelInformation;
    public GameUpdateManager gameUpdateManager;

    // Fields
    public GameObject Click; // 0x18
    public GameObject Drag; // 0x20
    public GameObject Hold; // 0x28
    public GameObject Flick; // 0x30
    public Sprite ClickHL; // 0x38
    public Sprite HoldHL0; // 0x40
    public Sprite HoldHL1; // 0x48
    public Sprite DragHL; // 0x50
    public Sprite FlickHL; // 0x58
    public float theta; // 0x7C
    public bool isShowingPic; // 0x80
    public Color picColor; // 0x84
    public List<ChartNote> notesAbove; // 0x98
    public List<ChartNote> notesBelow; // 0xA0
    public List<JudgeLineEvent> judgeLineDisappearEvents; // 0xA8
    public List<JudgeLineEvent> judgeLineMoveEvents; // 0xB0
    public List<JudgeLineEvent> judgeLineRotateEvents; // 0xB8
    public List<SpeedEvent> speedEvents; // 0xC0
    public float[] fingerPositionX = new float[10]; // 0xD0
    public float[] fingerPositionY = new float[10]; // 0xD8
    public int numOfFingers; // 0xE0
    private int nowSpeedIndex; // 0xE4
    private int nowDisappearIndex; // 0xE8
    private int nowMoveIndex; // 0xEC
    private int nowRotateIndex; // 0xF0
    private float nowTime; // 0x104
    private float noteScale; // 0x108
    private bool chordSupport = true; // 0x10C
    private float moveScale; // 0x110
    public SpriteRenderer _judgeLineSpriteRenderer; // 0x118

    private void Start()
    {
        // ��ȡNoteUpdateManager���
        if (levelInformation != null)
        {
            gameUpdateManager = levelInformation.GetComponent<GameUpdateManager>();
        }

        // ��LevelInformation��ȡ��Ӧ�ж�������
        if (levelInformation != null &&
            levelInformation.judgeLineList != null &&
            index < levelInformation.judgeLineList.Count)
        {
            JudgeLine judgeLine = levelInformation.judgeLineList[index];
            judgeLineDisappearEvents = judgeLine.judgeLineDisappearEvents;
            judgeLineMoveEvents = judgeLine.judgeLineMoveEvents;
            judgeLineRotateEvents = judgeLine.judgeLineRotateEvents;
            speedEvents = judgeLine.speedEvents;
            notesAbove = judgeLine.notesAbove;
            notesBelow = judgeLine.notesBelow;
        }

        // ������������
        if (levelInformation != null)
        {
            noteScale = levelInformation.noteScale;
        }

        // ������Ļ�������
        float screenRatio = (float)Screen.height / Screen.width;
        moveScale = 1.0f;

        if (screenRatio > 0.5625f) // 9:16��߱�
        {
            float targetHeight = Screen.width * 0.5625f;
            moveScale = targetHeight / Screen.height;
        }

        // �����ж�����ʧ�¼�
        if (judgeLineDisappearEvents != null && judgeLineDisappearEvents.Count > 0)
        {
            if (chartLoad != null && chartLoad.aPfCisOn)
            {
                // APFCģʽ�µ���ɫ����
                JudgeLineEvent disappearEvent = judgeLineDisappearEvents[0];
                Color color = new Color(1.0f, 1.0f, 0.635f, disappearEvent.start);
                _judgeLineSpriteRenderer.color = color;
            }
            else
            {
                // ��ͨģʽ�µ���ɫ����
                JudgeLineEvent disappearEvent = judgeLineDisappearEvents[0];
                Color color = isShowingPic
                    ? new Color(picColor.r, picColor.g, picColor.b, disappearEvent.start)
                    : new Color(1.0f, 1.0f, 1.0f, disappearEvent.start);
                _judgeLineSpriteRenderer.color = color;
            }
        }
        else
        {
            // ����ʧ�¼�ʱ�Ĵ���
            if (index != 0)
            {
                _judgeLineSpriteRenderer.color = Color.clear;
            }
            else if (chartLoad != null)
            {
                Color color = chartLoad.aPfCisOn
                    ? new Color(1.0f, 1.0f, 0.635f, 1.0f)
                    : (isShowingPic ? picColor : Color.white);
                _judgeLineSpriteRenderer.color = color;
            }
        }

        // ����λ�ú���ת
        Transform transform = this.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);

        // �����ٶ��¼�
        if (levelInformation != null)
        {
            // ����������
            for (int i = 0; i < notesAbove.Count; i++)
            {
                CreateNote(i, true);
            }
            for (int j = 0; j < notesBelow.Count; j++)
            {
                CreateNote(j, false);
            }
        }
    }

    private void UpdateJudgeLineEventsIndex()
    {
        // �����ٶ��¼�����
        UpdateJudgeLineEventIndex(speedEvents, ref nowSpeedIndex);
        // �����ж�����ʧ�¼�����
        UpdateJudgeLineEventIndex(judgeLineDisappearEvents, ref nowDisappearIndex);
        // �����ж����ƶ��¼�����
        UpdateJudgeLineEventIndex(judgeLineMoveEvents, ref nowMoveIndex);
        // �����ж�����ת�¼�����
        UpdateJudgeLineEventIndex(judgeLineRotateEvents, ref nowRotateIndex);
    }

    private void UpdateJudgeLineEventIndex<T>(List<T> lineEvents, ref int idx)
        where T : JudgeLineEventBase
    {
        if (lineEvents == null)
        {
            idx = 0;
            return;
        }

        var eventCount = lineEvents.Count;

        if (idx >= 0 && idx < eventCount)
        {
            // ����������ҵ���һ������Ŀ��ʱ��Ĺؼ�֡
            while (idx + 1 < eventCount)
            {
                T nextKeyframe = lineEvents[idx + 1];
                // ʹ�ùؼ�֡����ʼʱ��Ƚ�
                if (nextKeyframe.startTime > nowTime)
                    break;

                idx++;
            }

            // ���������ȷ����ǰ�ؼ�֡ʱ�䲻����Ŀ��ʱ��
            while (idx > 0)
            {
                T currentKeyframe = lineEvents[idx];
                // ʹ�ùؼ�֡����ʼʱ��Ƚ�
                if (currentKeyframe.startTime <= nowTime)
                    break;

                idx--;
            }
        }
        else
        {
            idx = 0;
        }
    }

    public void UpdateInfo()
    {
        // ����ж�����Ⱦ���Ƿ���Ч
        if (_judgeLineSpriteRenderer != null)
        {
            // ���µ�ǰʱ��
            if (progressControl != null)
            {
                nowTime = progressControl.nowTime;
            }

            // �����¼�����
            UpdateJudgeLineEventsIndex();

            // ������Ϸ��ʼǰ��ʱ��
            if (nowTime < 0.0f)
            {
                return;
            }

            // �����ٶ��¼�
            if (levelInformation != null)
            {
                if (speedEvents != null && levelInformation.floorPositions != null)
                {
                    // ���µذ�λ��
                    SpeedEvent speedEvent = speedEvents[nowSpeedIndex];
                    float newPosition = speedEvent.floorPosition +
                                       (nowTime - speedEvent.startTime) * speedEvent.value;

                    if (index < levelInformation.floorPositions.Length)
                    {
                        levelInformation.floorPositions[index] = newPosition;
                    }
                }
            }

            // ������Ϸ�����е�״̬
            if (/*scoreControl != null && scoreControl.isAllPerfect && */levelInformation != null && levelInformation.aPfCisOn)
            {
                // APFCģʽ�µ���ɫ��ֵ
                JudgeLineEvent disappearEvent = judgeLineDisappearEvents[nowDisappearIndex];
                float alpha = Mathf.Lerp(
                    disappearEvent.start,
                    disappearEvent.end,
                    Mathf.InverseLerp(
                        disappearEvent.startTime,
                        disappearEvent.endTime,
                        nowTime
                    )
                );
                _judgeLineSpriteRenderer.color = new Color(1.0f, 1.0f, 0.635f, alpha);
            }
            else if (/*scoreControl != null && scoreControl.isFullCombo && */levelInformation != null && levelInformation.aPfCisOn)
            {
                // FCģʽ�µ���ɫ��ֵ
                JudgeLineEvent disappearEvent = judgeLineDisappearEvents[nowDisappearIndex];
                float alpha = Mathf.Lerp(
                    disappearEvent.start,
                    disappearEvent.end,
                    Mathf.InverseLerp(
                        disappearEvent.startTime,
                        disappearEvent.endTime,
                        nowTime
                    )
                );
                _judgeLineSpriteRenderer.color = new Color(0.627f, 0.125f, 0.941f, alpha);
            }
            else
            {
                // ��ͨģʽ�µ���ɫ��ֵ
                JudgeLineEvent disappearEvent = judgeLineDisappearEvents[nowDisappearIndex];
                float alpha = Mathf.Lerp(
                    disappearEvent.start,
                    disappearEvent.end,
                    Mathf.InverseLerp(
                        disappearEvent.startTime,
                        disappearEvent.endTime,
                        nowTime
                    )
                );

                Color color = isShowingPic
                    ? new Color(picColor.r, picColor.g, picColor.b, alpha)
                    : new Color(1.0f, 1.0f, 1.0f, alpha);

                _judgeLineSpriteRenderer.color = color;
            }

            // �����ж���λ��
            if (judgeLineMoveEvents != null)
            {
                JudgeLineEvent moveEvent = judgeLineMoveEvents[nowMoveIndex];

                float posX = Mathf.Lerp(
                    moveEvent.start,
                    moveEvent.end,
                    Mathf.InverseLerp(
                        moveEvent.startTime,
                        moveEvent.endTime,
                        nowTime
                    )
                );

                float posY = Mathf.Lerp(
                    moveEvent.start2,
                    moveEvent.end2,
                    Mathf.InverseLerp(
                        moveEvent.startTime,
                        moveEvent.endTime,
                        nowTime
                    )
                );

                transform.localPosition = new Vector3(posX, posY, 0.0f);
            }

            // �����ж�����ת
            if (judgeLineRotateEvents != null)
            {
                JudgeLineEvent rotateEvent = judgeLineRotateEvents[nowRotateIndex];

                theta = Mathf.Lerp(
                    rotateEvent.start,
                    rotateEvent.end,
                    Mathf.InverseLerp(
                        rotateEvent.startTime,
                        rotateEvent.endTime,
                        nowTime
                    )
                );

                transform.localRotation = Quaternion.AngleAxis(theta, Vector3.forward);
            }

            // �����ж�����ת
            if (judgeLineRotateEvents != null)
            {
                JudgeLineEvent rotateEvent = judgeLineRotateEvents[nowRotateIndex];

                theta = Mathf.Lerp(
                    rotateEvent.start,
                    rotateEvent.end,
                    Mathf.InverseLerp(
                        rotateEvent.startTime,
                        rotateEvent.endTime,
                        nowTime
                    )
                );

                transform.localRotation = Quaternion.AngleAxis(theta, Vector3.forward);
            }
        }
    }

    public void CreateNote<T>(
        GameObject prefab, Transform parent, ChartNote chartNote, List<T> controls, Sprite HLSprite
    )
        where T : BaseNoteControl
    {
        if (
            controls == null ||
            !Instantiate(prefab, parent)
                .TryGetComponent<T>(out var control)
        ) return;

        control.transform.position = new Vector3(1000f, 0f, 0f); // ��ʼλ������Ļ��

        control.levelInformation = levelInformation;
        control.progressControl = progressControl;
        control.judgeLine = this;
        control.noteInfor = chartNote;

        if (chordSupport)
        {
            int noteIndex = levelInformation.chartNoteSortByTime.IndexOf(chartNote);
            if (noteIndex >= 0)
            {
                // ���ǰһ�������Ƿ�ͬʱ����
                if (noteIndex > 0)
                {
                    ChartNote prevNote = levelInformation.chartNoteSortByTime[noteIndex - 1];
                    if (Mathf.Abs(prevNote.realTime - chartNote.realTime) <= 0.001f)
                    {
                        control.SetSprite(HLSprite);
                    }
                }

                // ����һ�������Ƿ�ͬʱ����
                if (noteIndex < levelInformation.chartNoteSortByTime.Count - 1)
                {
                    ChartNote nextNote = levelInformation.chartNoteSortByTime[noteIndex + 1];
                    if (Mathf.Abs(nextNote.realTime - chartNote.realTime) <= 0.001f)
                    {
                        control.SetSprite(HLSprite);
                    }
                }
            }
        }

        control.noteScale = this.noteScale;
        control.SetScale();

        controls.Add(control);
    }

    public void CreateNote(int thisIndex, bool ifAbove)
    {
        List<ChartNote> targetNotes = ifAbove ? notesAbove : notesBelow;
        if (targetNotes == null || thisIndex < 0 || thisIndex >= targetNotes.Count)
            throw new ArgumentOutOfRangeException();

        ChartNote chartNote = targetNotes[thisIndex];

        switch (chartNote.type)
        {
            case 1: // Click
                CreateNote(Click, transform, chartNote, gameUpdateManager.clickControls, ClickHL);
                break;
            case 2: // Drag
                CreateNote(Drag, transform, chartNote, gameUpdateManager.dragControls, DragHL);
                break;

            case 3: // Hold (����ʵ��)
                    // ����Ҫ����ʱ��ת��Hold����
                    // 1. ʵ����HoldControl����
                var holdObj = Instantiate(Hold, transform);
                holdObj.transform.position = new Vector3(1000f, 0f, 0f); // ��ʼλ������Ļ��

                // 2. ��ȡHoldControl�������ʼ����������
                var holdControl = holdObj.GetComponent<HoldControl>();
                if (holdControl == null) return;

                holdControl.levelInformation = this.levelInformation;
                holdControl.progressControl = this.progressControl;
                holdControl.noteInfor = chartNote;  // ��������������
                holdControl.judgeLine = this; // ��������

                // 3. ���Ҽ�⣨ͬʱ���µ�������
                if (this.chordSupport)
                {
                    int currentIndex = levelInformation.chartNoteSortByTime.IndexOf(chartNote);

                    // ���ǰһ�������Ƿ񹹳ɺ���
                    if (currentIndex > 0)
                    {
                        ChartNote prevNote = levelInformation.chartNoteSortByTime[currentIndex - 1];
                        if (Mathf.Abs(prevNote.realTime - chartNote.realTime) <= 0.001f)
                        {
                            holdControl.SetSprite(this.HoldHL0, this.HoldHL1);
                        }
                    }
                    // ����һ�������Ƿ񹹳ɺ���
                    else if (currentIndex < levelInformation.chartNoteSortByTime.Count - 1)
                    {
                        ChartNote nextNote = levelInformation.chartNoteSortByTime[currentIndex + 1];
                        if (Mathf.Abs(nextNote.realTime - chartNote.realTime) <= 0.001f)
                        {
                            holdControl.SetSprite(this.HoldHL0, this.HoldHL1);
                        }
                    }
                }

                // 4. ��ӵ����¹�����
                gameUpdateManager.holdControls.Add(holdControl);

                // 5. ������Ļ��������������С
                float screenAspect = (float)Screen.width / Screen.height;
                float scaleFactor = this.noteScale;

                if (screenAspect < 1.7778f)
                {
                    scaleFactor *= screenAspect / 1.7778f;
                }

                holdControl.noteScale = scaleFactor;
                holdControl.SetScale();
                return;

            case 4: // Flick
                CreateNote(Flick, transform, chartNote, gameUpdateManager.flickControls, FlickHL);
                break;
        }
    }
}
