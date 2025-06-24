using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeControl : MonoBehaviour
{
    public List<GameObject> judgeLines; // 0x18
    public List<JudgeLineControl> judgeLineControls; // 0x20
    public FingerManagement fingerManagement; // 0x28
    public ProgressControl progressControl; // 0x30
    public float nowTime; // 0x38
    public float pauseTime; // 0x3C
    public GameObject pauseRing; // 0x40
    public GameObject backButton; // 0x48
    public CircleCollider2D backButtonCollider; // 0x50
    public GameObject retryButton; // 0x58
    public CircleCollider2D retryButtonCollider; // 0x60
    public List<ChartNote> chartNoteSortByTime; // 0x68
    private int _startIndex; // 0x70
    private int _endIndex; // 0x74
    public static bool inChallengeMode; // 0x0
    public static float perfectTimeRange = 0.08f; // 0x4
    public static float goodTimeRange = 0.18f; // 0x8
    public static float badTimeRange = 0.22f; // 0xC
    private float _minDeltaTime; // 0x78
    private int _code; // 0x7C
    private float _badTime; // 0x80
    private float _touchPos; // 0x84

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ��ȡ��ǰ��Ϸʱ��
        float currentTime = progressControl.nowTime;
        if (currentTime < 0) return;

        nowTime = currentTime;
        GetFingerPosition(); // ������ָλ��

        // ����������ָ����
        if (fingerManagement.fingers != null)
        {
            for (int fingerIndex = 0; fingerIndex < fingerManagement.fingers.Count; fingerIndex++)
            {
                Fingers finger = fingerManagement.fingers[fingerIndex];

                // ������״̬����ָ
                if (finger.phase == TouchPhase.Began)
                {
                    CheckNote(fingerIndex);   // ����������
                    // CheckPause(fingerIndex);  // �����ͣ����
                }

                // ����µ���ɨ(flick)����
                /*if (finger.isNewFlick)
                {
                    CheckFlick(fingerIndex);
                }*/
            }
        }
    }

    public void GetFingerPosition()
    {
        // ���������ж���
        for (int lineIndex = 0; lineIndex < judgeLines.Count; lineIndex++)
        {
            // ��ȡ��ǰ�ж��߿������
            JudgeLineControl lineControl = judgeLineControls[lineIndex];
            lineControl.numOfFingers = fingerManagement.fingers.Count;

            if (lineControl.fingerPositionX.Length < lineControl.numOfFingers || 
                lineControl.fingerPositionY.Length < lineControl.numOfFingers)
            {
                lineControl.fingerPositionX = new float[lineControl.numOfFingers];
                lineControl.fingerPositionY = new float[lineControl.numOfFingers];
            }

            // ����������ָ
            for (int fingerIndex = 0; fingerIndex < fingerManagement.fingers.Count; fingerIndex++)
            {
                // ��ȡ��ָλ��
                Vector2 fingerPos = fingerManagement.fingers[fingerIndex].nowPosition;

                // ��ȡ�ж���λ�úͽǶ�
                Vector3 linePos = judgeLines[lineIndex].transform.position;
                float theta = lineControl.theta;

                // ����Ƕ����ֵ
                float rad = theta * Mathf.Deg2Rad;
                float sinTheta = Mathf.Sin(rad);
                float cosTheta = Mathf.Cos(rad);

                // ������ָ���ж����ϵ�ͶӰλ��
                lineControl.fingerPositionX[fingerIndex] =
                    (linePos.y - fingerPos.y) * sinTheta -
                    (linePos.x - fingerPos.x) * cosTheta;

                lineControl.fingerPositionY[fingerIndex] =
                    (linePos.x - fingerPos.x) * sinTheta -
                    (linePos.y - fingerPos.y) * cosTheta;
            }
        }
    }

    private void CheckNote(int fingerIndex)
    {
        // ��ʼ����������
        _endIndex = -1;
        if (chartNoteSortByTime == null) return;

        // ���ҵ�һ������ʱ����ֵ������
        int tempEndIndex = -1;
        while (tempEndIndex < chartNoteSortByTime.Count - 1)
        {
            int nextIndex = tempEndIndex + 1;
            if (nextIndex >= chartNoteSortByTime.Count) break;

            ChartNote nextNote = chartNoteSortByTime[nextIndex];
            float timeThreshold = nowTime + badTimeRange;

            if (nextNote.realTime >= timeThreshold) break;

            tempEndIndex = nextIndex;
        }
        _endIndex = tempEndIndex;

        // ������ʼ����
        for (_startIndex = _endIndex; _startIndex >= 1; _startIndex--)
        {
            int prevIndex = _startIndex - 1;
            if (prevIndex >= chartNoteSortByTime.Count) break;

            ChartNote prevNote = chartNoteSortByTime[prevIndex];
            float timeThreshold = nowTime - goodTimeRange;

            if (prevNote.realTime <= timeThreshold) break;
        }

        // ���������Χ��Ч��
        if (_startIndex < 0 || _startIndex > _endIndex) return;

        // ��ʼ����Сʱ���͵�ǰѡ������
        _minDeltaTime = float.MaxValue;
        _code = -1;
        float v23 = -0.5f; // ԭʼ�����еĳ���

        // �������������ж�
        for (int index = _startIndex; index <= _endIndex; index++)
        {
            if (index >= chartNoteSortByTime.Count) break;

            ChartNote currentNote = chartNoteSortByTime[index];
            if (currentNote == null) continue;

            // ��ȡ�ж��߿������
            int lineIndex = currentNote.judgeLineIndex;

            if (judgeLineControls == null || lineIndex >= judgeLineControls.Count) continue;

            JudgeLineControl lineControl = judgeLineControls[lineIndex];
            if (lineControl == null || lineControl.fingerPositionX == null ||
                fingerIndex >= lineControl.fingerPositionX.Length) continue;

            // ����λ�ò� // Touch��������
            _touchPos = Mathf.Abs(currentNote.positionX - lineControl.fingerPositionX[fingerIndex]);

            // �������ж�����
            if (currentNote.isJudged) continue;

            // ���ʱ����λ������
            float timeDelta = currentNote.realTime - nowTime;
            if (timeDelta < _minDeltaTime + 0.01f && _touchPos < 1.9f)
            {
                // �����ж�ʱ����ֵ
                _badTime = (_touchPos <= 0.9f) ?
                    perfectTimeRange :
                    perfectTimeRange + (_touchPos - 0.9f) * goodTimeRange * v23;

                // ����Ƿ�����Ч�ж�ʱ����
                if (timeDelta <= _badTime)
                {
                    // ����������������
                    if (_code >= 0 && _code < chartNoteSortByTime.Count)
                    {
                        ChartNote codeNote = chartNoteSortByTime[_code];
                        if (codeNote != null && (codeNote.type == 2 || codeNote.type == 4))
                        {
                            continue;
                        }
                    }
                    // �������ƥ������
                    float absTimeDelta = Mathf.Abs(timeDelta);
                    if (absTimeDelta < _minDeltaTime)
                    {
                        _minDeltaTime = absTimeDelta;
                        _code = index;
                    }
                }
                else
                {
                    // bad�߼���ʧ
                }
                
            }
        }

        // ��������ѡ��������
        if (_code >= 0 && _code < chartNoteSortByTime.Count)
        {
            ChartNote selectedNote = chartNoteSortByTime[_code];
            if (selectedNote != null && selectedNote.type != 4)
            {
                // ��ȡ��Ӧ���ж���
                int lineIndex = selectedNote.judgeLineIndex;

                if (judgeLineControls == null || lineIndex >= judgeLineControls.Count)
                    return;

                var lineControl = judgeLineControls[lineIndex];
                if (lineControl == null) return;

                // ����λ��ȷ��ʹ���Ϸ�/�·������б�
                var targetList = selectedNote.isAbove ?
                    lineControl.notesAbove :
                    lineControl.notesBelow;

                // �������Ϊ���ж�
                if (targetList != null && selectedNote.noteIndex < targetList.Count)
                {
                    ChartNote noteToJudge = targetList[selectedNote.noteIndex];
                    if (noteToJudge != null)
                    {
                        noteToJudge.isJudged = true;
                    }
                }
            }
        }
    }
}
