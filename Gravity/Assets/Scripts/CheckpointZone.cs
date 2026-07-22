using UnityEngine;

/// <summary>
/// プレイヤーがエリアに入ったらリスポーン地点を更新するクラス
/// BoxColliderのIsTriggerをONにして使用する
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class CheckpointZone : MonoBehaviour
{
    [Header("チェックポイント設定")]
    [SerializeField] private Transform respawnPoint;    // リスポーン地点のTransform
    [SerializeField] private int checkpointIndex;       // チェックポイントの番号(順番管理用)

    [Header("演出設定")]
    [SerializeField] private Color activeColor = Color.green;       // 有効時の色
    [SerializeField] private Color inactiveColor = Color.gray;      // 無効時の色

    private Renderer zoneRenderer;     // ゾーンのRenderer
    private bool isActive = false;     // このチェックポイントが有効かどうか

    private void Awake()
    {
        // IsTriggerを強制的にONにする
        GetComponent<BoxCollider>().isTrigger = true;

        zoneRenderer = GetComponent<Renderer>();
        SetVisual(false);   // 初期状態は無効色
    }

    // プレイヤーがエリアに入ったらリスポーン地点を更新
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // RespawnManagerにリスポーン地点を通知
        RespawnManager.Instance.UpdateCheckpoint(respawnPoint, checkpointIndex);

        // このチェックポイントを有効化
        SetActive(true);
    }

    // チェックポイントの有効/無効を設定するメソッド(RespawnManagerから呼ばれる)
    public void SetActive(bool active)
    {
        isActive = active;
        SetVisual(active);
    }

    // ゾーンの見た目を切り替えるメソッド
    private void SetVisual(bool active)
    {
        if (zoneRenderer == null) return;
        zoneRenderer.material.color = active ? activeColor : inactiveColor;
    }

    // チェックポイントの番号を取得するプロパティ
    public int CheckpointIndex => checkpointIndex;
}
