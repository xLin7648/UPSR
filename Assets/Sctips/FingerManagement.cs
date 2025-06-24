using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class Fingers
{
    // Fields
    public int fingerId;
    public Vector2 lastMove;
    public Vector2 nowMove;
    public Vector2[] lastPositions;
    public Vector2 lastPosition;
    public Vector2 nowPosition;
    public bool isNewFlick;
    public bool stopped;
    public TouchPhase phase;

    // Constructor
    public Fingers()
    {
        fingerId = -100;
        lastMove = Vector2.zero;
        nowMove = Vector2.zero;

        // 创建历史位置数组（大小为3）
        lastPositions = new Vector2[3];
        for (int i = 0; i < 3; i++)
        {
            lastPositions[i] = Vector2.zero;
        }

        lastPosition = Vector2.zero;
        nowPosition = Vector2.zero;
        isNewFlick = true;
        stopped = false;
    }
}
public class FingerManagement : MonoBehaviour
{
    // Fields
    public List<Fingers> fingers = new();
    private float _flickJudgeSpeed = 0.06f;
    private float _flickSpeed;
    private Vector2 _tempoVector2;
    private Camera _camera;

    private void Start()
    {
        // 获取主相机并存储
        _camera = Camera.main;

        // 根据屏幕 DPI 调整轻扫判定速度
        float originalSpeed = _flickJudgeSpeed;
        _flickJudgeSpeed = (originalSpeed / 380f) * Screen.dpi;
    }

    private void Update()
    {
        SyncFingers();
        foreach (Fingers finger in fingers)
        {
            // 重置轻扫速度
            _flickSpeed = 0f;

            // 计算当前移动向量的模
            float moveMagnitude = finger.nowMove.magnitude;

            if (moveMagnitude > 0.1f)
            {
                // 计算点积并标准化
                float dotProduct = Vector2.Dot(finger.lastMove, finger.nowMove);
                _flickSpeed = dotProduct / moveMagnitude;
            }

            // 计算时间调整后的速度
            float deltaTime = Time.deltaTime;
            float adjustedSpeed = (_flickSpeed / 60f) / deltaTime;
            float judgeSpeedThreshold = _flickJudgeSpeed;

            // 判断是否需要更新轻扫状态
            if (adjustedSpeed < judgeSpeedThreshold || finger.stopped)
            {
                // 计算实际速度值
                float actualSpeed = (moveMagnitude / 60f) / deltaTime;
                float speedThreshold = _flickJudgeSpeed * 5f;

                // 更新手指状态
                finger.isNewFlick = actualSpeed >= speedThreshold;
                finger.stopped = actualSpeed < speedThreshold;
            }
        }
    }

    public void AddFinger(Touch touch)
    {
        // 获取触摸点的屏幕坐标
        Vector2 screenPosition = touch.position;

        // 将屏幕坐标转换为世界坐标
        Vector3 screenPos3D = new Vector3(screenPosition.x, screenPosition.y, 0);
        Vector3 worldPos3D = _camera.ScreenToWorldPoint(screenPos3D);
        _tempoVector2 = new Vector2(worldPos3D.x, worldPos3D.y);

        // 初始化手指数据
        var newFinger = new Fingers()
        {
            fingerId = touch.fingerId,
            nowPosition = _tempoVector2,
            lastPosition = _tempoVector2,
            lastPositions = new Vector2[3]
        };

        // 初始化位置历史数组（存储最近3个位置）
        for (int i = 0; i < 3; i++)
        {
            newFinger.lastPositions[i] = _tempoVector2;
        }

        // 添加到手指管理列表
        fingers.Add(newFinger);
    }

    public void SyncFingers()
    {
        // 1. 获取当前所有触摸点
        Touch[] touches = Input.touches;

        // 2. 同步手指列表：添加新触摸点
        for (int i = 0; i < touches.Length; i++)
        {
            Touch touch = touches[i];
            int fingerId = touch.fingerId;

            // 如果手指不在管理列表中，添加新手指
            if (FindFingerIndexWithFingerId(fingerId) == -1)
            {
                AddFinger(touch);
            }
        }

        // 3. 同步手指列表：移除已消失的触摸点
        for (int i = fingers.Count - 1; i >= 0; i--)
        {
            Fingers finger = fingers[i];
            int touchIndex = FindTouchIndexWithFingerId(finger.fingerId);

            // 如果触摸点已消失，从列表中移除
            if (touchIndex == -1)
            {
                fingers.RemoveAt(i);
            }
        }

        // 4. 更新所有手指状态
        foreach (Fingers finger in fingers)
        {
            // 查找对应的触摸点
            Touch? touch = FindTouchWithFingerId(finger.fingerId);
            if (!touch.HasValue) continue;

            // 更新位置历史（队列式位移）
            finger.lastPosition = finger.nowPosition;
            finger.lastPositions[2] = finger.lastPositions[1];
            finger.lastPositions[1] = finger.lastPositions[0];

            // 转换屏幕坐标到世界坐标
            Vector2 screenPos = touch.Value.position;
            Vector3 worldPos = _camera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, 0)
            );

            // 更新当前位置
            finger.nowPosition = new Vector2(worldPos.x, worldPos.y);
            finger.lastPositions[0] = finger.nowPosition;

            // 更新触摸状态（phase）
            finger.phase = touch.Value.phase;
        }
    }

    public Touch? FindTouchWithFingerId(int fingerId)
    {
        // 获取当前所有触摸点
        Touch[] touches = Input.touches;

        // 遍历查找匹配的触摸点
        foreach (Touch touch in touches)
        {
            if (touch.fingerId == fingerId)
            {
                return touch;
            }
        }

        // 未找到匹配项
        return null;
    }

    public int FindTouchIndexWithFingerId(int fingerId)
    {
        // 获取当前所有触摸点
        Touch[] touches = Input.touches;

        // 检查触摸数组是否为空
        if (touches == null)
        {
            return -1;
        }

        // 遍历所有触摸点查找匹配项
        for (int i = 0; i < touches.Length; i++)
        {
            // 获取当前触摸点
            Touch touch = touches[i];

            // 检查fingerId是否匹配
            if (touch.fingerId == fingerId)
            {
                return i; // 返回匹配的索引
            }
        }

        return -1; // 未找到匹配项
    }

    public int FindFingerIndexWithFingerId(int fingerId)
    {
        // 获取手指列表
        List<Fingers> fingers = this.fingers;

        // 检查列表是否为空
        if (fingers == null || fingers.Count == 0)
        {
            return -1;
        }

        // 遍历手指列表查找匹配项
        for (int i = 0; i < fingers.Count; i++)
        {
            // 获取当前手指
            Fingers finger = fingers[i];

            // 检查fingerId是否匹配
            if (finger != null && finger.fingerId == fingerId)
            {
                return i; // 返回匹配的索引
            }
        }

        return -1; // 未找到匹配项
    }
}