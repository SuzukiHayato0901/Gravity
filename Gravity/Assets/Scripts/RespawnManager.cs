using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// リスポーン処理を管理するクラス
/// Lキーでリスポーン、GravityBoxと重力を初期状態に戻す
/// シングルトンパターンで実装
/// </summary>
public class RespawnManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static RespawnManager Instance { get; private set; }

    [Header("参照設定")]
    [SerializeField] private Transform player;                  // プレイヤーのTransform
    [SerializeField] private Rigidbody playerRigidbody;         // プレイヤーのRigidbody
    [SerializeField] private GravityController gravityController; // 重力制御クラス
    [SerializeField] private Transform defaultRespawnPoint;     // デフォルトのリスポーン地点

    private Transform currentRespawnPoint;      // 現在のリスポーン地点
    private int currentCheckpointIndex = -1;    // 現在のチェックポイント番号(-1はデフォルト)

    // フィールド上の全GravityBoxの初期状態を保存する構造体
    private struct GravityBoxState
    {
        public GravityBox box;          // GravityBoxの参照
        public Vector3 initialPosition; // 初期位置
        public Quaternion initialRotation; // 初期回転
    }

    private GravityBoxState[] gravityBoxStates; // 全GravityBoxの初期状態

    private void Awake()
    {
        // シングルトンの設定
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // デフォルトのリスポーン地点を設定
        currentRespawnPoint = defaultRespawnPoint;

        // シーン内の全GravityBoxの初期状態を保存
        SaveGravityBoxStates();
    }

    private void Update()
    {
        // Lキーでリスポーン
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Respawn();
        }
    }

    // シーン内の全GravityBoxの初期状態を保存するメソッド
    private void SaveGravityBoxStates()
    {
        GravityBox[] boxes = FindObjectsByType<GravityBox>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        gravityBoxStates = new GravityBoxState[boxes.Length];

        for (int i = 0; i < boxes.Length; i++)
        {
            gravityBoxStates[i] = new GravityBoxState
            {
                box = boxes[i],
                initialPosition = boxes[i].transform.position,
                initialRotation = boxes[i].transform.rotation
            };
        }
    }

    // チェックポイントを更新するメソッド(CheckpointZoneから呼ばれる)
    public void UpdateCheckpoint(Transform respawnPoint, int checkpointIndex)
    {
        // 現在のチェックポイントより番号が大きい場合のみ更新
        if (checkpointIndex <= currentCheckpointIndex) return;

        currentRespawnPoint = respawnPoint;
        currentCheckpointIndex = checkpointIndex;

        // 全チェックポイントの見た目を更新
        UpdateCheckpointVisuals();

        Debug.Log($"チェックポイント {checkpointIndex} を更新しました");
    }

    // リスポーン処理を行うメソッド
    private void Respawn()
    {
        // プレイヤーをリスポーン地点に移動
        player.position = currentRespawnPoint.position;
        player.rotation = currentRespawnPoint.rotation;

        // プレイヤーの速度をリセット
        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;

        // 重力を初期状態(通常重力)に戻す
        gravityController.ResetGravity();

        // GravityBoxを初期位置に戻す
        ResetGravityBoxes();

        Debug.Log("リスポーンしました");
    }

    // 全GravityBoxを初期状態に戻すメソッド
    private void ResetGravityBoxes()
    {
        foreach (GravityBoxState state in gravityBoxStates)
        {
            if (state.box == null) continue;

            // 位置と回転を初期状態に戻す
            state.box.transform.position = state.initialPosition;
            state.box.transform.rotation = state.initialRotation;

            // GravityBoxの状態をリセット
            state.box.ResetState();
        }
    }

    // 全チェックポイントの見た目を更新するメソッド
    private void UpdateCheckpointVisuals()
    {
        CheckpointZone[] checkpoints = FindObjectsByType<CheckpointZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (CheckpointZone checkpoint in checkpoints)
        {
            // 現在のチェックポイント番号以下のものを有効化
            checkpoint.SetActive(checkpoint.CheckpointIndex <= currentCheckpointIndex);
        }
    }
}
