using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if (progressControl.isPaused) return;

        // 获取当前游戏时间
        float currentTime = progressControl.nowTime;
        if (currentTime < 0) return;

        nowTime = currentTime;
        GetFingerPosition(); // 更新手指位置

        // 遍历所有手指输入
        if (fingerManagement.fingers != null)
        {
            for (int fingerIndex = 0; fingerIndex < fingerManagement.fingers.Count; fingerIndex++)
            {
                Fingers finger = fingerManagement.fingers[fingerIndex];

                // 检查空闲状态的手指
                if (finger.phase == TouchPhase.Began)
                {
                    CheckNote(fingerIndex);   // 检测音符点击
                    // CheckPause(fingerIndex);  // 检测暂停操作
                }

                // 检测新的轻扫(flick)手势
                if (finger.isNewFlick)
                {
                    CheckFlick(fingerIndex);
                }
            }
        }
    }

    public void GetFingerPosition()
    {
        if (judgeLines == null) return;

        for (int i = 0; i < judgeLines.Count; i++)
        {
            if (judgeLineControls == null || i >= judgeLineControls.Count) continue;
            JudgeLineControl lineControl = judgeLineControls[i];
            List<Fingers> fingers = fingerManagement.fingers;
            lineControl.numOfFingers = fingers.Count;

            Vector3 linePos = judgeLines[i].transform.position;

            lineControl.fingerPositionX ??= new List<float>();
            lineControl.fingerPositionX.Clear();

            lineControl.fingerPositionY ??= new List<float>();
            lineControl.fingerPositionY.Clear();

            for (int j = 0; j < fingers.Count; j++)
            {
                Fingers finger = fingers[j];
                Vector2 fingerPos = finger.nowPosition;
                float thetaRad = -lineControl.theta * Mathf.Deg2Rad;
                float sinVal = Mathf.Sin(thetaRad);
                float cosVal = Mathf.Cos(thetaRad);
                float sinPos = Mathf.Sin(lineControl.theta * Mathf.Deg2Rad);

                float deltaX = linePos.x - fingerPos.x;
                float deltaY = linePos.y - fingerPos.y;

                lineControl.fingerPositionX.Add(deltaY * sinVal - deltaX * cosVal);
                lineControl.fingerPositionY.Add(deltaX * sinPos - deltaY * cosVal);
            }
        }
    }

    private void CheckNote(int fingerIndex)
    {
        // 初始化结束索引
        _endIndex = -1;
        if (chartNoteSortByTime == null) return;

        // 查找第一个超过时间阈值的音符
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

        // 查找起始索引
        for (_startIndex = _endIndex; _startIndex >= 1; _startIndex--)
        {
            int prevIndex = _startIndex - 1;
            if (prevIndex >= chartNoteSortByTime.Count) break;

            ChartNote prevNote = chartNoteSortByTime[prevIndex];
            float timeThreshold = nowTime - goodTimeRange;

            if (prevNote.realTime <= timeThreshold) break;
        }

        // 检查索引范围有效性
        if (_startIndex < 0 || _startIndex > _endIndex) return;

        // 初始化最小时间差和当前选中音符
        _minDeltaTime = float.MaxValue;
        _code = -1;

        // 遍历音符进行判定
        for (int index = _startIndex; index <= _endIndex; index++)
        {
            ChartNote note = chartNoteSortByTime[index];
            JudgeLineControl judgeLine = judgeLineControls[note.judgeLineIndex];

            // 计算手指与音符的横向距离
            float fingerX = judgeLine.fingerPositionX[fingerIndex];
            _touchPos = Mathf.Abs(note.positionX - fingerX);

            // 核心判定逻辑
            if (!note.isJudged)
            {
                float timeDelta = note.realTime - nowTime;

                // 重点关注的判定条件
                if (timeDelta < _minDeltaTime + 0.01f && _touchPos < 1.9f)
                {
                    // 计算动态判定阈值
                    _badTime = (_touchPos <= 0.9f)
                        ? JudgeControl.badTimeRange
                        : JudgeControl.badTimeRange + (_touchPos - 0.9f) * JudgeControl.perfectTimeRange * -0.5f;

                    // 满足判定时间条件
                    if (timeDelta <= _badTime)
                    {
                        // 处理多音符重叠情况
                        if (note.type != 1 && note.type != 3 && _code >= 0)
                        {
                            ChartNote prevNote = chartNoteSortByTime[_code];

                            if (prevNote.type == 2 || prevNote.type == 4)
                            {
                                if (Mathf.Abs(note.realTime - prevNote.realTime) <= 0.01f)
                                {
                                    // 计算当前音符综合距离
                                    float fingerY = judgeLine.fingerPositionY[fingerIndex];
                                    float currentDist = _touchPos + Mathf.Abs(fingerY / 2.2f);

                                    // 计算前一个音符综合距离
                                    JudgeLineControl prevLine = judgeLineControls[prevNote.judgeLineIndex];
                                    float prevX = prevLine.fingerPositionX[fingerIndex];
                                    float prevY = prevLine.fingerPositionY[fingerIndex];
                                    float prevDist = Mathf.Abs(prevNote.positionX - prevX) + Mathf.Abs(prevY / 2.2f);

                                    // 选择距离更近的音符
                                    if (currentDist < prevDist)
                                    {
                                        _minDeltaTime = Mathf.Abs(timeDelta);
                                        _code = index;
                                    }
                                }
                                else
                                {
                                    _minDeltaTime = Mathf.Abs(timeDelta);
                                    _code = index;
                                }
                            }
                        }
                        else
                        {
                            _minDeltaTime = Mathf.Abs(timeDelta);
                            _code = index;
                        }
                    }
                }
            }
        }

        // 处理最终选定的音符
        if (_code >= 0 && _code < chartNoteSortByTime.Count)
        {
            ChartNote selectedNote = chartNoteSortByTime[_code];
            var lineControl = judgeLineControls[selectedNote.judgeLineIndex];

            // 根据位置确定使用上方/下方音符列表
            var targetList = selectedNote.isAbove ?
                lineControl.notesAbove :
                lineControl.notesBelow;

            // 标记音符为已判定
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

    public void CheckFlick(int fingerIndex)
    {
        if (chartNoteSortByTime == null || judgeLineControls == null || fingerManagement == null)
            return;

        // 初始化索引
        _endIndex = -1;

        // 计算结束索引
        for (int i = 0; i < chartNoteSortByTime.Count; i++)
        {
            ChartNote note = chartNoteSortByTime[i];
            if (note.realTime >= nowTime + perfectTimeRange * 1.75f)
            {
                _endIndex = i - 1;
                break;
            }
        }
        if (_endIndex < 0) _endIndex = chartNoteSortByTime.Count - 1;

        // 计算开始索引
        _startIndex = _endIndex;
        for (int i = _endIndex; i >= 0; i--)
        {
            ChartNote note = chartNoteSortByTime[i];
            if (note.realTime <= nowTime - perfectTimeRange * 1.75f)
            {
                _startIndex = i + 1;
                break;
            }
        }

        // 重置选择状态
        _minDeltaTime = float.MaxValue;
        _code = -1;

        // 遍历可判定范围内的音符
        for (int index = _startIndex; index <= _endIndex; index++)
        {
            ChartNote note = chartNoteSortByTime[index];
            if (note.type != 4 || note.isJudgedForFlick) continue;

            // 检查时间差是否更优
            float deltaTime = Mathf.Abs(note.realTime - nowTime);
            if (deltaTime >= _minDeltaTime + 0.01f) continue;

            // 获取对应的判定线
            JudgeLineControl judgeLine = judgeLineControls[note.judgeLineIndex];

            // 检查手指位置
            float fingerX = judgeLine.fingerPositionX[fingerIndex];
            if (Mathf.Abs(note.positionX - fingerX) >= 2.1f) continue;

            // 更新最优选择
            _minDeltaTime = deltaTime;
            _code = index;
        }

        // 处理选中的音符
        if (_code < 0 || _code >= chartNoteSortByTime.Count) return;

        ChartNote selectedNote = chartNoteSortByTime[_code];
        int noteLineIndex = selectedNote.judgeLineIndex;

        JudgeLineControl targetLine = judgeLineControls[noteLineIndex];

        // 获取目标音符列表
        List<ChartNote> targetList = selectedNote.isAbove ?
            targetLine.notesAbove : targetLine.notesBelow;

        // 标记为已判定
        ChartNote targetNote = targetList[selectedNote.noteIndex];
        if (targetNote != null) targetNote.isJudgedForFlick = true;

        // 更新手指状态
        Fingers finger = fingerManagement.fingers[fingerIndex];
        if (finger != null) finger.isNewFlick = false;
    }

    private float GetFingerPositionForNote(ChartNote note, int fingerIndex)
    {
        JudgeLineControl lineControl = judgeLineControls[note.judgeLineIndex];
        return lineControl.fingerPositionX[fingerIndex];
    }
}
