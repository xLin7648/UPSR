using System.Collections.Generic;
using UnityEngine;

public class DragControl : BaseNoteControl
{
    public List<float> FingerPositionX; // 0x50

    public override bool Judge()
    {
        if (noteInfor == null || progressControl == null)
            return false;

        float timeDiff = noteInfor.realTime - progressControl.nowTime;
        float absDelta = Mathf.Abs(timeDiff);

        if (GameUpdateManager.instance.AUTO_PLAY)
        {
            // PERFECT �ж�
            if (timeDiff <= 0)
            {
                isJudged = true;
                HitSongManager.instance.Play(1);
                Transform transform = this.transform;

                transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);

                HitEffectManager.instance.Play(true, noteScale, transform);

                ScoreManager.instance.Hit();
                return true;
            }
            return false;
        }

        // �����ָλ���Ƿ�����Ч��Χ��
        if (absDelta <= 0.1f && !isJudged && judgeLine != null)
        {
            FingerPositionX = judgeLine.fingerPositionX;
            int numOfFingers = judgeLine.numOfFingers;

            if (FingerPositionX != null)
            {
                for (int i = 0; i < numOfFingers; i++)
                {
                    if (i >= FingerPositionX.Count) break;

                    float fingerX = FingerPositionX[i];
                    float distance = Mathf.Abs(fingerX - noteInfor.positionX);

                    if (distance < 2.1f)
                    {
                        isJudged = true;
                        break;
                    }
                }
            }
        }

        // �����ж����
        if (timeDiff < 0.005f && isJudged)
        {
            // ������Ч
            HitSongManager.instance.Play(1);

            // ��������λ��
            Transform transform = this.transform;
            if (noteInfor != null)
            {
                Vector3 localPosition = new Vector3(noteInfor.positionX, 0, 0);
                transform.localPosition = localPosition;
            }

            HitEffectManager.instance.Play(true, noteScale, transform);

            ScoreManager.instance.Hit();

            // ��¼�����ж�
            /*if (scoreControl != null && noteInfor != null)
            {
                Vector3 worldPosition = transform.position;
                scoreControl.Perfect(noteInfor.noteCode, -deltaTime, worldPosition, 0);
            }*/

            return true;
        }
        else if (timeDiff < -0.1f && !isJudged)
        {
            // ��¼ʧ��
            /*if (scoreControl != null && noteInfor != null)
            {
                scoreControl.Miss(noteInfor.noteCode);
            }*/

            ScoreManager.instance.Miss();

            return true;
        }

        return false;
    }
}