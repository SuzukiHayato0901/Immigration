using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UIドラッグ機能を実装するクラス
/// IBeginDragHandler: ドラッグ開始時に元の位置を保存
/// IDragHandler: ドラッグ中に位置を更新
/// IEndDragHandler: ドラッグ終了時に範囲外ならリセット
/// ※注意:上記のインターフェースメソッドをすべて実装する必要があります。
///       実装していないとコンパイルエラーが発生します。
/// </summary>
public class DocumentDraggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private Vector2 prevPos;                    // ドラッグ開始時のドキュメント位置
    private RectTransform rectTransform;        // このオブジェクトのRectTransform
    private RectTransform parentRectTransform;  // 親要素のRectTransform

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();                  // このオブジェクトのRectTransformを取得
        parentRectTransform = rectTransform.parent as RectTransform;    // 親オブジェクトのRectTransformを取得
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        prevPos = rectTransform.anchoredPosition;  // ドラッグ開始時の位置を記録
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPosition = GetLocalPosition(eventData.position);
        rectTransform.anchoredPosition = localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 親要素の範囲外にドラッグされた場合は、元の位置に戻す
        if (!RectTransformUtility.RectangleContainsScreenPoint(parentRectTransform, rectTransform.position, eventData.pressEventCamera))
        {
            rectTransform.anchoredPosition = prevPos;
        }
    }

    private Vector2 GetLocalPosition(Vector2 screenPosition)
    {
        // スクリーン座標を親要素のローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, screenPosition, Camera.main, out Vector2 result);
        return result;
    }
}
