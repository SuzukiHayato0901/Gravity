// GravityBox.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]       // Rigidbodyコンポーネントを必須にする
[RequireComponent(typeof(BoxCollider))]     // BoxColliderコンポーネントを必須にする
[RequireComponent(typeof(Renderer))]        // Rendererコンポーネントを必須にする
public class GravityBox : MonoBehaviour
{
    [Header("発光設定")]
    [SerializeField] private Color emissionColor = Color.cyan;   // 発光色
    [SerializeField] private float emissionIntensity = 2f;       // 発光強度

    private Rigidbody rb;               // Rigidbodyコンポーネントの参照
    private Renderer rd;                // Rendererコンポーネントの参照
    private bool isSelected = false;    // 選択状態を示すフラグ

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rd = GetComponent<Renderer>();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;          // 選択状態を更新
        rb.useGravity = !selected;      // 選択状態に応じて重力を有効/無効化

        if (isSelected)
        {
            rb.linearVelocity = Vector3.zero;   // 速度をリセットして空中静止
            rb.linearDamping = 0f;              // 選択状態のときは減衰を無効化
        }

        SetEmission(isSelected);        // 選択状態に応じて発光を切り替える
    }

    private void SetEmission(bool enable)
    {
        Material mat = rd.material;

        if (enable)
        {
            mat.EnableKeyword("_EMISSION");                                         // 発光を有効化
            mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);      // 発光色と強度を設定
        }
        else
        {
            mat.DisableKeyword("_EMISSION");    // 発光を無効化
        }
    }
}