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
    private Camera cam;                 // カメラコンポーネント
    private float fixedDistance;        // 操作開始時のカメラと箱の距離(固定)

    private Collider selectedBoxCollider;   // 選択中の箱のCollider(Linecast判定除外用)
    private Vector3 targetVelocity;         // SmoothDamp用の内部速度バッファ(ノイズ低減用)
    // ★削除:selectedBoxRigidbody(isKinematic制御用だったが、地面めり込みの原因のため削除)

    private void Start()
    {
        // カメラコンポーネントを取得
        cam = playerCamera.GetComponent<Camera>();
    }

    private void Update()
    {
        HandleFKey();

        switch (currentState)
        {
            case State.Selected:
                HandleLeftClick();
                break;

            case State.Controlled:
                HandleMove();
                HandleDepthMove();
                // LimitRangeはHandleMove内でtargetPosition確定時に適用するためここでは呼ばない
                break;
        }
    }

    // Fキーで選択/解除するメソッド
    private void HandleFKey()
    {
        if (!Keyboard.current.fKey.wasPressedThisFrame) return;

        if (currentState == State.None)
        {
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

                // 選択時にカメラを箱の反対側に回り込ませる
                playerCamera.TrackBoxPosition(selectedBox.transform.position);
            }
        }
        else
        {
            Deselect();
        }
    }

    // 左クリック押した瞬間に操作開始するメソッド
    private void HandleLeftClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        // 操作開始時の箱位置をtargetPositionに設定
        targetPosition = selectedBox.transform.position;

        // 操作開始時のカメラと箱の距離を固定(操作中は変更しない)
        fixedDistance = Vector3.Distance(cam.transform.position, selectedBox.transform.position);

        // Linecast除外用に参照を取得
        selectedBoxCollider = selectedBox.GetComponent<Collider>();
        // ★削除:Rigidbody取得・isKinematic=true化(地面めり込みの原因だったため)

        targetVelocity = Vector3.zero;  // SmoothDampの速度バッファを初期化

        currentState = State.Controlled;
    }

    // 箱の選択を解除するメソッド
    private void Deselect()
    {
        if (selectedBox != null)
        {
            selectedBox.transform.DOKill();             // 実行中のTweenを停止
            selectedBox.SetSelected(false);             // 発光OFF
            // ★削除:isKinematicを戻す処理(不要になったため削除)
        }
        selectedBox = null;
        selectedBoxCollider = null;   // 参照をクリア
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

    // スクリーン座標をワールド座標に変換して箱を移動させるメソッド
    private void HandleMove()
    {
        // 左クリックを離したらカメラを追従させて選択状態に戻る
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // B案: マウスを離したタイミングでカメラを箱の反対側に回り込ませる
            playerCamera.TrackBoxPosition(targetPosition);
            currentState = State.Selected;
            return;
        }

        if (!Mouse.current.leftButton.isPressed) return;

        // マウスのスクリーン座標を取得
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // Z値にカメラから箱までの固定距離を設定してワールド座標に変換
        // fixedDistanceを固定することでZ軸の混入を防ぐ
        Vector3 screenPos = new Vector3(mouseScreenPos.x, mouseScreenPos.y, fixedDistance);
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);

        // 生のworldPosをそのまま使わずSmoothDampでノイズを低減してからtargetPositionに反映
        // (カメラがプレイヤーから離れているため、マウスの微小な動きが増幅されるのを緩和)
        targetPosition = Vector3.SmoothDamp(targetPosition, worldPos, ref targetVelocity, 0.1f);

        // 壁貫通チェック(箱自身のColliderを一時的に無効化して自己衝突を回避)
        selectedBoxCollider.enabled = false;
        if (Physics.Linecast(transform.position, targetPosition, out RaycastHit hit))
        {
            targetPosition = hit.point;                 // 障害物手前で停止
        }
        selectedBoxCollider.enabled = true;

        // targetPosition確定後にプレイヤーからの範囲制限を適用
        LimitRange();

        // 毎フレームDOKill/DOMoveする代わりにLerpで滑らかに追従(揺れの増幅を防ぐ)
        selectedBox.transform.position = Vector3.Lerp(
            selectedBox.transform.position,
            targetPosition,
            Time.deltaTime * followSpeed
        );
    }

    // Q/E長押しで奥行方向に移動させるメソッド
    private void HandleDepthMove()
    {
        if (!Mouse.current.leftButton.isPressed) return;    // 長押し中のみ有効

        if (Keyboard.current.qKey.isPressed)
        {
            // 奥へ: カメラから箱への距離を増やす
            fixedDistance += depthMoveSpeed * Time.deltaTime;
        }
        else if (Keyboard.current.eKey.isPressed)
        {
            // 手前へ: カメラから箱への距離を減らす
            fixedDistance -= depthMoveSpeed * Time.deltaTime;
            fixedDistance = Mathf.Max(1f, fixedDistance);   // 最小距離1m
        }
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