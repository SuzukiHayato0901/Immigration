using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ドラッグ可能なドキュメント（UI）を管理するクラス
/// マウスでドラッグして移動でき、親要素の範囲外に出たら元の位置に戻る
/// </summary>
public class DocumentDraggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    // ドラッグ開始時のドキュメント位置（範囲外判定用に保存）
    private Vector2 prevPos;
    
    // このオブジェクトのRectTransform（位置情報）
    private RectTransform rectTransform;
    
    // 親要素のRectTransform（ドラッグ範囲の判定に使用）
    private RectTransform parentRectTransform;
    
    // マウス位置とドキュメント中心のズレ（ドラッグ中の位置計算に使用）
    private Vector2 offset;
    
    // Canvas参照（RenderMode判定でカメラを取得するために必要）
    private Canvas canvas;

    /// <summary>
    /// 初期化：参照を取得
    /// </summary>
    public void Awake()
    {
        // このゲームオブジェクトのRectTransform取得
        rectTransform = GetComponent<RectTransform>();
        
        // 親要素のRectTransform取得（ドラッグ範囲の判定に使用）
        parentRectTransform = rectTransform.parent as RectTransform;
        
        // Canvasの参照を取得（カメラの判定に使用）
        canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// ドラッグ開始時に呼ばれる
    /// 1. 元の位置を保存
    /// 2. マウスとドキュメント中心のズレを計算
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // デバッグ情報のログ出力
        Debug.Log($"クリック位置: {eventData.position}");
        Debug.Log($"カメラ: {canvas.worldCamera}");
        Debug.Log($"親RectTransform: {parentRectTransform.name}");
        Debug.Log($"クリックされたオブジェクト: {eventData.pointerCurrentRaycast.gameObject.name}");
        Debug.Log($"書類のサイズ: {rectTransform.rect}");
        Debug.Log($"書類の位置: {rectTransform.anchoredPosition}");

        // ドラッグ開始時の位置を保存（範囲外判定時に使用）
        prevPos = rectTransform.anchoredPosition;

        // ScreenSpaceOverlay時はカメラ不要（null）、それ以外はCanvasのカメラを使用
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        // スクリーン座標を親のローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform,     // 親要素の座標系
            eventData.position,      // マウスのスクリーン座標
            cam,                     // 使用するカメラ
            out Vector2 mousePosInParent  // 変換結果
        );

        // マウス位置とドキュメント中心のズレを計算
        offset = rectTransform.anchoredPosition - mousePosInParent;
        
        // ドキュメントを最前面に表示
        rectTransform.SetAsLastSibling();
    }

    /// <summary>
    /// ドラッグ中に毎フレーム呼ばれる
    /// ドキュメントの位置をマウスに合わせて更新
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // ScreenSpaceOverlay時はカメラ不要（null）、それ以外はCanvasのカメラを使用
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        // スクリーン座標を親のローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            parentRectTransform,            // 親要素の座標系　第一引数
            eventData.position,             // マウスのスクリーン座標
            cam,                            // 使用するカメラ
            out Vector2 mousePosInParent    // 変換結果
        );
        
        // ドキュメントの位置を更新（ズレを加えてスムーズなドラッグを実現）
        rectTransform.anchoredPosition = mousePosInParent + offset;
    }

    /// <summary>
    /// ドラッグ終了時に呼ばれる
    /// 親要素の範囲外に出た場合は元の位置に戻す
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // ScreenSpaceOverlay時はカメラ不要（null）、それ以外はCanvasのカメラを使用
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        /// <summary>
        /// ドキュメントが親要素の範囲内にあるかをチェック
        /// parentRectTransform: 親要素の範囲
        /// eventData.position: マウスのスクリーン座標
        /// cam: メインカメラ
        /// rectTransform.positionだとキャンバスにメインカメラを入れたからScaleが変わってしまい
        /// 正しく範囲内判定ができない。
        /// </summary>
        if (!RectTransformUtility.RectangleContainsScreenPoint(parentRectTransform, eventData.position, cam))
        {
            //  範囲外の場合は元の位置に戻す
            rectTransform.anchoredPosition = prevPos;
        }
    }
}