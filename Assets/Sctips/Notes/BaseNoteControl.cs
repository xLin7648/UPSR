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
        Vector3 scale = noteTransform.localScale;
        float aspectRatio = (float)Screen.width / Screen.height;

        float finalScale = aspectRatio >= 1.7778f
            ? noteScale
            : noteScale * (aspectRatio / 1.7778f);

        noteTransform.localScale = scale * finalScale;
    }

    public virtual void NoteReset()
    {
        SetScale();
        NoteMove();

        spriteRenderer.color = Color.white;
        transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);
        gameObject.SetActive(true);
    }

    public virtual void NoteMove()
    {
        ChartNote noteInfor = this.noteInfor;
        if (noteInfor == null) return;

        int judgeLineIndex = judgeLine.index;

        // �Ϸ��������λ�ü���
        if (noteInfor.isAbove)
        {
            Transform transform = base.transform;
            float positionX = noteInfor.positionX;
            float[] floorPositions = levelInformation.floorPositions;

            if (judgeLineIndex < 0 || judgeLineIndex >= floorPositions.Length)
                throw new IndexOutOfRangeException();

            float targetFloor = floorPositions[judgeLineIndex];
            float noteSpeed = noteInfor.speed;
            float currentFloor = noteInfor.floorPosition;
            float globalSpeed = LevelInformation.speed;

            // ����Y��λ��
            float positionY = (currentFloor - targetFloor) * noteSpeed * globalSpeed;
            transform.localPosition = new Vector3(positionX, positionY, 0);
        }
        // �·��������λ�ü���
        else
        {
            Transform transform = base.transform;
            float positionX = noteInfor.positionX;
            float[] floorPositions = levelInformation.floorPositions;

            if (judgeLineIndex < 0 || judgeLineIndex >= floorPositions.Length)
                throw new IndexOutOfRangeException();

            float targetFloor = floorPositions[judgeLineIndex];
            float noteSpeed = noteInfor.speed;
            float currentFloor = noteInfor.floorPosition;
            float globalSpeed = LevelInformation.speed;

            // ����Y��λ��
            float positionY = -(currentFloor - targetFloor) * noteSpeed * globalSpeed;
            transform.localPosition = new Vector3(positionX, positionY, 0);
        }

        // ������ʧʱ��͸���Ƚ���
        if (noteInfor.realTime < progressControl.nowTime)
        {
            float timeDiff = progressControl.nowTime - noteInfor.realTime;
            float alpha = Mathf.Clamp01(1.0f - (timeDiff / 0.18f));

            spriteRenderer.color = new Color(1, 1, 1, alpha);
        }
        
    }
    public abstract bool Judge();
}