// GravityBoxSelector_B.cs (B案: 常にカメラが追従)
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class GravityBoxSelector_B : MonoBehaviour
{
    [Header("選択設定")]
    [SerializeField] private float selectRange = 5f;
    [SerializeField] private PlayerCamera playerCamera;

    [Header("移動設定")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float depthMoveSpeed = 3f;

    private enum State { None, Selected, Controlled }
    private State currentState = State.None;

    private GravityBox selectedBox;
    private Vector3 targetPosition;
    private Camera cam;
    private float fixedDistance;

    private void Start()
    {
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
                LimitRange();
                break;
        }
    }

    private void HandleFKey()
    {
        if (!Keyboard.current.fKey.wasPressedThisFrame) return;

        if (currentState == State.None)
        {
            GravityBox nearestBox = FindNearestBox();
            if (nearestBox != null)
            {
                selectedBox = nearestBox;
                selectedBox.SetSelected(true);
                currentState = State.Selected;

                playerCamera.SetCameraEnabled(false);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                playerCamera.TrackBoxPosition(selectedBox.transform.position);
            }
        }
        else
        {
            Deselect();
        }
    }

    private void HandleLeftClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        targetPosition = selectedBox.transform.position;
        fixedDistance = Vector3.Distance(cam.transform.position, selectedBox.transform.position);

        currentState = State.Controlled;
    }

    private void Deselect()
    {
        if (selectedBox != null)
        {
            selectedBox.SetSelected(false);
        }
        selectedBox = null;
        currentState = State.None;

        playerCamera.SetCameraEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private GravityBox FindNearestBox()
    {
        GravityBox nearest = null;
        float nearestDistance = selectRange;

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

    private void HandleMove()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            currentState = State.Selected;
            return;
        }

        if (!Mouse.current.leftButton.isPressed) return;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = fixedDistance;
        Vector3 worldPos = cam.ScreenToWorldPoint(mouseScreenPos);

        targetPosition = worldPos;

        // B案: 箱が動くたびにカメラを反対側に追従させる
        playerCamera.TrackBoxPosition(targetPosition);

        if (Physics.Linecast(transform.position, targetPosition, out RaycastHit hit))
        {
            targetPosition = hit.point;
        }

        selectedBox.transform.DOMove(targetPosition, 1f / followSpeed)
            .SetEase(Ease.OutQuad);
    }

    private void HandleDepthMove()
    {
        if (!Mouse.current.leftButton.isPressed) return;

        if (Keyboard.current.qKey.isPressed)
        {
            fixedDistance += depthMoveSpeed * Time.deltaTime;
        }
        else if (Keyboard.current.eKey.isPressed)
        {
            fixedDistance -= depthMoveSpeed * Time.deltaTime;
            fixedDistance = Mathf.Max(1f, fixedDistance);
        }

        // 奥行き変更時もカメラを追従させる
        playerCamera.TrackBoxPosition(targetPosition);
    }

    private void LimitRange()
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > selectRange)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            targetPosition = transform.position + direction * selectRange;
        }
    }
}