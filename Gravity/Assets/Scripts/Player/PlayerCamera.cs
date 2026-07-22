// PlayerCamera.cs
using UnityEngine;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    [Header("ターゲット設定")]
    [SerializeField] GameObject player;
    public Transform target;

    [Header("マウス設定")]
    public float mouseSensitivity = 3f;
    public float pitchMin = -80f;
    public float pitchMax = 60f;

    [Header("カメラオフセット")]
    [SerializeField] private Vector3 normaloffset = new Vector3(0f, 5f, -7f);    // 通常時のカメラオフセット
    [SerializeField] private Vector3 reverseOffset = new Vector3(0f, -5f, -7f);  // 重力反転時のカメラオフセット
    [SerializeField] private float offsetDuration = 0.5f;                        // オフセットの切り替えにかかる時間

    [Header("箱追従設定")]
    [SerializeField] private float cameraRotateDuration = 0.5f;  // カメラが箱の反対側に回り込む速度

    public float yaw = 0f;
    public float pitch = 0f;

    private Vector3 currentOffset;
    private GravityController gravityController;
    private bool isCameraEnabled = true;
    //private bool isFollowingBox = false;    // 箱追従モード中かどうかのフラグ

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentOffset = normaloffset;

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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target.position + rotation * currentOffset;
        transform.LookAt(target);
    }

    void CamereMove()
    {
        if (!isCameraEnabled) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }

    // 重力反転時のカメラオフセットを切り替えるメソッド
    public void OnGravityChanged(bool isReversed)
    {
        Vector3 targetOffset = isReversed ? reverseOffset : normaloffset;

        DOTween.To(() => currentOffset, x => currentOffset = x, targetOffset, offsetDuration)
            .SetEase(Ease.OutQuad);
    }

    // 外部からカメラ操作を有効/無効にするメソッド
    public void SetCameraEnabled(bool enabled)
    {
        isCameraEnabled = enabled;
    }

    // 箱の位置に応じてカメラをプレイヤーの反対側に回り込ませるメソッド
    public void TrackBoxPosition(Vector3 boxPosition)
    {
        if (target == null) return;

        // プレイヤーから箱への方向をXZ平面で計算
        Vector3 toBox = boxPosition - target.position;
        toBox.y = 0f;

        if (toBox.sqrMagnitude < 0.01f) return; // 箱がプレイヤーと同位置なら無視

        // 符号を反転。プレイヤー→カメラの向きが「箱と反対方向」になるよう修正
        // (offsetがローカル-Z方向のため、forward(yaw)はtoBoxと同じ向きにする必要がある)
        Vector3 cameraDirection = toBox.normalized;
        float targetYaw = Mathf.Atan2(cameraDirection.x, cameraDirection.z) * Mathf.Rad2Deg;

        // DOTweenで滑らかにyaw回転
        DOTween.To(() => yaw, x => yaw = x, targetYaw, cameraRotateDuration)
            .SetEase(Ease.OutQuad);
    }
}