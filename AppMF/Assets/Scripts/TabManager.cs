using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona tabs con contenido dinámico dentro de un panel.
/// No cambia de pantalla — solo hace fade entre contenidos.
/// Completamente genérico: arrástralo a cualquier panel con tabs.
/// </summary>
public class TabManager : MonoBehaviour
{
    // ─── Configuración de un Tab ───────────────────────────────────
    [System.Serializable]
    public class Tab
    {
        public string tabName;
        public Button tabButton;
        public CanvasGroup contentGroup;   // el contenido que muestra este tab
    }

    [Header("Tabs")]
    [SerializeField] private List<Tab> tabs = new List<Tab>();
    [SerializeField] private int defaultTabIndex = 0;

    [Header("Animación")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Estilo Visual")]
    [SerializeField] private Color activeTabTextColor = Color.white;
    [SerializeField] private Color inactiveTabTextColor = new Color(0.55f, 0.55f, 0.55f, 1f); // #8C8C8C
    [SerializeField] private Color activeTabBgColor = Color.white;
    [SerializeField] private Color inactiveTabBgColor = new Color(1f, 1f, 1f, 0.06f);

    private int currentTabIndex = -1;
    private Coroutine transitionCoroutine;

    // ── Opcional: evento para que otros scripts reaccionen al cambio de tab ──
    public System.Action<int> OnTabChanged;

    void Start()
    {
        // Registrar listeners en cada botón
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i; // captura local para el closure
            tabs[i].tabButton.onClick.AddListener(() => SelectTab(index));
        }

        // Ocultar todos los contenidos primero
        foreach (var tab in tabs)
            SetCanvasGroupState(tab.contentGroup, false);

        // Activar tab por defecto sin animación
        SelectTab(defaultTabIndex, animate: false);
    }

    /// <summary>
    /// Selecciona un tab por índice.
    /// Puede llamarse desde botones en el Inspector o desde código.
    /// </summary>
    public void SelectTab(int index, bool animate = true)
    {
        if (index == currentTabIndex || index < 0 || index >= tabs.Count) return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(
            TransitionTab(currentTabIndex, index, animate)
        );
    }

    // ── Métodos de conveniencia por nombre ──
    public void SelectTab(string name)
    {
        int index = tabs.FindIndex(t => t.tabName == name);
        if (index >= 0) SelectTab(index);
    }

    public void SelectNextTab()
    {
        int next = (currentTabIndex + 1) % tabs.Count;
        SelectTab(next);
    }

    public void SelectPrevTab()
    {
        int prev = (currentTabIndex - 1 + tabs.Count) % tabs.Count;
        SelectTab(prev);
    }

    // ─────────────────────────────────────────
    //  TRANSICIÓN
    // ─────────────────────────────────────────

    private IEnumerator TransitionTab(int fromIndex, int toIndex, bool animate)
    {
        // 1. Fade out del contenido actual
        if (fromIndex >= 0 && animate)
        {
            var fromContent = tabs[fromIndex].contentGroup;
            yield return StartCoroutine(
                FadeCanvasGroup(fromContent, 1f, 0f, fadeDuration)
            );
            SetCanvasGroupState(fromContent, false);
        }
        else if (fromIndex >= 0)
        {
            SetCanvasGroupState(tabs[fromIndex].contentGroup, false);
        }

        // 2. Actualizar estilos visuales de los botones
        UpdateTabStyles(toIndex);

        // 3. Fade in del nuevo contenido
        SetCanvasGroupState(tabs[toIndex].contentGroup, true);

        if (animate)
        {
            // Pequeño slide-up sutil al aparecer
            RectTransform rt = tabs[toIndex].contentGroup.GetComponent<RectTransform>();
            if (rt != null)
                yield return StartCoroutine(SlideAndFadeIn(tabs[toIndex].contentGroup, rt));
            else
                yield return StartCoroutine(
                    FadeCanvasGroup(tabs[toIndex].contentGroup, 0f, 1f, fadeDuration)
                );
        }
        else
        {
            tabs[toIndex].contentGroup.alpha = 1f;
        }

        currentTabIndex = toIndex;
        OnTabChanged?.Invoke(toIndex);
    }

    // ─────────────────────────────────────────
    //  ANIMACIONES
    // ─────────────────────────────────────────

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

    /// <summary>
    /// Fade + leve slide desde abajo (8px) — coherente con UIScreenTransition
    /// </summary>
    private IEnumerator SlideAndFadeIn(CanvasGroup cg, RectTransform rt)
    {
        float slideOffset = 8f;
        Vector2 startPos = rt.anchoredPosition + Vector2.down * slideOffset;
        Vector2 endPos = rt.anchoredPosition;

        float elapsed = 0f;
        cg.alpha = 0f;
        rt.anchoredPosition = startPos;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);
            cg.alpha = t;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        cg.alpha = 1f;
        rt.anchoredPosition = endPos;
    }

    // ─────────────────────────────────────────
    //  ESTILOS VISUALES DE LOS BOTONES
    // ─────────────────────────────────────────

    private void UpdateTabStyles(int activeIndex)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            bool isActive = i == activeIndex;
            Button btn = tabs[i].tabButton;

            // Color de fondo del botón
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = isActive ? activeTabBgColor : inactiveTabBgColor;

            // Color del texto
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.color = isActive ? activeTabTextColor : inactiveTabTextColor;
        }
    }

    // ─────────────────────────────────────────
    //  UTILIDADES
    // ─────────────────────────────────────────

    private void SetCanvasGroupState(CanvasGroup cg, bool active)
    {
        cg.alpha = active ? 1f : 0f;
        cg.interactable = active;
        cg.blocksRaycasts = active;
    }

    public int GetCurrentTabIndex() => currentTabIndex;
    public string GetCurrentTabName() => currentTabIndex >= 0 ? tabs[currentTabIndex].tabName : "";
}