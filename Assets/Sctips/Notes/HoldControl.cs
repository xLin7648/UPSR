// Namespace: 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HoldControl : BaseNoteControl // TypeDefIndex: 7910
{
	public List<float> FingerPositionX; // 0x50
	public GameObject holdHead; // 0x60
	public GameObject holdEnd; // 0x68
	public GameObject perfect; // 0x70
	public GameObject good; // 0x78
	[SerializeField] private SpriteRenderer holdHeadSpriteRenderer; // 0x90
	[SerializeField] private SpriteRenderer holdSpriteRenderer; // 0x98
	[SerializeField] private SpriteRenderer holdEndSpriteRenderer1; // 0xA0

	public float timeOfJudge; // 0x80
	private bool missed; // 0x85
	private bool judged; // 0x86
	private bool judgeOver; // 0x87
	private bool isPerfect; // 0x88
	private bool _holdHeadIsDestroyed; // 0xA8
	private int _safeFrame; // 0xAC
	private float _judgeTime; // 0xB0

    public override void Start()
    {
		holdHeadSpriteRenderer.sortingLayerName = "HoldNote";
        holdSpriteRenderer.sortingLayerName = "HoldNote";
        holdEndSpriteRenderer1.sortingLayerName = "HoldNote";

        // 1. 获取当前对象的Transform组件
        var trans = transform;

		// 3. 计算旋转角度（核心逻辑）
		int angle = 180 * (noteInfor.isAbove ? 0 : 1);

		// 4. 创建绕Z轴旋转的四元数
		Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		// 5. 应用旋转到当前对象
		trans.localRotation = rotation;
	}

    public void SetSprite(Sprite holdHeadSprite, Sprite holdSprite)
    {
		holdHeadSpriteRenderer.sprite = holdHeadSprite;
		holdSpriteRenderer.sprite = holdSprite;
	}

    public override void SetScale()
    {
        base.SetScale();
        SetHoldScale(noteInfor.holdTime);
    }

    public override void NoteReset()
    {
        SetScale();
        NoteMove();

        holdHeadSpriteRenderer.color = Color.white;
        holdSpriteRenderer.color = Color.white;
        holdEndSpriteRenderer1.color = Color.white;

        holdHead.transform.localPosition = Vector3.zero;
        _holdHeadIsDestroyed = false;

        transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);
        gameObject.SetActive(true);
    }

    public override void NoteMove()
    {
        float currentTime = progressControl.nowTime;
        float noteTime = noteInfor.realTime;
        
        float holdEndTime = noteTime + noteInfor.holdTime;

        if (noteTime >= currentTime)
        {
            // 音符尚未到达判定时间
            int lineIndex = noteInfor.judgeLineIndex;
            float floorPos = levelInformation.floorPositions[lineIndex];
            float positionDiff = noteInfor.floorPosition - floorPos;

            if (positionDiff > -0.001f)
            {
                if (noteInfor.isAbove)
                {
                    transform.localPosition = new Vector3(
                        noteInfor.positionX,
                        positionDiff * LevelInformation.speed,
                        0
                    );
                }
                else
                {
                    transform.localPosition = new Vector3(
                        noteInfor.positionX,
                        -positionDiff * LevelInformation.speed,
                        0
                    );
                }
            }
        }
        else if (!_holdHeadIsDestroyed)
        {
            var oldPos = transform.localPosition;
            oldPos.y = 0;
            transform.localPosition = oldPos;

            // 隐藏音符头部
            _holdHeadIsDestroyed = true;
            holdHead.transform.localPosition = Vector3.forward * -100000f;
        }

        // 3. 缩放调整逻辑
        if (holdEndTime <= currentTime)
        {
            // 长按音符已结束
            transform.localPosition = new Vector3(0, 0, -50);
        }
        else
        {
            if (noteTime >= currentTime)
            {
                SetHoldScale(noteInfor.holdTime);
            }
            else
            {
                SetHoldScale(holdEndTime - currentTime);
            }
        }

        // 4. 判定效果生成
        if (
            timeOfJudge >= (0.5f * (60f / GetBPM()) - 0.00001f) &&
            judged && !missed)
        {
            HitEffectManager.instance.Play(isPerfect, noteScale, transform);

            timeOfJudge = 0;
        }

        // 5. 错过状态处理
        if (missed)
        {
            var fadedColor = new Color(1, 1, 1, 0.45f);
            holdSpriteRenderer.color = fadedColor;
            holdEndSpriteRenderer1.color = fadedColor;
            holdHeadSpriteRenderer.color = fadedColor;
        }
    }

    float GetBPM()
    {
        var judgeLines = levelInformation.judgeLineList;
        return judgeLines[noteInfor.judgeLineIndex].bpm;
    }

    public void SetHoldScale(float holdLength)
    {
        float heightScale = (noteInfor.speed * (holdLength / 3.8f) * 0.2f) * LevelInformation.speed;

        Vector3 newScale = new Vector3(
            noteScale * 0.22f,
            heightScale,
            1.0f
        );
        transform.localScale = newScale;

        // 调整头部和尾部大小
        Vector3 headTailScale = new Vector3(
            1.0f,
            0.22f / newScale.y,
            1.0f
        );
        holdHead.transform.localScale = headTailScale;
        holdEnd.transform.localScale = headTailScale;
    }

    public override bool Judge()
    {
        if (GameUpdateManager.instance.AUTO_PLAY)
        {
            float timeDiff = noteInfor.realTime - progressControl.nowTime;

            // PERFECT 判定
            if (timeDiff <= 0)
            {
                if (!isJudged)
                {
                    judged = true;
                    isJudged = true;
                    isPerfect = true;

                    HitSongManager.instance.Play(0);
                    Transform transform = this.transform;

                    transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);

                    HitEffectManager.instance.Play(true, noteScale, transform);

                    return false;
                }

                var het = noteInfor.realTime + noteInfor.holdTime;

                if (judged && !judgeOver && het < progressControl.nowTime)
                {
                    judgeOver = true;
                    return true;
                }
            }
            return false;
        }

        // 长按开始判定
        if (!judged && !missed)
        {
            // 获取目标音符列表
            List<ChartNote> targetList = noteInfor.isAbove ? judgeLine.notesAbove : judgeLine.notesBelow;
            if (targetList != null && noteInfor.noteIndex < targetList.Count)
            {
                ChartNote targetNote = targetList[noteInfor.noteIndex];
                if (targetNote != null && targetNote.isJudged)
                {
                    isJudged = true;
                }
            }

            // 时间差计算
            float timeDiff = noteInfor.realTime - progressControl.nowTime;
            float absDelta = Mathf.Abs(timeDiff);

            // 完美/良好判定
            if (isJudged)
            {
                float perfectRange = JudgeControl.perfectTimeRange;
                float goodRange = JudgeControl.goodTimeRange;

                if (absDelta < perfectRange)
                {
                    // 完美判定
                    judged = true;
                    HitSongManager.instance.Play(0);
                    isPerfect = true;

                    HitEffectManager.instance.Play(true, noteScale, transform);
                }
                else if (absDelta < goodRange)
                {
                    // 良好判定
                    judged = true;
                    HitSongManager.instance.Play(0);
                    isPerfect = false;

                    HitEffectManager.instance.Play(false, noteScale, transform);
                }
                else
                {
                    // 超出判定范围，重置状态
                    if (targetList != null && noteInfor.noteIndex < targetList.Count)
                    {
                        ChartNote targetNote = targetList[noteInfor.noteIndex];
                        if (targetNote != null) targetNote.isJudged = false;
                    }
                    missed = true;
                }
            }
            // 过早判定
            else if (timeDiff < -JudgeControl.goodTimeRange)
            {
                // 标记为已判定
                if (targetList != null && noteInfor.noteIndex < targetList.Count)
                {
                    ChartNote targetNote = targetList[noteInfor.noteIndex];
                    if (targetNote != null) targetNote.isJudged = true;
                }

                missed = true;
                /*if (scoreControl != null)
                {
                    scoreControl.Miss(noteInfor.noteCode);
                }*/
            }
        }

        // 长按持续判定
        if (judged && !missed && !judgeOver)
        {
            FingerPositionX = judgeLine.fingerPositionX;
            int numOfFingers = judgeLine.numOfFingers;
            bool fingerOnTrack = false;

            if (FingerPositionX != null)
            {
                for (int i = 0; i < numOfFingers && i < FingerPositionX.Count; i++)
                {
                    float distance = Mathf.Abs(FingerPositionX[i] - noteInfor.positionX);
                    if (distance < 1.9f)
                    {
                        fingerOnTrack = true;
                        _safeFrame = 2;
                        break;
                    }
                }
            }

            if (!fingerOnTrack)
            {
                if (_safeFrame > 0)
                {
                    _safeFrame--;
                    missed = false;
                }
                else
                {
                    judgeOver = true;
                    missed = true;
                    /*if (scoreControl != null)
                    {
                        scoreControl.Miss(noteInfor.noteCode);
                    }*/
                }
            }
        }

        // 长按结束判定
        float holdEndTime = noteInfor.realTime + noteInfor.holdTime;
        if (judged && !judgeOver && holdEndTime - 0.22f < progressControl.nowTime)
        {
            Vector3 position = transform.position;
            /*if (isPerfect)
            {
                scoreControl.Perfect(noteInfor.noteCode, -_judgeTime, position, 1);
            }
            else
            {
                scoreControl.Good(noteInfor.noteCode, -_judgeTime, position, 1);
            }*/
            judgeOver = true;
        }

        // 长按超时判定
        if (holdEndTime + 0.25f < progressControl.nowTime && !judged && !missed && !judgeOver)
        {
            /*if (scoreControl != null)
            {
                scoreControl.Miss(noteInfor.noteCode);
            }*/
            missed = true;
            return true;
        }

        return false;
    }
}
