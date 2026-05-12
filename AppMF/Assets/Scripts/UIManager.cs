using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona el flujo entre pantallas de la app Mediamorfosis.
/// Cada pantalla es un Canvas Group. Al cambiar de pantalla,
/// lanza la transición de UI y el giro de cámara correspondiente.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [System.Serializable]
    public class Screen
    {
        public string screenName;
        public CanvasGroup canvasGroup;
        [HideInInspector] public bool isActive;
    }

    [Header("Screens")]
    public List<Screen> screens = new List<Screen>();

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.35f;

    private Screen currentScreen;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Inicializar: ocultar todas excepto la primera
        foreach (var s in screens)
        {
            SetCanvasGroupState(s.canvasGroup, false);
        }

        if (screens.Count > 0)
            ShowScreen(screens[0].screenName, false);
    }

    /// <summary>
    /// Navega a una pantalla por nombre.
    /// triggerCameraAnim: si debe disparar giro de cámara.
    /// </summary>
    public void ShowScreen(string screenName, bool triggerCameraAnim = true)
    {
        Screen target = screens.Find(s => s.screenName == screenName);
        if (target == null)
        {
            Debug.LogWarning($"[UIManager] Pantalla '{screenName}' no encontrada.");
            return;
        }

        if (currentScreen != null)
            StartCoroutine(TransitionScreens(currentScreen, target, triggerCameraAnim));
        else
        {
            SetCanvasGroupState(target.canvasGroup, true);
            currentScreen = target;
        }
    }

    private IEnumerator TransitionScreens(Screen from, Screen to, bool doCamera)
    {
        // Fade out pantalla actual
        yield return StartCoroutine(FadeCanvasGroup(from.canvasGroup, 1f, 0f, fadeDuration));
        SetCanvasGroupState(from.canvasGroup, false);

        // Disparar animación de cámara (no bloqueante, corre en paralelo)
        if (doCamera && CameraManager.Instance != null)
            CameraManager.Instance.PlayTransitionFor(from.screenName, to.screenName);

        // Fade in nueva pantalla
        SetCanvasGroupState(to.canvasGroup, true);
        yield return StartCoroutine(FadeCanvasGroup(to.canvasGroup, 0f, 1f, fadeDuration));

        currentScreen = to;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private void SetCanvasGroupState(CanvasGroup cg, bool active)
    {
        cg.alpha = active ? 1f : 0f;
        cg.interactable = active;
        cg.blocksRaycasts = active;
    }

    // ── Métodos públicos para asignar a los botones en el Inspector ──

    public void GoToMainMenu() => ShowScreen("MainMenu");
    public void GoToProgramacion() => ShowScreen("Programacion");
    public void GoToRegistro() => ShowScreen("Registro");
}
