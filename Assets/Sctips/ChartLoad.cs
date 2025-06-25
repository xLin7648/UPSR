using LeTai.Asset.TranslucentImage;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Chart
{
    public int formatVersion; // 0x10
    public float offset; // 0x14
    public int numOfNotes; // 0x18
    public List<JudgeLine> judgeLineList; // 0x20
}

[Serializable]
public class JudgeLine // TypeDefIndex: 7904
{
    // Fields
    public int numOfNotes; // 0x10
    public int numOfNotesAbove; // 0x14
    public int numOfNotesBelow; // 0x18
    public float bpm; // 0x1C
    public List<SpeedEvent> speedEvents; // 0x20
    public List<ChartNote> notesAbove; // 0x28
    public List<ChartNote> notesBelow; // 0x30
    public List<JudgeLineEvent> judgeLineDisappearEvents; // 0x38
    public List<JudgeLineEvent> judgeLineMoveEvents; // 0x40
    public List<JudgeLineEvent> judgeLineRotateEvents; // 0x48
}

[Serializable]
public class ChartNote // TypeDefIndex: 7901
{
    // Fields
    public int type; // 0x10
    public int time; // 0x14
    public float positionX; // 0x18
    public float holdTime; // 0x1C
    public float speed; // 0x20
    public float floorPosition; // 0x24
    public bool isAbove; // 0x28
    public bool isJudged; // 0x28
    public bool isJudgedForFlick; // 0x29
    public float realTime; // 0x2C
    public int judgeLineIndex; // 0x30
    public int noteIndex; // 0x34
    public float noteCode; // 0x38
}

[Serializable]
public class SpeedEvent : JudgeLineEventBase // TypeDefIndex: 7902
{
    // Fields
    public float floorPosition; // 0x18
    public float value; // 0x1C
}

// Namespace: 
[Serializable]
public class JudgeLineEvent : JudgeLineEventBase // TypeDefIndex: 7903
{
    // Fields
    public float start; // 0x18
    public float end; // 0x1C
    public float start2; // 0x20
    public float end2; // 0x24
}

public class JudgeLineEventBase
{
    public float startTime; // 0x10
    public float endTime; // 0x14
}

public class ChartLoad : MonoBehaviour
{
    public Transform lineParent;
    public GameObject linePrefabe;

    public JudgeControl judgeControl;
    public ProgressControl progressControl;
    public LevelInformation levelInformation;

    public TextAsset chartAsset;

    [Header("Res")]
    public GameObject Click; // 0x18
    public GameObject Drag; // 0x20
    public GameObject Hold; // 0x28
    public GameObject Flick; // 0x30
    public Sprite ClickHL; // 0x38
    public Sprite HoldHL0; // 0x40
    public Sprite HoldHL1; // 0x48
    public Sprite DragHL; // 0x50
    public Sprite FlickHL; // 0x58

    [Space(20)]

    [Header("Settings")]
    public float offset;
    public float noteScale;
    public float scale;
    public float musicVol;
    public float speed;
    public bool hitFxIsOn;
    public bool aPfCisOn;
    public Sprite[] judgeLineImages;

    public float screenW, screenH;

    public Chart chart;


    // Start is called before the first frame update
    void Start()
    {
        // ������������
        chart = JsonUtility.FromJson<Chart>(chartAsset.text);

        // ������Ļ�ߴ�
        screenH = Screen.height;
        screenW = Screen.width;

        // ���ùؿ���Ϣ
        levelInformation.offset = offset + chart.offset;
        levelInformation.noteScale = noteScale;

        // ���ݿ�߱ȵ�������
        float aspectRatio = screenW / screenH;
        if (aspectRatio < 1.7778f)
            noteScale *= (aspectRatio / 1.7778f);

        levelInformation.scale = scale;
        levelInformation.noteScale = noteScale;
        levelInformation.musicVol = musicVol;
        levelInformation.hitFxIsOn = hitFxIsOn;
        levelInformation.aPfCisOn = aPfCisOn;

        LevelInformation.speed = speed;
        levelInformation.numOfNotes = chart.numOfNotes;
        levelInformation.judgeLineList = chart.judgeLineList;

        progressControl.offset = levelInformation.offset;

        // ��������
        SortForNoteWithFloorPosition();
        SetCodeForNote();
        SetInformation();
        SortForAllNoteWithTime();

        // �����ж�����
        judgeControl.chartNoteSortByTime = levelInformation.chartNoteSortByTime;

        levelInformation.floorPositions = new float[levelInformation.judgeLineList.Count];

        // �����ж���
        for (int i = 0; i < levelInformation.judgeLineList.Count; i++)
        {
            GameObject judgeLineObj = Instantiate(linePrefabe);
            judgeLineObj.transform.SetParent(lineParent);

            JudgeLineControl judgeLineControl = judgeLineObj.GetComponent<JudgeLineControl>();
            judgeLineControl.index = i;
            judgeLineControl.levelInformation = levelInformation;
            judgeLineControl.progressControl = progressControl;

            // �����ж�����Դ
            judgeLineControl.Click = Click;
            judgeLineControl.Drag = Drag;
            judgeLineControl.Hold = Hold;
            judgeLineControl.Flick = Flick;
            judgeLineControl.ClickHL = ClickHL;
            judgeLineControl.DragHL = DragHL;
            judgeLineControl.HoldHL0 = HoldHL0;
            judgeLineControl.HoldHL1 = HoldHL1;
            judgeLineControl.FlickHL = FlickHL;

            // ��ӵ��б�
            levelInformation.judgeLines.Add(judgeLineObj);
            judgeControl.judgeLines.Add(judgeLineObj);
            judgeControl.judgeLineControls.Add(judgeLineControl);

            // Ӧ���ж���ͼ��
            if (judgeLineImages != null && i < judgeLineImages.Length)
            {
                SpriteRenderer renderer = judgeLineObj.GetComponentInChildren<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = judgeLineImages[i];
                    renderer.sortingLayerName = "UI";
                    renderer.sortingOrder = 0;

                    // ������ɫ
                    float colorValue = (0.75f * 135f) / 255f;
                    renderer.color = new Color(colorValue, colorValue, colorValue, 1f);

                    // ����λ�ú�����
                    Transform rendererTransform = renderer.transform;
                    rendererTransform.localScale = new Vector3(0.46296f, 0.46296f, 0.46296f);
                    rendererTransform.localPosition = Vector3.forward * 0.001f;
                }
            }
        }

        // �������������
        levelInformation.chartLoaded = true;
    }

    private void SortForNoteWithFloorPosition()
    {
        foreach (JudgeLine judgeLine in chart.judgeLineList)
        {
            // ���ϲ������� floorPosition ����
            if (judgeLine.notesAbove != null)
            {
                judgeLine.notesAbove.Sort((a, b) =>
                    a.floorPosition.CompareTo(b.floorPosition) // ��������
                );
            }

            // ���²������� floorPosition ����
            if (judgeLine.notesBelow != null)
            {
                judgeLine.notesBelow.Sort((a, b) =>
                    a.floorPosition.CompareTo(b.floorPosition) // ��������
                );
            }
        }
    }

    public void SetCodeForNote()
    {
        int baseCodeAbove = 0;          // �ϲ�������������
        int baseCodeBelow = 100000;     // �²�������������

        for (int lineIndex = 0; lineIndex < chart.judgeLineList.Count; lineIndex++)
        {
            JudgeLine judgeLine = chart.judgeLineList[lineIndex];

            // �����ϲ�����
            int currentCodeAbove = baseCodeAbove;
            if (judgeLine.notesAbove != null)
            {
                for (int noteIndex = 0; noteIndex < judgeLine.notesAbove.Count; noteIndex++)
                {
                    judgeLine.notesAbove[noteIndex].noteCode = currentCodeAbove;
                    currentCodeAbove += 10;  // ÿ����������10
                }
            }

            // �����²�����
            int currentCodeBelow = baseCodeBelow;
            if (judgeLine.notesBelow != null)
            {
                for (int noteIndex = 0; noteIndex < judgeLine.notesBelow.Count; noteIndex++)
                {
                    judgeLine.notesBelow[noteIndex].noteCode = currentCodeBelow;
                    currentCodeBelow += 10;  // ÿ����������10
                }
            }

            // ���������ж��ߵĻ�������
            baseCodeAbove += 1000000;
            baseCodeBelow += 1000000;
        }
    }

    private void SortForAllNoteWithTime()
    {
        // ʹ��Ԥ�����ʱ��Ƚ�����������
        levelInformation.chartNoteSortByTime.Sort((x, y) =>
            x.realTime.CompareTo(y.realTime) // ��������
        );
    }

    private float GetRealTime(int time, float bpm)
    {
        return (float)((float)time * 1.875) / bpm;
    }

    public void SetInformation()
    {
        if (chart.formatVersion != 3) throw new Exception("33333333333");

        const float v6 = -0.5f; // λ��ƫ�Ƴ���
        const float v7 = 10.0f;  // ��������

        
        for (int judgeLineIndex = 0; judgeLineIndex < chart.judgeLineList.Count; judgeLineIndex++)
        {
            JudgeLine judgeLine = chart.judgeLineList[judgeLineIndex];

            // Note����
            {
                // �����Ϸ�����
                for (int noteIndex = 0; noteIndex < judgeLine.notesAbove.Count; noteIndex++)
                {
                    ChartNote note = judgeLine.notesAbove[noteIndex];
                    ProcessNotePosition(note);
                    note.realTime = note.time * 1.875f / judgeLine.bpm;
                    note.holdTime = note.holdTime * 1.875f / judgeLine.bpm;
                    note.judgeLineIndex = judgeLineIndex;
                    note.noteIndex = noteIndex;
                    note.isJudged = false;
                    note.isAbove = true;

                    levelInformation.chartNoteSortByTime.Add(note);
                }

                // �����·�����
                for (int noteIndex = 0; noteIndex < judgeLine.notesBelow.Count; noteIndex++)
                {
                    ChartNote note = judgeLine.notesBelow[noteIndex];
                    ProcessNotePosition(note);
                    note.realTime = note.time * 1.875f / judgeLine.bpm;
                    note.holdTime = note.holdTime * 1.875f / judgeLine.bpm;
                    note.judgeLineIndex = judgeLineIndex;
                    note.noteIndex = noteIndex;
                    note.isJudged = false;
                    note.isAbove = false;

                    levelInformation.chartNoteSortByTime.Add(note);
                }
            }

            // �����ƶ��¼�
            for (int eventIndex = 0; eventIndex < judgeLine.judgeLineMoveEvents.Count; eventIndex++)
            {
                JudgeLineEvent moveEvent = judgeLine.judgeLineMoveEvents[eventIndex];

                // ת����ʼʱ��
                moveEvent.startTime = moveEvent.startTime * 1.875f / judgeLine.bpm;

                // ת������ʱ��
                moveEvent.endTime = moveEvent.endTime * 1.875f / judgeLine.bpm;

                float aspectRatio = screenW / screenH;

                if (aspectRatio <= 1.7778f) // խ������
                {
                    moveEvent.start = screenW * ((moveEvent.start + v6) * v7) / screenH;
                    moveEvent.start2 = (moveEvent.start2 + v6) * v7;
                    moveEvent.end = screenW * ((moveEvent.end + v6) * v7) / screenH;
                    moveEvent.end2 = (moveEvent.end2 + v6) * v7;
                }
                else // ��������
                {
                    moveEvent.start = ((moveEvent.start + v6) * v7) / 9.0f * 16.0f;
                    moveEvent.start2 = (moveEvent.start2 + v6) * v7;
                    moveEvent.end = ((moveEvent.end + v6) * v7) / 9.0f * 16.0f;
                    moveEvent.end2 = (moveEvent.end2 + v6) * v7;
                }
            }

            // ������ת�¼�
            for (int eventIndex = 0; eventIndex < judgeLine.judgeLineRotateEvents.Count; eventIndex++)
            {
                JudgeLineEvent rotateEvent = judgeLine.judgeLineRotateEvents[eventIndex];

                // ת����ʼʱ��
                rotateEvent.startTime = rotateEvent.startTime * 1.875f / judgeLine.bpm;

                // ת������ʱ��
                rotateEvent.endTime = rotateEvent.endTime * 1.875f / judgeLine.bpm;
            }

            // ����͸�����¼�
            for (int eventIndex = 0; eventIndex < judgeLine.judgeLineDisappearEvents.Count; eventIndex++)
            {
                JudgeLineEvent rotateEvent = judgeLine.judgeLineDisappearEvents[eventIndex];

                // ת����ʼʱ��
                rotateEvent.startTime = rotateEvent.startTime * 1.875f / judgeLine.bpm;

                // ת������ʱ��
                rotateEvent.endTime = rotateEvent.endTime * 1.875f / judgeLine.bpm;
            }

            // ת���ٶ��¼�ʱ��

            var speedEvents = judgeLine.speedEvents;
            float bpm = judgeLine.bpm;

            // �����һ���¼�
            var firstEvent = speedEvents[0];
            firstEvent.floorPosition = (firstEvent.startTime * 1.875f) / bpm;

            // ��������¼�
            for (int eventIdx = 1; eventIdx < speedEvents.Count; eventIdx++)
            {
                var prevEvent = speedEvents[eventIdx - 1];
                var currentEvent = speedEvents[eventIdx];

                // ���㵱ǰλ�ã����Ĺ�ʽ��
                currentEvent.floorPosition = prevEvent.floorPosition +
                    ((prevEvent.endTime - prevEvent.startTime) * 1.875f / bpm) * prevEvent.value;

                // ת��ǰһ���¼���ʱ��
                prevEvent.startTime = AdjustTime(prevEvent.startTime, bpm);
                prevEvent.endTime = AdjustTime(prevEvent.endTime, bpm);
            }

            // �������һ���¼���ʱ��ת��
            var lastEvent = speedEvents[^1];
            lastEvent.startTime = AdjustTime(lastEvent.startTime, bpm);
            lastEvent.endTime = AdjustTime(lastEvent.endTime, bpm);
        }

        float AdjustTime(float time, float bpm)
        {
            // ������������ֵ
            if (time == float.PositiveInfinity)
                return float.NegativeInfinity;

            return (time * 1.875f) / bpm;
        }
    }

    private void ProcessNotePosition(ChartNote note)
    {
        float aspectRatio = screenW / screenH;
        if (aspectRatio < 1.7778f)
        {
            note.positionX = (aspectRatio / 1.7778f) * note.positionX;
        }
    }
}