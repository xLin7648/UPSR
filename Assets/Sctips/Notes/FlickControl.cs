// Namespace: 
using System.Collections.Generic;
using UnityEngine;

public class FlickControl : BaseNoteControl // TypeDefIndex: 7910
{
    public override bool Judge()
    {
        if (noteInfor == null || progressControl == null || judgeLine == null)
        {
            Debug.LogError("Null reference in FlickControl.Judge");
            return false;
        }

        float realTime = noteInfor.realTime;
        float nowTime = progressControl.nowTime;
        float deltaTime = realTime - nowTime;

        List<ChartNote> targetList = noteInfor.isAbove ? judgeLine.notesAbove : judgeLine.notesBelow;
        if (targetList != null && noteInfor.noteIndex < targetList.Count)
        {
            ChartNote targetNote = targetList[(int)noteInfor.noteIndex];
            if (targetNote != null && targetNote.isJudgedForFlick)
            {
                isJudged = true;
            }
        }

        // �ж��ɹ��߼�
        if (deltaTime < 0.005f && isJudged)
        {
            // ������Ч
            HitSongManager.instance.Play(2);

            // ��������λ��
            Transform transform = this.transform;
            Vector3 localPosition = new Vector3(noteInfor.positionX, 0, 0);
            transform.localPosition = localPosition;

            // �����ж�ʱ���
            float judgeDelta = -deltaTime;

            // ��ȡ����λ��
            Vector3 worldPosition = transform.position;

            // ����Perfect�ӷ�
            // scoreControl.Perfect(noteInfor.noteCode, judgeDelta, worldPosition, 0);

            HitEffectManager.instance.Play(true, noteScale, transform);

            // ��Ǹ�����Ϊ���ж�
            MarkNoteAsJudged(noteInfor.judgeLineIndex, noteInfor.noteIndex);

            return true;
        }

        // �����ж��߼�
        float perfectRange = JudgeControl.perfectTimeRange;
        if (deltaTime < -1.75f * perfectRange)
        {
            // Miss����
            // scoreControl.Miss(noteInfor.noteCode);

            // ��Ǹ�����Ϊ���ж�
            MarkNoteAsJudged(noteInfor.judgeLineIndex, noteInfor.noteIndex);

            // ��Ǹ�������Flick�ж�Ϊ���ж�
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
