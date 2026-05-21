using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Vector2 moveInput;      // 移動入力
    private Vector3 lastMoveInput;  // 前回移動方向

    private InputAction moveAction;
    private InputAction jumpAction;
    private Rigidbody rb;
    private Animator animator;

    private bool isGrounded;

    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float rotationDuration = 0.2f;

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

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
        return new Vector3(moveInput.x, 0f, moveInput.y).normalized;
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

        Junp();
    }

    // ジャンプ
    public void Junp()
    {
        if (jumpAction.triggered &&
            isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // 地面判定
    private void OnCollisionEnter(
        Collision collision)
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