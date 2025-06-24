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
        // 获取NoteUpdateManager组件
        if (levelInformation != null)
        {
            gameUpdateManager = levelInformation.GetComponent<GameUpdateManager>();
        }

        // 从LevelInformation获取对应判定线数据
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

        // 加载配置数据
        if (levelInformation != null)
        {
            noteScale = levelInformation.noteScale;
        }

        // 计算屏幕适配比例
        float screenRatio = (float)Screen.height / Screen.width;
        moveScale = 1.0f;

        if (screenRatio > 0.5625f) // 9:16宽高比
        {
            float targetHeight = Screen.width * 0.5625f;
            moveScale = targetHeight / Screen.height;
        }

        // 处理判定线消失事件
        if (judgeLineDisappearEvents != null && judgeLineDisappearEvents.Count > 0)
        {
            if (chartLoad != null && chartLoad.aPfCisOn)
            {
                // APFC模式下的颜色处理
                JudgeLineEvent disappearEvent = judgeLineDisappearEvents[0];
                Color color = new Color(1.0f, 1.0f, 0.635f, disappearEvent.start);
                _judgeLineSpriteRenderer.color = color;
            }
            else
            {
                // 普通模式下的颜色处理
                JudgeLineEvent disappearEvent = judgeLineDisappearEvents[0];
                Color color = isShowingPic
                    ? new Color(picColor.r, picColor.g, picColor.b, disappearEvent.start)
                    : new Color(1.0f, 1.0f, 1.0f, disappearEvent.start);
                _judgeLineSpriteRenderer.color = color;
            }
        }
        else
        {
            // 无消失事件时的处理
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

        // 重置位置和旋转
        Transform transform = this.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.forward);

        // 处理速度事件
        if (levelInformation != null)
        {
            // 创建新音符
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
        // 更新速度事件索引
        UpdateJudgeLineEventIndex(speedEvents, ref nowSpeedIndex);
        // 更新判定线消失事件索引
        UpdateJudgeLineEventIndex(judgeLineDisappearEvents, ref nowDisappearIndex);
        // 更新判定线移动事件索引
        UpdateJudgeLineEventIndex(judgeLineMoveEvents, ref nowMoveIndex);
        // 更新判定线旋转事件索引
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
            // 正向遍历：找到第一个超过目标时间的关键帧
            while (idx + 1 < eventCount)
            {
                T nextKeyframe = lineEvents[idx + 1];
                // 使用关键帧的起始时间比较
                if (nextKeyframe.startTime > nowTime)
                    break;

                idx++;
            }

            // 反向遍历：确保当前关键帧时间不超过目标时间
            while (idx > 0)
            {
                T currentKeyframe = lineEvents[idx];
                // 使用关键帧的起始时间比较
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
        // 检查判定线渲染器是否有效
        if (_judgeLineSpriteRenderer != null)
        {
            // 更新当前时间
            if (progressControl != null)
            {
                nowTime = progressControl.nowTime;
            }

            // 更新事件索引
            UpdateJudgeLineEventsIndex();

            // 处理游戏开始前的时间
            if (nowTime < 0.0f)
            {
                return;
            }

            // 处理速度事件
            if (levelInformation != null)
            {
                if (speedEvents != null && levelInformation.floorPositions != null)
                {
                    // 更新地板位置
                    SpeedEvent speedEvent = speedEvents[nowSpeedIndex];
                    float newPosition = speedEvent.floorPosition +
                                       (nowTime - speedEvent.startTime) * speedEvent.value;

                    if (index < levelInformation.floorPositions.Length)
                    {
                        levelInformation.floorPositions[index] = newPosition;
                    }
                }
            }

            // 处理游戏进行中的状态
            if (/*scoreControl != null && scoreControl.isAllPerfect && */levelInformation != null && levelInformation.aPfCisOn)
            {
                // APFC模式下的颜色插值
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
                // FC模式下的颜色插值
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
                // 普通模式下的颜色插值
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

            // 更新判定线位置
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

            // 更新判定线旋转
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

            // 更新判定线旋转
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

        control.transform.position = new Vector3(1000f, 0f, 0f); // 初始位置在屏幕外

        control.levelInformation = levelInformation;
        control.progressControl = progressControl;
        control.judgeLine = this;
        control.noteInfor = chartNote;

        if (chordSupport)
        {
            int noteIndex = levelInformation.chartNoteSortByTime.IndexOf(chartNote);
            if (noteIndex >= 0)
            {
                // 检查前一个音符是否同时出现
                if (noteIndex > 0)
                {
                    ChartNote prevNote = levelInformation.chartNoteSortByTime[noteIndex - 1];
                    if (Mathf.Abs(prevNote.realTime - chartNote.realTime) <= 0.001f)
                    {
                        control.SetSprite(HLSprite);
                    }
                }

                // 检查后一个音符是否同时出现
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

            case 3: // Hold (跳过实现)
                    // 根据要求暂时不转换Hold部分
                    // 1. 实例化HoldControl对象
                var holdObj = Instantiate(Hold, transform);
                holdObj.transform.position = new Vector3(1000f, 0f, 0f); // 初始位置在屏幕外

                // 2. 获取HoldControl组件并初始化核心引用
                var holdControl = holdObj.GetComponent<HoldControl>();
                if (holdControl == null) return;

                holdControl.levelInformation = this.levelInformation;
                holdControl.progressControl = this.progressControl;
                holdControl.noteInfor = chartNote;  // 关联的音符数据
                holdControl.judgeLine = this; // 父控制器

                // 3. 和弦检测（同时按下的音符）
                if (this.chordSupport)
                {
                    int currentIndex = levelInformation.chartNoteSortByTime.IndexOf(chartNote);

                    // 检查前一个音符是否构成和弦
                    if (currentIndex > 0)
                    {
                        ChartNote prevNote = levelInformation.chartNoteSortByTime[currentIndex - 1];
                        if (Mathf.Abs(prevNote.realTime - chartNote.realTime) <= 0.001f)
                        {
                            holdControl.SetSprite(this.HoldHL0, this.HoldHL1);
                        }
                    }
                    // 检查后一个音符是否构成和弦
                    else if (currentIndex < levelInformation.chartNoteSortByTime.Count - 1)
                    {
                        ChartNote nextNote = levelInformation.chartNoteSortByTime[currentIndex + 1];
                        if (Mathf.Abs(nextNote.realTime - chartNote.realTime) <= 0.001f)
                        {
                            holdControl.SetSprite(this.HoldHL0, this.HoldHL1);
                        }
                    }
                }

                // 4. 添加到更新管理器
                gameUpdateManager.holdControls.Add(holdControl);

                // 5. 根据屏幕比例调整音符大小
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
