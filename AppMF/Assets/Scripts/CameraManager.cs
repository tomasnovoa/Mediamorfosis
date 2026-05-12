using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Maneja los giros orbitales de la cámara alrededor del cubo.
/// Cada transición entre pantallas ejecuta una función de giro
/// predefinida (eje, grados, velocidad).
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Orbit Target")]
    [SerializeField] private Transform orbitTarget;   // El cubo

    [Header("Orbit Settings")]
    [SerializeField] private float orbitRadius = 5f;
    [SerializeField] private float defaultDuration = 1.2f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Ángulos actuales de órbita
    private float currentYaw = 0f;    // rotación horizontal
    private float currentPitch = 15f; // rotación vertical

    private bool isAnimating = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (orbitTarget == null)
            orbitTarget = GameObject.FindWithTag("Cube")?.transform;

        ApplyOrbitPosition(currentYaw, currentPitch);
    }

    // ─────────────────────────────────────────
    //  FUNCIONES DE GIRO PREDEFINIDAS
    //  Úsalas como eventos de botón o llama a PlayTransitionFor
    // ─────────────────────────────────────────

    /// <summary>Gira 90° en el eje Y (derecha)</summary>
    public void OrbitRight90() => StartOrbit(90f, 0f, defaultDuration);

    /// <summary>Gira -90° en el eje Y (izquierda)</summary>
    public void OrbitLeft90() => StartOrbit(-90f, 0f, defaultDuration);

    /// <summary>Gira 180° en el eje Y</summary>
    public void OrbitBack180() => StartOrbit(180f, 0f, defaultDuration);

    /// <summary>Gira 360° completo horizontal</summary>
    public void OrbitFull360() => StartOrbit(360f, 0f, 2.0f);

    /// <summary>Gira 45° hacia arriba (eje X)</summary>
    public void OrbitUp45() => StartOrbit(0f, 45f, defaultDuration);

    /// <summary>Gira -45° hacia abajo (eje X)</summary>
    public void OrbitDown45() => StartOrbit(0f, -45f, defaultDuration);

    /// <summary>Gira 30° diagonal (X e Y)</summary>
    public void OrbitDiagonal() => StartOrbit(30f, 30f, defaultDuration);

    // ─────────────────────────────────────────
    //  MAPA DE TRANSICIONES POR PANTALLA
    // ─────────────────────────────────────────

    /// <summary>
    /// Selecciona automáticamente el giro según la pantalla de destino.
    /// Llamado por UIManager en cada cambio de pantalla.
    /// </summary>
    public void PlayTransitionFor(string fromScreen, string toScreen)
    {
        switch (toScreen)
        {
            case "MainMenu": OrbitLeft90(); break;
            case "Programacion": OrbitRight90(); break;
            case "Registro": OrbitUp45(); break;
            default: OrbitRight90(); break;
        }
    }

    // ─────────────────────────────────────────
    //  MOTOR DE ANIMACIÓN DE ÓRBITA
    // ─────────────────────────────────────────

    private void StartOrbit(float deltaDegYaw, float deltaDegPitch, float duration)
    {
        if (isAnimating) StopAllCoroutines();
        StartCoroutine(AnimateOrbit(deltaDegYaw, deltaDegPitch, duration));
    }

    private IEnumerator AnimateOrbit(float deltaYaw, float deltaPitch, float duration)
    {
        isAnimating = true;
        float startYaw = currentYaw;
        float startPitch = currentPitch;
        float targetYaw = currentYaw + deltaYaw;
        float targetPitch = Mathf.Clamp(currentPitch + deltaPitch, -80f, 80f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = easeCurve.Evaluate(elapsed / duration);
            float yaw = Mathf.Lerp(startYaw, targetYaw, t);
            float pitch = Mathf.Lerp(startPitch, targetPitch, t);
            ApplyOrbitPosition(yaw, pitch);
            yield return null;
        }

        currentYaw = targetYaw;
        currentPitch = targetPitch;
        ApplyOrbitPosition(currentYaw, currentPitch);
        isAnimating = false;
    }

    private void ApplyOrbitPosition(float yaw, float pitch)
    {
        if (orbitTarget == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -orbitRadius);
        transform.position = orbitTarget.position + offset;
        transform.LookAt(orbitTarget.position);
    }
}
