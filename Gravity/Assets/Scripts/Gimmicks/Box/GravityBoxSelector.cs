using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class GravityBoxSelector : MonoBehaviour
{
    [Header("選択設定")]
    [SerializeField] private float selectRange = 5f;        // 選択可能範囲(m)
    [SerializeField] private PlayerCamera playerCamera;     // カメラへの参照

    [Header("移動設定")]
    [SerializeField] private float followSpeed = 5f;        // 箱の追従速度
    [SerializeField] private float depthMoveSpeed = 3f;     // 奥行移動速度

    // 状態管理
    private enum State { None, Selected, Controlled }  // 通常、選択候補、操作中
    private State currentState = State.None;            // 現在の状態

    private GravityBox selectedBox;     // 現在選択中の箱
    private Vector3 targetPosition;     // 箱の目標位置
    private Camera cam;                 // カメラコンポーネントの参照

    private void Start()
    {
        cam = playerCamera.GetComponent<Camera>(); // カメラコンポーネントを取得
    }

    private void Update()
    {
        HandleFKey();

        switch (currentState)
        {
            case State.Selected:
                HandleLeftClick();  // 左クリック長押しで操作開始
                break;

            case State.Controlled:
                HandleMove();       // マウスでXY軸移動
                HandleDepthMove();  // Q/EでZ軸移動
                LimitRange();       // 範囲制限
                break;
        }
    }

    // Fキーで選択/解除するメソッド
    private void HandleFKey()
    {
        if (!Keyboard.current.fKey.wasPressedThisFrame) return;

        if (currentState == State.None)
        {
            // 範囲内の最も近い箱を探して選択候補にする
            GravityBox nearestBox = FindNearestBox();
            if (nearestBox != null)
            {
                selectedBox = nearestBox;
                selectedBox.SetSelected(true);          // 発光ON
                currentState = State.Selected;

                playerCamera.SetCameraEnabled(false);   // カメラ操作を無効化

                // マウスポインターを表示
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            // 選択中・操作中どちらでも解除
            Deselect();
        }
    }

    // 左クリック長押しで操作開始するメソッド
    private void HandleLeftClick()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            targetPosition = selectedBox.transform.position;    // 現在位置を目標位置に設定
            currentState = State.Controlled;                    // 操作中に移行
        }
    }

    // 箱の選択を解除するメソッド
    private void Deselect()
    {
        if (selectedBox != null)
        {
            selectedBox.SetSelected(false);             // 発光OFF
        }
        selectedBox = null;
        currentState = State.None;

        playerCamera.SetCameraEnabled(true);            // カメラ操作を有効化

        // マウスポインターを非表示
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 5m以内で最も近い箱を探すメソッド
    private GravityBox FindNearestBox()
    {
        GravityBox nearest = null;
        float nearestDistance = selectRange;

        // Fキー押下時のみ取得するので軽い
        GravityBox[] boxes = FindObjectsByType<GravityBox>(FindObjectsSortMode.None);
        foreach (GravityBox box in boxes)
        {
            float distance = Vector3.Distance(transform.position, box.transform.position);
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = box;
            }
        }

        return nearest;
    }

    // マウス移動に応じて箱をXY方向に移動させるメソッド
    private void HandleMove()
    {
        // 左クリックを離したら操作終了、選択状態に戻る
        if (!Mouse.current.leftButton.isPressed)
        {
            currentState = State.Selected;
            return;
        }

        // マウスのデルタ値(移動量)を取得
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        if (mouseDelta == Vector2.zero) return;

        // カメラの右方向と上方向に移動量を掛けて目標位置を更新
        targetPosition += playerCamera.transform.right * mouseDelta.x * Time.deltaTime;
        targetPosition += playerCamera.transform.up * mouseDelta.y * Time.deltaTime;

        // 壁貫通チェック
        if (Physics.Linecast(transform.position, targetPosition, out RaycastHit hit))
        {
            targetPosition = hit.point;     // 障害物手前で停止
        }

        // DOTweenで遅れて追従
        selectedBox.transform.DOMove(targetPosition, 1f / followSpeed)
            .SetEase(Ease.OutQuad);
    }

    // Q/E長押しで奥行方向に移動させるメソッド
    private void HandleDepthMove()
    {
        Vector3 depthDirection = Vector3.zero;

        if (Keyboard.current.qKey.isPressed)
        {
            depthDirection = playerCamera.transform.forward;    // 奥へ
        }
        else if (Keyboard.current.eKey.isPressed)
        {
            depthDirection = -playerCamera.transform.forward;   // 手前へ
        }

        if (depthDirection == Vector3.zero) return;

        targetPosition += depthDirection * depthMoveSpeed * Time.deltaTime;
    }

    // 箱がプレイヤーから5m以内に収まるよう制限するメソッド
    private void LimitRange()
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > selectRange)
        {
            // 範囲外なら境界上にクランプ
            Vector3 direction = (targetPosition - transform.position).normalized;
            targetPosition = transform.position + direction * selectRange;
        }
    }
}