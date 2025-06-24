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
            // ��ȡ��ǰ��Ļ�ֱ���
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            this._screenWidth = screenWidth;
            this._screenHeight = screenHeight;

            // ������Ļ��߱�
            screenRatio = (float)screenWidth / screenHeight;

            // ȷ��CanvasScalerƥ��ģʽ
            matchValue = screenRatio >= targetAspect ? 0.5f : 0.0f;

            // ����CanvasScaler����
            if (Mathf.Abs(contentCanvasScaleSetter.matchWidthOrHeight - matchValue) > 0.1f)
            {
                contentCanvasScaleSetter.matchWidthOrHeight = matchValue;
                worldCanvasScaleSetter.matchWidthOrHeight = matchValue;

                // �ȴ�һ֡��UIϵͳ����
                yield return null;
            }

            // �������ݻ�������ֵ
            Vector3 canvasScalePos = canvasScaleFlag.position;
            Vector3 canvasScaleParentPos = canvasScaleFlag.parent.position;
            this._contentCanvasScale = canvasScalePos.y - canvasScaleParentPos.y;

            // �������续������ֵ
            Vector3 worldScalePos = worldScaleFlag.position;
            Vector3 worldScaleParentPos = worldScaleFlag.parent.position;
            this._worldCanvasScale = worldScalePos.y - worldScaleParentPos.y;

            // ����������ߴ�
            // this._chapterCoverSize = _songCoverPanelRectTransform.sizeDelta;

            // �������������С
            float orthoSize = screenRatio >= maxRatio
                ? (5.0f / screenRatio) * maxRatio
                : this._startScale;
            backgroundCamera.orthographicSize = orthoSize;

            // ����������λ��
            /*if (topBarAnchors != null && topBarAnchors.Length > 1)
            {
                Vector2 anchorPos = topBarAnchors[0].anchoredPosition;
                Vector3 topBarPos = topBarAnchors[1].position;
                Vector3 canvasScaleParent = canvasScaleFlag.parent.position;

                float xPos = anchorPos.x + ((topBarPos.y - canvasScaleParent.y) * 0.26795f / _contentCanvasScale) * 100f;
                float yPos = ((topBarPos.y - canvasScaleParent.y) / _contentCanvasScale) * 100f;

                topBarAnchors[1].anchoredPosition = new Vector2(xPos, yPos);
            }*/

            // �ȴ��ֱ��ʱ仯
            yield return new WaitUntil(() =>
                _screenWidth != Screen.width ||
                _screenHeight != Screen.height);
        }
    }

}
