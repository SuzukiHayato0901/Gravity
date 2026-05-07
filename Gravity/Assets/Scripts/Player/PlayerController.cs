using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction jumpAction;
    private Rigidbody rb;           // Rigidbodyコンポーネント
    private Animator animator;      // Animatorコンポーネント
    private Vector3 lastMoveInput;  // 前回の移動入力を保存する変数

    private bool isGrounded;        // 地面にいるかどうかのフラグ

    [SerializeField] float moveSpeed = 10f;

    [SerializeField] float jumpForce = 5f;          // ジャンプの力
    [SerializeField] float rotationDuration = 0.2f; // 回転の時間

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

       DOTween.Init(); // DOTweenの初期化
    }

    // 移動は物理演算で行うため、FixedUpdateで処理
    private void FixedUpdate()
    {
        Vector3 moveInput = GetMoveInput();

        ApplyMovement(moveInput);
        ApplyRotation(moveInput);
        UpdateAnimation(moveInput);
    }

    // 入力から移動方向を取得
    private Vector3 GetMoveInput()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        return new Vector3(input.x, 0f, input.y);
    }

    // 移動を適用
    private void ApplyMovement(Vector3 direction)
    {
        // 水平方向の速度を適用
        rb.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            rb.linearVelocity.y,
            direction.z * moveSpeed
        );
    }

    // 回転を適用
    private void ApplyRotation(Vector3 direction)
    {
        // 入力があるかつ、前回と入力方向が変わった時だけ回転
        if (direction.sqrMagnitude > 0.01f && direction != lastMoveInput)
        {
            transform.DOKill(); // 重複命令を防止
            transform.DORotateQuaternion(Quaternion.LookRotation(direction), rotationDuration);
            lastMoveInput = direction;
        }
    }

    // アニメーションを更新
    private void UpdateAnimation(Vector3 direction)
    {
        // 入力が少しでもあればRunをtrue、なければfalse
        animator.SetBool("Run", direction.sqrMagnitude > 0.01f);
    }

    private void Update()
    {
        Junp();
    }

    // ジャンプ処理
    public void Junp()
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

    // 地面から離れたときの判定
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}