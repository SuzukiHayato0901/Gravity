using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UniRx;

/// <summary>
/// �v���C���[�̏d�͕����𐧌䂷��N���X�B
/// R�L�[�ŏd�͂𔽓]�����A����ɍ��킹�ăv���C���[�̌����ڂ���]������B
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GravityController : MonoBehaviour
{
    [Header("�d�͐ݒ�")]
    [SerializeField] private float gravityPower = 9.81f;   // �d�͂̋���
    [SerializeField] private float flipDuration = 0.3f;    // ���]�A�j���[�V�����̏��v����(�A�Ŗh�~�̊Ԋu�ɂ��g�p)

    [SerializeField] private PlayerCamera playerCamera;    // �v���C���[�̃J�����ւ̎Q��

    private Rigidbody rb;                    // Rigidbody�R���|�[�l���g�ւ̎Q��
    private bool isReverseGravity = false;   // �d�͂����]���Ă��邩�ǂ����̃t���O
    public bool IsReverseGravity => isReverseGravity;   // �O������Q�Ƃł���v���p�e�B
    private bool isFlipping = false;                    // 
    public bool IsFlipping => isFlipping;               // 

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Unity�W���̏d�͂�؂�A���O�ŏd�͂��v�Z����

        // R�L�[�̓��͂��Ď�����X�g���[�����쐬
        Observable.EveryUpdate().Where(_ => Keyboard.current.rKey.wasPressedThisFrame).ThrottleFirst(System.TimeSpan.FromSeconds(flipDuration)).Subscribe(_ => ChangeGravity()).AddTo(this);
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    // ���݂̏d�͕����ɉ����ăv���C���[�ɗ͂�������
    private void ApplyGravity()
    {
        // ���]�t���O�ɉ����ďd�͂̌���������(���]���͏����)
        Vector3 gravityDirection = isReverseGravity ? Vector3.up : Vector3.down;

        // �����x�Ƃ��ė͂�������(���ʂɈˑ��������A��ɓ������������x�ɂ���)
        rb.AddForce(gravityDirection * gravityPower, ForceMode.Acceleration);
    }

    // �d�͂̌����𔽓]�����A�v���C���[�̌����ڂ���]�����郁�\�b�h
    private void ChangeGravity()
    {
        isFlipping = true;      // �d�͔��]���t���O�𗧂Ă�

        // �d�͂̌����t���O�𔽓]
        isReverseGravity = !isReverseGravity;

        // �J�����ɂ��d�͔��]��ʒm����
        playerCamera.OnGravityChanged(isReverseGravity);

        // ���݂�Z���p�x����+180�x�̈ʒu�܂�DOTween�Ŋ��炩�ɉ�]������
        transform.DORotate(
            new Vector3(0f, 0f, transform.eulerAngles.z + 180f), // �ڕW�p�x(���݊p�x+180�x)
            flipDuration,                                         // �A�j���[�V��������
            RotateMode.FastBeyond360                              // 360�x�𒴂����]����������Ԃ���ݒ�
        ).SetEase(Ease.OutQuad)                                   // �I�ՂŌ�������C�[�W���O�Ŏ��R�ȓ����ɂ���
        .OnComplete(() =>
         {
             // ���]�I��
             isFlipping = false;
         });
    }

    // 重力を初期状態に戻すメソッド
    public void ResetGravity()
    {
        isReverseGravity = false;
        playerCamera.OnGravityChanged(false);

        // Z軸回転を0に戻す
        transform.DORotate(new Vector3(0f, transform.eulerAngles.y, 0f), flipDuration)
        .SetEase(Ease.OutQuad);
    }
}