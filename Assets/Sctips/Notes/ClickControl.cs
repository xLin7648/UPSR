using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickControl : BaseNoteControl
{
    public override bool Judge()
    {
        // ��ȡ������Ϣ
        ChartNote noteInfor = this.noteInfor;

        // �����ж�������
        int judgeLineIndex = noteInfor.judgeLineIndex;

        // ��������Ƿ��ѱ��ж�
        if (noteInfor.isJudged) this.isJudged = true;

        // ����ʱ���
        float timeDiff = noteInfor.realTime - this.progressControl.nowTime;

        if (GameUpdateManager.instance.AUTO_PLAY)
        {
            // PERFECT �ж�
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

        // ���ж������Ĵ���
        if (this.isJudged)
        {
            float absTime = Mathf.Abs(timeDiff);

            // PERFECT �ж�
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
            // GOOD �ж�
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
            // BAD �ж�
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
        // δ�ж������Ĵ���
        else
        {
            if (timeDiff >= -JudgeControl.goodTimeRange)
            {
                return false;
            }

            // MISS �ж�
            /*if (this.scoreControl == null) throw new NullReferenceException();

            this.scoreControl.Miss(this.noteInfor.noteCode);*/

            // ���������ж�״̬
            noteInfor.isJudged = true;

            return true;
        }
    }
}