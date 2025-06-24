using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PPDD : MonoBehaviour
{
    public bool exit;
    
    public CanvasScaler contentCanvasScaleSetter;
    public CanvasScaler worldCanvasScaleSetter;
    public Transform canvasScaleFlag; // 0x200
    public Transform worldScaleFlag; // 0x208

    public Camera backgroundCamera; // 0x158

    private int _screenWidth;
    private int _screenHeight;

    private float _contentCanvasScale; // 0x70
    private float _worldCanvasScale; // 0x74

    private float _startScale; // 0x26C

    // Start is called before the first frame update
    void Start()
    {
        _startScale = backgroundCamera.orthographicSize;
        StartCoroutine(CheckScreenResolution());
    }

    private IEnumerator CheckScreenResolution()
    {
        const float maxRatio = 1.8963f;
        const float targetAspect = 1.7778f; // 16:9
        float screenRatio;
        float matchValue;

        while (!exit)
        {
            // 获取当前屏幕分辨率
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            this._screenWidth = screenWidth;
            this._screenHeight = screenHeight;

            // 计算屏幕宽高比
            screenRatio = (float)screenWidth / screenHeight;

            // 确定CanvasScaler匹配模式
            matchValue = screenRatio >= targetAspect ? 0.5f : 0.0f;

            // 更新CanvasScaler设置
            if (Mathf.Abs(contentCanvasScaleSetter.matchWidthOrHeight - matchValue) > 0.1f)
            {
                contentCanvasScaleSetter.matchWidthOrHeight = matchValue;
                worldCanvasScaleSetter.matchWidthOrHeight = matchValue;

                // 等待一帧让UI系统更新
                yield return null;
            }

            // 计算内容画布缩放值
            Vector3 canvasScalePos = canvasScaleFlag.position;
            Vector3 canvasScaleParentPos = canvasScaleFlag.parent.position;
            this._contentCanvasScale = canvasScalePos.y - canvasScaleParentPos.y;

            // 计算世界画布缩放值
            Vector3 worldScalePos = worldScaleFlag.position;
            Vector3 worldScaleParentPos = worldScaleFlag.parent.position;
            this._worldCanvasScale = worldScalePos.y - worldScaleParentPos.y;

            // 保存封面面板尺寸
            // this._chapterCoverSize = _songCoverPanelRectTransform.sizeDelta;

            // 调整背景相机大小
            float orthoSize = screenRatio >= maxRatio
                ? (5.0f / screenRatio) * maxRatio
                : this._startScale;
            backgroundCamera.orthographicSize = orthoSize;

            // 调整顶部栏位置
            /*if (topBarAnchors != null && topBarAnchors.Length > 1)
            {
                Vector2 anchorPos = topBarAnchors[0].anchoredPosition;
                Vector3 topBarPos = topBarAnchors[1].position;
                Vector3 canvasScaleParent = canvasScaleFlag.parent.position;

                float xPos = anchorPos.x + ((topBarPos.y - canvasScaleParent.y) * 0.26795f / _contentCanvasScale) * 100f;
                float yPos = ((topBarPos.y - canvasScaleParent.y) / _contentCanvasScale) * 100f;

                topBarAnchors[1].anchoredPosition = new Vector2(xPos, yPos);
            }*/

            // 等待分辨率变化
            yield return new WaitUntil(() =>
                _screenWidth != Screen.width ||
                _screenHeight != Screen.height);
        }
    }

}
