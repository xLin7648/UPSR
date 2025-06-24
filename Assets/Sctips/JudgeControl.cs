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
                /*if (finger.isNewFlick)
                {
                    CheckFlick(fingerIndex);
                }*/
            }
        }
    }

    public void GetFingerPosition()
    {
        // 遍历所有判定线
        for (int lineIndex = 0; lineIndex < judgeLines.Count; lineIndex++)
        {
            // 获取当前判定线控制组件
            JudgeLineControl lineControl = judgeLineControls[lineIndex];
            lineControl.numOfFingers = fingerManagement.fingers.Count;

            if (lineControl.fingerPositionX.Length < lineControl.numOfFingers || 
                lineControl.fingerPositionY.Length < lineControl.numOfFingers)
            {
                lineControl.fingerPositionX = new float[lineControl.numOfFingers];
                lineControl.fingerPositionY = new float[lineControl.numOfFingers];
            }

            // 遍历所有手指
            for (int fingerIndex = 0; fingerIndex < fingerManagement.fingers.Count; fingerIndex++)
            {
                // 获取手指位置
                Vector2 fingerPos = fingerManagement.fingers[fingerIndex].nowPosition;

                // 获取判定线位置和角度
                Vector3 linePos = judgeLines[lineIndex].transform.position;
                float theta = lineControl.theta;

                // 计算角度相关值
                float rad = theta * Mathf.Deg2Rad;
                float sinTheta = Mathf.Sin(rad);
                float cosTheta = Mathf.Cos(rad);

                // 计算手指在判定线上的投影位置
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
        float v23 = -0.5f; // 原始代码中的常量

        // 遍历音符进行判定
        for (int index = _startIndex; index <= _endIndex; index++)
        {
            if (index >= chartNoteSortByTime.Count) break;

            ChartNote currentNote = chartNoteSortByTime[index];
            if (currentNote == null) continue;

            // 获取判定线控制组件
            int lineIndex = currentNote.judgeLineIndex;

            if (judgeLineControls == null || lineIndex >= judgeLineControls.Count) continue;

            JudgeLineControl lineControl = judgeLineControls[lineIndex];
            if (lineControl == null || lineControl.fingerPositionX == null ||
                fingerIndex >= lineControl.fingerPositionX.Length) continue;

            // 计算位置差 // Touch计算有误
            _touchPos = Mathf.Abs(currentNote.positionX - lineControl.fingerPositionX[fingerIndex]);

            // 跳过已判定音符
            if (currentNote.isJudged) continue;

            // 检查时间差和位置条件
            float timeDelta = currentNote.realTime - nowTime;
            if (timeDelta < _minDeltaTime + 0.01f && _touchPos < 1.9f)
            {
                // 计算判定时间阈值
                _badTime = (_touchPos <= 0.9f) ?
                    perfectTimeRange :
                    perfectTimeRange + (_touchPos - 0.9f) * goodTimeRange * v23;

                // 检查是否在有效判定时间内
                if (timeDelta <= _badTime)
                {
                    // 处理特殊音符类型
                    if (_code >= 0 && _code < chartNoteSortByTime.Count)
                    {
                        ChartNote codeNote = chartNoteSortByTime[_code];
                        if (codeNote != null && (codeNote.type == 2 || codeNote.type == 4))
                        {
                            continue;
                        }
                    }
                    // 更新最佳匹配音符
                    float absTimeDelta = Mathf.Abs(timeDelta);
                    if (absTimeDelta < _minDeltaTime)
                    {
                        _minDeltaTime = absTimeDelta;
                        _code = index;
                    }
                }
                else
                {
                    // bad逻辑丢失
                }
                
            }
        }

        // 处理最终选定的音符
        if (_code >= 0 && _code < chartNoteSortByTime.Count)
        {
            ChartNote selectedNote = chartNoteSortByTime[_code];
            if (selectedNote != null && selectedNote.type != 4)
            {
                // 获取对应的判定线
                int lineIndex = selectedNote.judgeLineIndex;

                if (judgeLineControls == null || lineIndex >= judgeLineControls.Count)
                    return;

                var lineControl = judgeLineControls[lineIndex];
                if (lineControl == null) return;

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
    }
}
