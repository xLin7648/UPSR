// Namespace: 
using System.Collections.Generic;
using UnityEngine;

public class FlickControl : BaseNoteControl // TypeDefIndex: 7910
{
    public override bool Judge()
    {
        float timeDiff = noteInfor.realTime - progressControl.nowTime;

        if (GameUpdateManager.instance.AUTO_PLAY)
        {
            // PERFECT 判定
            if (timeDiff <= 0)
            {
                isJudged = true;
                HitSongManager.instance.Play(2);
                Transform transform = this.transform;

                transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);

                HitEffectManager.instance.Play(true, noteScale, transform);

                ScoreManager.instance.Hit();
                return true;
            }
            return false;
        }

        List<ChartNote> targetList = noteInfor.isAbove ? judgeLine.notesAbove : judgeLine.notesBelow;
        if (targetList != null && noteInfor.noteIndex < targetList.Count)
        {
            ChartNote targetNote = targetList[(int)noteInfor.noteIndex];
            if (targetNote != null && targetNote.isJudgedForFlick)
            {
                isJudged = true;
            }
        }

        // 判定成功逻辑
        if (timeDiff < 0.005f && isJudged)
        {
            // 播放音效
            HitSongManager.instance.Play(2);

            // 设置音符位置
            Transform transform = this.transform;
            Vector3 localPosition = new Vector3(noteInfor.positionX, 0, 0);
            transform.localPosition = localPosition;

            // 计算判定时间差
            float judgeDelta = -timeDiff;

            // 获取音符位置
            Vector3 worldPosition = transform.position;

            // 调用Perfect加分
            // scoreControl.Perfect(noteInfor.noteCode, judgeDelta, worldPosition, 0);

            HitEffectManager.instance.Play(true, noteScale, transform);

            ScoreManager.instance.Hit();

            // 标记该音符为已判定
            MarkNoteAsJudged(noteInfor.judgeLineIndex, noteInfor.noteIndex);

            return true;
        }

        // 过早判定逻辑
        float perfectRange = JudgeControl.perfectTimeRange;
        if (timeDiff < -1.75f * perfectRange)
        {
            // Miss处理
            // scoreControl.Miss(noteInfor.noteCode);

            ScoreManager.instance.Miss();

            // 标记该音符为已判定
            MarkNoteAsJudged(noteInfor.judgeLineIndex, noteInfor.noteIndex);

            // 标记该音符的Flick判定为已判定
            if (targetList != null && noteInfor.noteIndex < targetList.Count)
            {
                ChartNote targetNote = targetList[noteInfor.noteIndex];
                if (targetNote != null)
                {
                    targetNote.isJudgedForFlick = true;
                }
            }

            return true;
        }

        return false;
    }

    private void MarkNoteAsJudged(int judgeLineIndex, long noteIndex)
    {
        List<ChartNote> targetList = noteInfor.isAbove ? judgeLine.notesAbove : judgeLine.notesBelow;
        if (targetList == null || noteIndex < 0 || noteIndex >= targetList.Count)
        {
            return;
        }

        ChartNote note = targetList[(int)noteIndex];
        if (note != null)
        {
            note.isJudged = true;
        }
    }
}
