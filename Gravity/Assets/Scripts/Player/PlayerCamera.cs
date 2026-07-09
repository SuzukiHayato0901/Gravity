using UnityEngine;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    [Header("ターゲット設定")]
    [SerializeField] GameObject player;                 // プレイヤーオブジェクト
    public Transform target;                            // プレイヤーのTransformを格納

    [Header("マウス設定")]
    public float mouseSensitivity = 3f;     // マウス感度
    public float pitchMin = -80f;           // 上下の最小角度
    public float pitchMax = 60f;            // 上下の最大角度

    [Header("カメラオフセット")]
    [SerializeField] private Vector3 normaloffset = new Vector3(0f, 5f, -7f);    // 通常時のカメラオフセット
    [SerializeField] private Vector3 reverseOffset = new Vector3(0f, -5f, -7f);  // 重力反転時のカメラオフセット
    [SerializeField] private float offsetDuration = 0.5f;                        // オフセットの切り替えにかかる時間

    public float yaw = 0f;      // 左右回転
    public float pitch = 0f;    // 上下回転

    private Vector3 currentOffset;              // 現在のカメラオフセット
    private GravityController gravityController; // 重力制御クラスの参照
    private bool isCameraEnabled = true;         // カメラ操作の有効/無効フラグ

    void Start()
    {
        // マウスカーソルを非表示にし、画面中央にロックして動かないようにする
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初期オフセットを通常時に設定
        currentOffset = normaloffset;

        // GravityControllerを取得
        if (target != null)
        {
            gravityController = target.GetComponent<GravityController>();
        }
    }

    void Update()
    {
        CamereMove();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;   // マウスのロックを解除
            Cursor.visible = true;                    // マウスカーソルを表示
        }
    }

    void LateUpdate()
    {
        // ターゲットが設定されていない場合は処理を中断
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);             // 回転をクォータニオンで計算
        transform.position = target.position + rotation * currentOffset;    // ターゲットの位置にオフセットを加えてカメラの位置を設定
        transform.LookAt(target);                                           // ターゲットを常に見るようにする
    }

    // カメラの回転をマウス入力に基づいて更新
    void CamereMove()
    {
        if (!isCameraEnabled) return; // カメラ操作が無効なら処理しない

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }

    // 重力反転時のカメラオフセットを切り替えるメソッド
    public void OnGravityChanged(bool isReversed)
    {
        Vector3 targetOffset = isReversed ? reverseOffset : normaloffset;   // 目標オフセットを選択

        // 現在のオフセットから目標オフセットへDOTweenで補間
        DOTween.To(() => currentOffset, x => currentOffset = x, targetOffset, offsetDuration)
            .SetEase(Ease.OutQuad);  // 補間のイージングを設定
    }

    // 外部からカメラ操作を有効/無効にするメソッド
    public void SetCameraEnabled(bool enabled)
    {
        isCameraEnabled = enabled;
    }
}