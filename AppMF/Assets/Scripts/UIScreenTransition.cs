using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Aplica una animación de entrada (slide + fade) a los elementos
/// de una pantalla cuando se activa. Asigna este script al root
/// de cada Canvas Group de pantalla.
/// </summary>
public class UIScreenTransition : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float slideDistance = 40f;      // px desde abajo
    [SerializeField] private float slideDuration = 0.45f;
    [SerializeField]
    private AnimationCurve slideCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rt;
    private Vector2 originalPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        originalPos = rt.anchoredPosition;
    }

    /// <summary>Llama esto desde UIManager al activar la pantalla.</summary>
    public void PlayEnterAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(SlideIn());
    }

    private IEnumerator SlideIn()
    {
        Vector2 startPos = originalPos + Vector2.down * slideDistance;
        rt.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = slideCurve.Evaluate(elapsed / slideDuration);
            rt.anchoredPosition = Vector2.Lerp(startPos, originalPos, t);
            yield return null;
        }
        rt.anchoredPosition = originalPos;
    }
}
