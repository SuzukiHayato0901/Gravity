using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("ターゲット設定")]
    [SerializeField] GameObject player;                 // プレイヤーオブジェクト
    public Transform target;                            // プレイヤーのTransformを格納
    public Vector3 offset = new Vector3(0f, 3f, -5f);   // カメラの位置オフセット

    [Header("マウス設定")]
    public float mouseSensitivity = 3f;     // マウス感度
    public float pitchMin = -80f;           // 上下の最小角度
    public float pitchMax = 60f;            // 上下の最大角度

    public float yaw = 0f;     // 左右回転
    public float pitch = 0f;   // 上下回転

    void Start()
    {
        // マウスカーソルを非表示にし、画面中央にロックして動かないようにする
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        if (target == null)
        {
            return;
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);     // 回転をクォータニオンで計算
        transform.position = target.position + rotation * offset;   // ターゲットの位置にオフセットを加えてカメラの位置を設定
        transform.LookAt(target);                                   // ターゲットを常に見るようにする
    }

    // カメラの回転をマウス入力に基づいて更新
    void CamereMove()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }
}