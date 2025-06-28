using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseNoteControl : MonoBehaviour
{
    public ProgressControl progressControl; // 0x20
    public LevelInformation levelInformation; // 0x28
    public JudgeLineControl judgeLine; // 0x30
    public ChartNote noteInfor; // 0x38
    public bool isVisible; // 0x40
    public GameObject noteBad; // 0x50
    public float noteScale; // 0x58

    protected bool isJudged; // 0x58
    [SerializeField] private SpriteRenderer spriteRenderer; // 0x60

    public virtual void Start()
    {
        spriteRenderer.sortingLayerName = "BaseNote";
    }

    public void SetSprite(Sprite noteImage)
    {
        spriteRenderer.sprite = noteImage;
    }
    
    public virtual void SetScale()
    {
        var noteTransform = transform;
        Vector3 scale = new Vector3(0.22f, 0.22f, 0);
        float aspectRatio = (float)Screen.width / Screen.height;

        float finalScale = aspectRatio >= 1.7778f
            ? noteScale
            : noteScale * (aspectRatio / 1.7778f);

        noteTransform.localScale = scale * finalScale;
    }

    public virtual bool NoteReset()
    {
        SetScale();

        var result = noteInfor.time <= progressControl.nowTime;

        if (result)
        {
            isJudged = true;
        }
        else
        {
            isJudged = false;
        }

        NoteMove();

        return result;
    }

    public virtual void NoteMove()
    {
        Transform transform = base.transform;

        ChartNote noteInfor = this.noteInfor;
        int judgeLineIndex = judgeLine.index;
        float positionX = noteInfor.positionX;
        float targetFloor = levelInformation.floorPositions[judgeLineIndex];
        float noteSpeed = noteInfor.speed;
        float currentFloor = noteInfor.floorPosition;
        float globalSpeed = LevelInformation.speed;

        // 上方轨道音符位置计算
        if (noteInfor.isAbove)
        {
            // 计算Y轴位置
            float positionY = (currentFloor - targetFloor) * noteSpeed * globalSpeed;
            transform.localPosition = new Vector3(positionX, positionY, 0);
        }
        // 下方轨道音符位置计算
        else
        {
            // 计算Y轴位置
            float positionY = -(currentFloor - targetFloor) * noteSpeed * globalSpeed;
            transform.localPosition = new Vector3(positionX, positionY, 0);
        }

        // 音符消失时的透明度渐变
        if (noteInfor.realTime < progressControl.nowTime)
        {
            float timeDiff = progressControl.nowTime - noteInfor.realTime;
            float alpha = Mathf.Clamp01(1.0f - (timeDiff / 0.18f));

            spriteRenderer.color = new Color(1, 1, 1, alpha);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
        
    }
    public abstract bool Judge();
}