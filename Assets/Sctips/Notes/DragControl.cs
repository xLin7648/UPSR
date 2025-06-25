using System.Collections.Generic;
using UnityEngine;

public class DragControl : BaseNoteControl
{
    public List<float> FingerPositionX; // 0x50

    public override bool Judge()
    {
        if (noteInfor == null || progressControl == null)
            return false;

        float deltaTime = noteInfor.realTime - progressControl.nowTime;
        float absDelta = Mathf.Abs(deltaTime);

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
        if (deltaTime < 0.005f && isJudged)
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

            // ��¼�����ж�
            /*if (scoreControl != null && noteInfor != null)
            {
                Vector3 worldPosition = transform.position;
                scoreControl.Perfect(noteInfor.noteCode, -deltaTime, worldPosition, 0);
            }*/

            return true;
        }
        else if (deltaTime < -0.1f && !isJudged)
        {
            // ��¼ʧ��
            /*if (scoreControl != null && noteInfor != null)
            {
                scoreControl.Miss(noteInfor.noteCode);
            }*/

            return true;
        }

        return false;
    }
}