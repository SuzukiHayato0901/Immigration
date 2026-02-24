using UnityEngine;

[CreateAssetMenu(fileName = "DocumentDates", menuName = "データ/書類データ")]
public class DocumentDates : ScriptableObject
{
    public string personName;   // ドキュメント所有者のフルネーム。例: "山田太郎"
    public string counter;      // ドキュメントが扱われるカウンターの識別名。使用例: "Counter A" または "受付１"
    public string idName;       // 書類に付けられた識別ID名。ユニークである必要はないが区別しやすい名前を付ける。例: "Document 1" や "ビザ申請書"
    public string expiryDate;   // 書類の有効期限。ISO 8601 形式 (YYYY-MM-DD) で記録するのが推奨。例: "2024-12-31"
}
