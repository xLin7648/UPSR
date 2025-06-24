// Namespace: 
using UnityEngine;

public class HoldControl : BaseNoteControl // TypeDefIndex: 7910
{
	public float[] FingerPositionX; // 0x50
	public GameObject holdHead; // 0x60
	public GameObject holdEnd; // 0x68
	public GameObject perfect; // 0x70
	public GameObject good; // 0x78
	[SerializeField] private SpriteRenderer holdHeadSpriteRenderer; // 0x90
	[SerializeField] private SpriteRenderer holdSpriteRenderer; // 0x98
	[SerializeField] private SpriteRenderer holdEndSpriteRenderer1; // 0xA0

	public float timeOfJudge; // 0x80
	private bool missed; // 0x85
	private bool judged; // 0x86
	private bool judgeOver; // 0x87
	private bool isPerfect; // 0x88
	private bool _holdHeadIsDestroyed; // 0xA8
	private int _safeFrame; // 0xAC
	private float _judgeTime; // 0xB0

    public override void Start()
    {
		holdHeadSpriteRenderer.sortingLayerName = "HoldNote";
        holdSpriteRenderer.sortingLayerName = "HoldNote";
        holdEndSpriteRenderer1.sortingLayerName = "HoldNote";

        // 1. 获取当前对象的Transform组件
        var trans = transform;

		// 3. 计算旋转角度（核心逻辑）
		int angle = 180 * (noteInfor.isAbove ? 0 : 1);

		// 4. 创建绕Z轴旋转的四元数
		Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

		// 5. 应用旋转到当前对象
		trans.localRotation = rotation;
	}

    public void SetSprite(Sprite holdHeadSprite, Sprite holdSprite)
    {
		holdHeadSpriteRenderer.sprite = holdHeadSprite;
		holdSpriteRenderer.sprite = holdSprite;
	}

    public override void SetScale()
    {
        base.SetScale();
        SetHoldScale(noteInfor.holdTime);
    }

    public override void NoteReset()
    {
        SetScale();
        NoteMove();

        holdHeadSpriteRenderer.color = Color.white;
        holdSpriteRenderer.color = Color.white;
        holdEndSpriteRenderer1.color = Color.white;

        holdHead.transform.localPosition = Vector3.zero;
        _holdHeadIsDestroyed = false;

        transform.localPosition = new Vector3(noteInfor.positionX, 0, 0);
        gameObject.SetActive(true);
    }

    public override void NoteMove()
    {
        float currentTime = progressControl.nowTime;
        float noteTime = noteInfor.realTime;
        
        float holdEndTime = noteTime + noteInfor.holdTime;

        if (noteTime >= currentTime)
        {
            // 音符尚未到达判定时间
            int lineIndex = noteInfor.judgeLineIndex;
            float floorPos = levelInformation.floorPositions[lineIndex];
            float positionDiff = noteInfor.floorPosition - floorPos;

            if (positionDiff > -0.001f)
            {
                if (noteInfor.isAbove)
                {
                    transform.localPosition = new Vector3(
                        noteInfor.positionX,
                        positionDiff * LevelInformation.speed,
                        0
                    );
                }
                else
                {
                    transform.localPosition = new Vector3(
                        noteInfor.positionX,
                        -positionDiff * LevelInformation.speed,
                        0
                    );
                }
            }
        }
        else if (!_holdHeadIsDestroyed)
        {
            var oldPos = transform.localPosition;
            oldPos.y = 0;
            transform.localPosition = oldPos;

            // 隐藏音符头部
            _holdHeadIsDestroyed = true;
            holdHead.transform.localPosition = Vector3.forward * -100000f;
        }

        // 3. 缩放调整逻辑
        if (holdEndTime <= currentTime)
        {
            // 长按音符已结束
            transform.localPosition = new Vector3(0, 0, -50);
        }
        else
        {
            if (noteTime >= currentTime)
            {
                SetHoldScale(noteInfor.holdTime);
            }
            else
            {
                SetHoldScale(holdEndTime - currentTime);
            }
        }

        // 4. 判定效果生成
        if (
            timeOfJudge >= (0.5f * (60f / GetBPM()) - 0.00001f) &&
            judged && !missed)
        {
            /*GameObject effectPrefab = isPerfect ? perfectEffect : goodEffect;
            GameObject effect = Instantiate(effectPrefab);

            // 设置效果位置和缩放
            effect.transform.position = transform.position + Vector3.back;
            effect.transform.localScale = Vector3.one * (scale * 1.35f);*/

            timeOfJudge = 0;
        }

        // 5. 错过状态处理
        if (missed)
        {
            var fadedColor = new Color(1, 1, 1, 0.45f);
            holdSpriteRenderer.color = fadedColor;
            holdEndSpriteRenderer1.color = fadedColor;
        }
    }

    float GetBPM()
    {
        var judgeLines = levelInformation.judgeLineList;
        return judgeLines[noteInfor.judgeLineIndex].bpm;
    }

    public void SetHoldScale(float holdLength)
    {
        float heightScale = (noteInfor.speed * (holdLength / 3.8f) * 0.2f) * LevelInformation.speed;

        Vector3 newScale = new Vector3(
            noteScale * 0.22f,
            heightScale,
            1.0f
        );
        transform.localScale = newScale;

        // 调整头部和尾部大小
        Vector3 headTailScale = new Vector3(
            1.0f,
            0.22f / newScale.y,
            1.0f
        );
        holdHead.transform.localScale = headTailScale;
        holdEnd.transform.localScale = headTailScale;
    }

    public override bool Judge()
    {
        return false;
    }
}
