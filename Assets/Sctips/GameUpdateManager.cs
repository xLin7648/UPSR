using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameUpdateManager : MonoBehaviour
{
    // Fields
    public LevelInformation levelInformation; // 0x18
    public JudgeControl judgeControl; // 0x20
    public ProgressControl progressControl; // 0x28

    public List<ClickControl> clickControls; // 0x30
    public List<DragControl> dragControls; // 0x38
    public List<HoldControl> holdControls; // 0x40
    public List<FlickControl> flickControls; // 0x48

    public List<ClickControl> clickControlsBK; // 0x30
    public List<DragControl> dragControlsBK; // 0x38
    public List<HoldControl> holdControlsBK; // 0x40
    public List<FlickControl> flickControlsBK; // 0x48
    private int i; // 0x50

    public void BackupControls()
    {
        clickControlsBK = clickControls.ToList();
        dragControlsBK = dragControls.ToList();
        holdControlsBK = holdControls.ToList();
        flickControlsBK = flickControls.ToList();
    }

    public void ResetControls()
    {
        clickControls = clickControlsBK.ToList();
        dragControls = dragControlsBK.ToList();
        holdControls = holdControlsBK.ToList();
        flickControls = flickControlsBK.ToList();
    }

    public void ResetNotesScore()
    {
        ResetNoteScore(clickControls);
        ResetNoteScore(dragControls);
        ResetNoteScore(holdControls);
        ResetNoteScore(flickControls);

        void ResetNoteScore<T>(List<T> baseNoteControls)
            where T : BaseNoteControl
        {
            foreach (var control in baseNoteControls)
            {
                control.NoteReset();
            }
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

            // 位置计算和可见性逻辑
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
                        // 情况1：音符时间已过，始终可见
                        control.isVisible = true;
                    }
                    else
                    {
                        // 计算位置偏移阈值（动态调整）
                        float threshold = Mathf.Max(tempoNoteInfo.floorPosition / 6000000f, 0.001f);

                        if (positionDiff >= -threshold)
                        {
                            // 计算速度调整后的位置偏移
                            float speedAdjustedOffset = positionDiff * tempoNoteInfo.speed * LevelInformation.speed;

                            // 情况2：位置偏移在可接受范围内
                            control.isVisible = speedAdjustedOffset <= 20f;
                        }
                        else
                        {
                            control.isVisible = false;
                        }
                        // 情况3：位置偏移超出阈值，不可见
                    }
                }
                else
                {
                    // 开始时间小于当前时间
                    if (tempoNoteInfo.realTime < progressControl.nowTime)
                    {
                        // 开始时间小于当前时间并且结束时间大于当前时间
                        control.isVisible =
                                tempoNoteInfo.realTime + tempoNoteInfo.holdTime > progressControl.nowTime;
                    }
                    else
                    {
                        // 计算位置偏移阈值（动态调整）
                        float threshold = Mathf.Max(tempoNoteInfo.floorPosition / 6000000f, 0.001f);

                        if (positionDiff >= -threshold)
                        {
                            // 计算速度调整后的位置偏移
                            float speedAdjustedOffset = positionDiff * LevelInformation.speed;

                            // 情况2：位置偏移在可接受范围内
                            control.isVisible = speedAdjustedOffset <= 20f;
                        }
                        else
                        {
                            control.isVisible = false;
                        }
                    }
                }


                // 如果可见则更新位置
                if (control.isVisible)
                {
                    if (tempoNoteInfo.type == 3)
                    {
                        (control as HoldControl).timeOfJudge += Time.deltaTime;
                    }

                    control.NoteMove();
                }
                else
                {
                    // 隐藏时移动到屏幕外
                    control.transform.localPosition = new Vector3(0, 0, -50);
                }

                if (!progressControl.isPlay)
                {
                    i++;
                    continue;
                }

                var diff = Mathf.Abs(tempoNoteInfo.realTime - progressControl.nowTime);

                if (diff <= 5)
                {
                    if (control.Judge())
                    {
                        /*Destroy(control.gameObject);
                        baseNoteControls.RemoveAt(i);
                        i--;*/

                        if (progressControl.isPlay)
                        {
                            control.gameObject.SetActive(false);
                            baseNoteControls.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            i++;
        }
    }

    private void Update()
    {
        if (levelInformation == null || !levelInformation.chartLoaded)
            return;

        // 1. 更新JudgeLineControls
        i = 0;
        if (judgeControl != null)
        {
            while (i < judgeControl.judgeLineControls.Count)
            {
                JudgeLineControl lineControl = judgeControl.judgeLineControls[i];
                if (lineControl != null)
                {
                    lineControl.UpdateInfo(); // 调用JudgeLineControl的方法
                    i++;
                }
                else
                {
                    break;
                }
            }
        }

        // 2. 处理ClickControls
        UpdateNoteControls(clickControls);
        UpdateNoteControls(dragControls);
        UpdateNoteControls(holdControls);
        UpdateNoteControls(flickControls);
    }
}
