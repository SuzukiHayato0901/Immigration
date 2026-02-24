using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// ドラッグ可能なドキュメント（UI）を管理するクラス
/// マウスでドラッグして移動でき、親要素の範囲外に出たら元の位置に戻る
/// </summary>
public class DocumentDraggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler
{
    [Header("ドキュメントデータ")]
    public DocumentDates Data;  // パスポートデータ（ScriptableObject）
    public TextMeshProUGUI nameText;  // 名前を表示するテキスト
    public TextMeshProUGUI counterText; // カウンター名を表示するテキスト
    public TextMeshProUGUI idNameText; // 書類ID名を表示するテキスト
    public TextMeshProUGUI expiryDateText; // 有効期限を表示するテキスト

    [Header("エフェクト")]
    public RectTransform shadowRect; // ドキュメントの影（ドラッグ中に表示）
    public Vector2 shadowOffset = new Vector2(10f, -10f); // 影のオフセット
    public Vector2 originalShadowPos; // 影の元の位置（ドラッグ終了後に戻すために保存）

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

        // 影の初期位置を保存
        if(shadowRect != null)
        {
            // 影の元の位置を保存（ドラッグ終了後に戻すため）
            originalShadowPos = shadowRect.anchoredPosition;
        }

        UpdateDocumentUI(); // ドキュメントデータをUIに反映
    }

    /// <summary>
    /// ドキュメントデータをUIに反映するメソッド
    /// </summary>
    public void UpdateDocumentUI()
    {
        if(nameText != null && Data != null)
        {
            nameText.text = Data.personName;            // 所有者の名前を表示（DocumentDates.personName を使用）
        }
        if(counterText != null && Data != null)
        {
            counterText.text = Data.counter.ToString(); // カウンター名を表示
        }
        if(idNameText != null && Data != null)
        {
            idNameText.text = Data.idName.ToString();   // 書類ID名を表示
        }
        if(expiryDateText != null && Data != null)
        {
            expiryDateText.text = Data.expiryDate.ToString(); // 有効期限を表示
        }
        if(Data != null)
        {
            return;     // データが存在する場合は正常にUIを更新したとみなす
        }
    }

    /// <summary>
    /// ドキュメントがマウスでクリックされたときに呼ばれる
    /// ドキュメントを最前面に表示する
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // ドキュメントを最前面に表示
        rectTransform.SetAsLastSibling();

        // ドキュメントの影をドラッグ中に表示
        if(shadowRect != null)
        {
            shadowRect.anchoredPosition = originalShadowPos + shadowOffset; // 影をオフセット位置に移動して表示
        }
    }

    /// <summary>
    /// ドラッグ開始時に呼ばれる
    /// 1. 元の位置を保存
    /// 2. マウスとドキュメント中心のズレを計算
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // デバッグ情報のログ出力
        // Debug.Log($"クリック位置: {eventData.position}");
        // Debug.Log($"カメラ: {canvas.worldCamera}");
        // Debug.Log($"親RectTransform: {parentRectTransform.name}");
        // Debug.Log($"クリックされたオブジェクト: {eventData.pointerCurrentRaycast.gameObject.name}");
        // Debug.Log($"書類のサイズ: {rectTransform.rect}");
        // Debug.Log($"書類の位置: {rectTransform.anchoredPosition}");

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
        // ドキュメントの影をドラッグ終了後に元の位置に戻す
        if(shadowRect != null)
        {
            shadowRect.anchoredPosition = originalShadowPos; // 影を元の位置に戻す
        }

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