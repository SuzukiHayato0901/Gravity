using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] float moveSpeed = 10f;         // 移動速度
    [SerializeField] float jumpForce = 5f;          // ジャンプの力
    [SerializeField] float rotationDuration = 0.2f; // 振り向き速度
    [SerializeField]  PlayerCamera playerCamera;     // カメラの参照

    // 入力
    private InputAction moveAction;     // 移動入力アクション
    private InputAction jumpAction;     // ジャンプ入力アクション
    private Vector2 moveInput;          // 移動入力の値
    private Vector3 lastMoveInput;      // 前回の移動入力の値

    // コンポーネント
    private Rigidbody rb;           // リジッドボディコンポーネント
    private Animator animator;      // アニメーターコンポーネント

    // 状態
    private bool isGrounded;    // 地面に接地しているかどうか

    private void Start()
    {
        // 入力アクションの取得
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        // リジッドボディとアニメーターの取得
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        DOTween.Init();
    }

    // 物理移動
    private void FixedUpdate()
    {
        Vector3 direction = GetMoveInput();

        ApplyMovement(direction);
        ApplyRotation(direction);
        UpdateAnimation(direction);
    }

    private Vector3 GetMoveInput()
    {
        // カメラの前方向と右方向を取得
        Vector3 cameraForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = playerCamera.transform.right;

        // 入力にカメラの向きを掛け合わせる
        return (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
    }

    // 移動
    private void ApplyMovement(Vector3 direction)
    {
        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);
    }

    // 回転
    private void ApplyRotation(Vector3 direction)
    {
        // 入力がある時だけ回転
        if (direction.sqrMagnitude > 0.01f)
        {
            // 同じ方向なら回転しない
            if ((direction - lastMoveInput).sqrMagnitude > 0.001f)
            {
                transform.DOKill();

                transform.DORotateQuaternion(Quaternion.LookRotation(direction), rotationDuration);

                lastMoveInput = direction;
            }
        }
    }

    // アニメーション
    private void UpdateAnimation(Vector3 direction)
    {
        animator.SetBool("Run", direction.sqrMagnitude > 0.01f);

        animator.SetBool("Jump", !isGrounded);
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        Jump();
    }

    // ジャンプ
    private void Jump()
    {
        if (jumpAction.triggered && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // 地面判定
    private void OnCollisionEnter(Collision collision)

    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}