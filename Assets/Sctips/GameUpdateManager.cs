using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameUpdateManager : MonoBehaviour
{
    public bool AUTO_PLAY;

    public static GameUpdateManager instance;

    // Fields
    public LevelInformation levelInformation; // 0x18
    public JudgeControl judgeControl; // 0x20
    public FingerManagement fingerManagement; // 0x20
    public ProgressControl progressControl; // 0x28

    public List<BaseNoteControl> clickControls; // 0x30
    public List<BaseNoteControl> dragControls; // 0x38
    public List<BaseNoteControl> holdControls; // 0x40
    public List<BaseNoteControl> flickControls; // 0x48

    public List<BaseNoteControl> clickControlsBK; // 0x30
    public List<BaseNoteControl> dragControlsBK; // 0x38
    public List<BaseNoteControl> holdControlsBK; // 0x40
    public List<BaseNoteControl> flickControlsBK; // 0x48
    private int i; // 0x50

    private void Awake()
    {
        instance = this;
    }

    public void RBK(bool isStart = false)
    {
        clickControls = clickControlsBK.ToList();
        dragControls = dragControlsBK.ToList();
        holdControls = holdControlsBK.ToList();
        flickControls = flickControlsBK.ToList();

        if (isStart) return;

        PauseEndUpdate(clickControls);
        PauseEndUpdate(dragControls);
        PauseEndUpdate(holdControls);
        PauseEndUpdate(flickControls);
    }

    private void RunningUpdate()
    {
        UpdateNoteControls(clickControls);
        UpdateNoteControls(dragControls);
        UpdateNoteControls(holdControls);
        UpdateNoteControls(flickControls);
    }

    public void PauseUpdate()
    {
        UpdateNoteControls(clickControlsBK);
        UpdateNoteControls(dragControlsBK);
        UpdateNoteControls(holdControlsBK);
        UpdateNoteControls(flickControlsBK);
    }

    public void PauseEndUpdate<T>(List<T> baseNoteControls)
        where T : BaseNoteControl
    {
        i = 0;
        while (i < baseNoteControls.Count)
        {
            var control = baseNoteControls[i];
            if (control.NoteReset())
            {
                control.gameObject.SetActive(false);
                baseNoteControls.RemoveAt(i);
                i--;
            }

            i++;
        }
    }

    public void UpdateNoteControls<T>(List<T> baseNoteControls)
        where T : BaseNoteControl
    {
        i = 0;
        if (baseNoteControls == null) return;

        while (i < baseNoteControls.Count)
        {
            var control = baseNoteControls[i];
            var tempoNoteInfo = control.noteInfor;

            // λ�ü���Ϳɼ����߼�
            float floorPosition = tempoNoteInfo.floorPosition;
            int judgeLineIndex = tempoNoteInfo.judgeLineIndex;

            if (judgeLineIndex < levelInformation.floorPositions.Length)
            {
                float targetPos = levelInformation.floorPositions[judgeLineIndex];
                float positionDiff = floorPosition - targetPos;

                if (tempoNoteInfo.type != 3)
                {
                    if (tempoNoteInfo.realTime < progressControl.nowTime)
                    {
                        // ���1������ʱ���ѹ���ʼ�տɼ�
                        control.isVisible = true;
                    }
                    else
                    {
                        // ����λ��ƫ����ֵ����̬������
                        float threshold = Mathf.Max(tempoNoteInfo.floorPosition / 6000000f, 0.001f);

                        if (positionDiff >= -threshold)
                        {
                            // �����ٶȵ������λ��ƫ��
                            float speedAdjustedOffset = positionDiff * tempoNoteInfo.speed * LevelInformation.speed;

                            // ���2��λ��ƫ���ڿɽ��ܷ�Χ��
                            control.isVisible = speedAdjustedOffset <= 20f;
                        }
                        else
                        {
                            control.isVisible = false;
                        }
                        // ���3��λ��ƫ�Ƴ�����ֵ�����ɼ�
                    }
                }
                else
                {
                    // ��ʼʱ��С�ڵ�ǰʱ��
                    if (tempoNoteInfo.realTime < progressControl.nowTime)
                    {
                        // ��ʼʱ��С�ڵ�ǰʱ�䲢�ҽ���ʱ����ڵ�ǰʱ��
                        // hold��

                        // ������������hold��󲻸�����ָʣһ�ڲ�����
                        control.isVisible =
                                tempoNoteInfo.realTime + tempoNoteInfo.holdTime + 2 > progressControl.nowTime;
                    }
                    else
                    {
                        // holdǰ

                        // ����λ��ƫ����ֵ����̬������
                        float threshold = Mathf.Max(tempoNoteInfo.floorPosition / 6000000f, 0.001f);

                        if (positionDiff >= -threshold)
                        {
                            // �����ٶȵ������λ��ƫ��
                            float speedAdjustedOffset = positionDiff * LevelInformation.speed;

                            // ���2��λ��ƫ���ڿɽ��ܷ�Χ��
                            control.isVisible = speedAdjustedOffset <= 20f;
                        }
                        else
                        {
                            control.isVisible = false;
                        }
                    }
                }

                // ����ɼ������λ��
                if (control.isVisible)
                {
                    if (progressControl.status == TimerState.Running)
                    {
                        if (tempoNoteInfo.type == 3)
                        {
                            (control as HoldControl).timeOfJudge += Time.deltaTime;
                        }

                        control.NoteMove();

                        if (tempoNoteInfo.realTime < progressControl.nowTime + 2f)
                        {
                            if (control.Judge())
                            {
                                control.gameObject.SetActive(false);
                                baseNoteControls.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        control.NoteReset();
                    }
                }
                else
                {
                    // ����ʱ�ƶ�����Ļ��
                    control.transform.localPosition = new Vector3(0, 0, -50);
                }
            }

            i++;
        }
    }

    private void Update()
    {
        if (progressControl.status == TimerState.Stop)
            return;

        // 1. ����JudgeLineControls
        foreach (var lineControl in judgeControl.judgeLineControls)
        {
            lineControl.UpdateInfo(); // ����JudgeLineControl�ķ���
        }

        i = 0;

        if (progressControl.status == TimerState.Pause)
        {
            PauseUpdate();
        }
        else
        {
            RunningUpdate();
        }

        
    }
}
