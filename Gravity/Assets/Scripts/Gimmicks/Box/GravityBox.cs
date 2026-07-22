// GravityBox.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]       // Rigidbody�R���|�[�l���g��K�{�ɂ���
[RequireComponent(typeof(BoxCollider))]     // BoxCollider�R���|�[�l���g��K�{�ɂ���
[RequireComponent(typeof(Renderer))]        // Renderer�R���|�[�l���g��K�{�ɂ���
public class GravityBox : MonoBehaviour
{
    [Header("�����ݒ�")]
    [SerializeField] private Color emissionColor = Color.cyan;   // �����F
    [SerializeField] private float emissionIntensity = 2f;       // �������x

    private Rigidbody rb;               // Rigidbody�R���|�[�l���g�̎Q��
    private Renderer rd;                // Renderer�R���|�[�l���g�̎Q��
    private bool isSelected = false;    // �I����Ԃ������t���O

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rd = GetComponent<Renderer>();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;          // �I����Ԃ��X�V
        rb.useGravity = !selected;      // �I����Ԃɉ����ďd�͂�L��/������

        if (isSelected)
        {
            rb.linearVelocity = Vector3.zero;   // ���x�����Z�b�g���ċ󒆐Î~
            rb.linearDamping = 0f;              // �I����Ԃ̂Ƃ��͌����𖳌���
        }

        SetEmission(isSelected);        // �I����Ԃɉ����Ĕ�����؂�ւ���
    }

    private void SetEmission(bool enable)
    {
        Material mat = rd.material;

        if (enable)
        {
            mat.EnableKeyword("_EMISSION");                                         // ������L����
            mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);      // �����F�Ƌ��x��ݒ�
        }
        else
        {
            mat.DisableKeyword("_EMISSION");    // �����𖳌���
        }
    }

    // GravityBoxの状態をリセットするメソッド
    public void ResetState()
    {
        // 選択状態を解除
        SetSelected(false);

        // 重力を有効化して通常状態に戻す
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}