using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickControl : BaseNoteControl
{
    public override bool Judge()
    {
        // 获取音符信息
        ChartNote noteInfor = this.noteInfor;

        // 计算判定线索引
        int judgeLineIndex = noteInfor.judgeLineIndex;

        // 检查音符是否已被判定
        if (noteInfor.isJudged) this.isJudged = true;

        // 计算时间差
        float timeDiff = noteInfor.realTime - this.progressControl.nowTime;

        if (GameUpdateManager.instance.AUTO_PLAY)
        {
            // PERFECT 判定
            if (timeDiff <= 0)
            {
                isJudged = true;
                HitSongManager.instance.Play(0);
                Transform transform = this.transform;

                transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);

                HitEffectManager.instance.Play(true, noteScale, transform);
                return true;
            }
            return false;
        }

        // 已判定音符的处理
        if (this.isJudged)
        {
            float absTime = Mathf.Abs(timeDiff);

            // PERFECT 判定
            if (absTime < JudgeControl.perfectTimeRange)
            {
                HitSongManager.instance.Play(0);
                Transform transform = this.transform;

                transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);

                HitEffectManager.instance.Play(true, noteScale, transform);

                /*if (this.scoreControl == null) throw new NullReferenceException();

                Vector3 position = transform.position;
                this.scoreControl.Perfect(
                    this.noteInfor.noteCode,
                    -timeDiff,
                    position,
                    0
                );*/
                return true;
            }
            // GOOD 判定
            else if (absTime < JudgeControl.goodTimeRange)
            {
                HitSongManager.instance.Play(0);
                Transform transform = this.transform;

                transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);

                HitEffectManager.instance.Play(false, noteScale, transform);
                /*if (this.scoreControl == null) throw new NullReferenceException();

                Vector3 position = transform.position;
                this.scoreControl.Good(
                    this.noteInfor.noteCode,
                    -timeDiff,
                    position,
                    0
                );*/
                return true;
            }
            // BAD 判定
            else
            {
                GameObject badInstance = GameObject.Instantiate(this.noteBad);

                Transform badTransform = badInstance.transform;
                Transform thisTransform = this.transform;

                badTransform.parent = thisTransform.parent;
                badTransform.localScale = thisTransform.localScale;

                Vector3 spawnPosition = thisTransform.position + new Vector3(0, 0, 1);
                badTransform.SetPositionAndRotation(
                    spawnPosition,
                    thisTransform.rotation
                );
                /*
                this.scoreControl.Bad(this.noteInfor.noteCode, -timeDiff);

                if (this.noteBad == null) throw new NullReferenceException();
                GameObject badInstance = GameObject.Instantiate(this.noteBad);

                Transform badTransform = badInstance.transform;
                Transform thisTransform = this.transform;

                badTransform.parent = thisTransform.parent;
                badTransform.localScale = thisTransform.localScale;

                Vector3 spawnPosition = thisTransform.position + new Vector3(0, 0, 1);
                badTransform.SetPositionAndRotation(
                    spawnPosition,
                    thisTransform.rotation
                );*/
                return true;
            }
        }
        // 未判定音符的处理
        else
        {
            if (timeDiff >= -JudgeControl.goodTimeRange)
            {
                return false;
            }

            // MISS 判定
            /*if (this.scoreControl == null) throw new NullReferenceException();

            this.scoreControl.Miss(this.noteInfor.noteCode);*/

            // 更新音符判定状态
            noteInfor.isJudged = true;

            return true;
        }
    }
}