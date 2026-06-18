using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UniRx;

/// <summary>
/// プレイヤーの重力方向を制御するクラス。
/// Rキーで重力を反転させ、それに合わせてプレイヤーの見た目も回転させる。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GravityController : MonoBehaviour
{
    [Header("重力設定")]
    [SerializeField] private float gravityPower = 9.81f;   // 重力の強さ
    [SerializeField] private float flipDuration = 0.3f;    // 反転アニメーションの所要時間(連打防止の間隔にも使用)

    private Rigidbody rb;                    // Rigidbodyコンポーネントへの参照
    private bool isReverseGravity = false;   // 重力が反転しているかどうかのフラグ

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Unity標準の重力を切り、自前で重力を計算する

        // Rキーの入力を監視するストリームを作成
        Observable.EveryUpdate().Where(_ => Keyboard.current.rKey.wasPressedThisFrame).ThrottleFirst(System.TimeSpan.FromSeconds(flipDuration)).Subscribe(_ => ChangeGravity()).AddTo(this);
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    // 現在の重力方向に応じてプレイヤーに力を加える
    private void ApplyGravity()
    {
        // 反転フラグに応じて重力の向きを決定(反転時は上向き)
        Vector3 gravityDirection = isReverseGravity ? Vector3.up : Vector3.down;

        // 加速度として力を加える(質量に依存させず、常に同じ落下加速度にする)
        rb.AddForce(gravityDirection * gravityPower, ForceMode.Acceleration);
    }

    // 重力の向きを反転させ、プレイヤーの見た目も回転させるメソッド
    private void ChangeGravity()
    {
        // 重力の向きフラグを反転
        isReverseGravity = !isReverseGravity;

        // 現在のZ軸角度から+180度の位置までDOTweenで滑らかに回転させる
        transform.DORotate(
            new Vector3(0f, 0f, transform.eulerAngles.z + 180f), // 目標角度(現在角度+180度)
            flipDuration,                                         // アニメーション時間
            RotateMode.FastBeyond360                              // 360度を超える回転も正しく補間する設定
        ).SetEase(Ease.OutQuad); // 終盤で減速するイージングで自然な動きにする
    }
}