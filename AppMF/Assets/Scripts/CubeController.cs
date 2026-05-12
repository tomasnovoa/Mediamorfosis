using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CubeController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float dragSensitivity = 0.4f;
    [SerializeField] private float inertiaDamping = 6f;

    [Header("Auto Spin (idle)")]
    [SerializeField] private bool autoSpinEnabled = true;
    [SerializeField] private float autoSpinSpeed = 12f;
    [SerializeField] private float autoSpinDelay = 3f;
    [Header("Constant Spin")]
    [SerializeField] private float spinSpeedX = 0f;    // grados/seg eje X
    [SerializeField] private float spinSpeedY = 15f;   // grados/seg eje Y ← ajusta este
    [SerializeField] private float spinSpeedZ = 0f;    // grados/seg eje Z
    private Vector2 lastInputPos;
    private Vector2 dragVelocity;
    private bool isDragging = false;
    private float idleTimer = 0f;

    private Touchscreen touchscreen;
    private Mouse mouse;

    void OnEnable()
    {
        touchscreen = Touchscreen.current;
        mouse = Mouse.current;
    }

    void Update()
    {
        // ── 1. Spin constante — siempre activo, incluso durante drag ──
        transform.Rotate(
            spinSpeedX * Time.deltaTime,
            spinSpeedY * Time.deltaTime,
            spinSpeedZ * Time.deltaTime,
            Space.World
        );

        // ── 2. Input del usuario ──
        HandleInput();

        // ── 3. Inercia post-drag ──
        if (dragVelocity.magnitude > 0.01f)
        {
            ApplyRotation(dragVelocity);
            dragVelocity = Vector2.Lerp(dragVelocity, Vector2.zero, inertiaDamping * Time.deltaTime);
        }
    }

    private bool HandleInput()
    {
        // ── TOUCH (móvil) ──
        touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.touches.Count > 0)
        {
            var touch = touchscreen.touches[0];
            Vector2 touchPos = touch.position.ReadValue();

            if (IsPointerOverUI(touchPos)) return false;

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                lastInputPos = touchPos;
                isDragging = true;
                dragVelocity = Vector2.zero;
                return true;
            }
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touchPos - lastInputPos;
                dragVelocity = delta * dragSensitivity;
                ApplyRotation(dragVelocity);
                lastInputPos = touchPos;
                return true;
            }
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }

        // ── MOUSE (editor / PC) ──
        mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mousePos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame && !IsPointerOverUI(mousePos))
            {
                lastInputPos = mousePos;
                isDragging = true;
                dragVelocity = Vector2.zero;
            }
            if (mouse.leftButton.isPressed && isDragging)
            {
                Vector2 delta = mousePos - lastInputPos;
                dragVelocity = delta * dragSensitivity;
                ApplyRotation(dragVelocity);
                lastInputPos = mousePos;
                return true;
            }
            if (mouse.leftButton.wasReleasedThisFrame)
                isDragging = false;
        }

        return isDragging;
    }

    private void ApplyRotation(Vector2 delta)
    {
        transform.Rotate(Vector3.up, -delta.x * dragSensitivity, Space.World);
        transform.Rotate(Vector3.right, delta.y * dragSensitivity, Space.World);
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        // Funciona con ambos sistemas de input
        var pointerData = new UnityEngine.EventSystems.PointerEventData(EventSystem.current)
        {
            position = screenPos
        };
        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Count > 0;
    }
}
