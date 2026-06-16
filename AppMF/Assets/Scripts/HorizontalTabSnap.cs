using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class HorizontalTabSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    [Header("Items")]
    [SerializeField] private List<RectTransform> tabItems = new List<RectTransform>();

    [Header("Snap Settings")]
    [SerializeField] private float snapDuration = 0.25f;
    [SerializeField] private float dragThreshold = 20f;
    [SerializeField] private bool centerOnStart = true;
    [SerializeField] private int startIndex = 0;

    private int currentIndex = 0;
    private Vector2 dragStartPos;
    private Coroutine snapCoroutine;
    private bool isDragging = false;

    public int CurrentIndex => currentIndex;

    void Awake()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (viewport == null) viewport = scrollRect.viewport;
        if (content == null) content = scrollRect.content;
    }

    void Start()
    {
        Canvas.ForceUpdateCanvases();

        if (centerOnStart && tabItems.Count > 0)
        {
            currentIndex = Mathf.Clamp(startIndex, 0, tabItems.Count - 1);
            CenterOnItemImmediate(currentIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPos = eventData.position;

        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        Vector2 dragEndPos = eventData.position;
        float deltaX = dragEndPos.x - dragStartPos.x;

        if (Mathf.Abs(deltaX) < dragThreshold)
        {
            SnapToNearest();
            return;
        }

        if (deltaX < 0f)
        {
            // arrastró hacia la izquierda → ir al siguiente
            SnapToIndex(currentIndex + 1);
        }
        else
        {
            // arrastró hacia la derecha → ir al anterior
            SnapToIndex(currentIndex - 1);
        }
    }

    public void SnapToIndex(int index)
    {
        if (tabItems.Count == 0) return;

        index = Mathf.Clamp(index, 0, tabItems.Count - 1);
        currentIndex = index;

        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);

        snapCoroutine = StartCoroutine(SmoothCenterOnItem(tabItems[index]));
    }

    public void SnapToNearest()
    {
        if (tabItems.Count == 0) return;

        float closestDistance = float.MaxValue;
        int closestIndex = currentIndex;

        Vector3 viewportCenterWorld = viewport.TransformPoint(new Vector3(viewport.rect.width * 0.5f, viewport.rect.height * 0.5f, 0f));

        for (int i = 0; i < tabItems.Count; i++)
        {
            Vector3 itemCenterWorld = GetItemCenterWorld(tabItems[i]);
            float distance = Mathf.Abs(itemCenterWorld.x - viewportCenterWorld.x);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        SnapToIndex(closestIndex);
    }

    public void CenterOnItemImmediate(int index)
    {
        index = Mathf.Clamp(index, 0, tabItems.Count - 1);
        currentIndex = index;

        Vector2 targetPos = GetCenteredAnchoredPosition(tabItems[index]);
        content.anchoredPosition = targetPos;
    }

    private IEnumerator SmoothCenterOnItem(RectTransform target)
    {
        Vector2 startPos = content.anchoredPosition;
        Vector2 targetPos = GetCenteredAnchoredPosition(target);

        float elapsed = 0f;

        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / snapDuration);
            content.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        content.anchoredPosition = targetPos;
    }

    private Vector2 GetCenteredAnchoredPosition(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        Vector2 contentPos = content.anchoredPosition;

        Vector3 itemCenterWorld = GetItemCenterWorld(target);
        Vector3 viewportCenterWorld = viewport.TransformPoint(new Vector3(viewport.rect.width * 0.5f, viewport.rect.height * 0.5f, 0f));

        float deltaX = viewportCenterWorld.x - itemCenterWorld.x;
        Vector2 newPos = contentPos + new Vector2(deltaX, 0f);

        float minX = Mathf.Min(0f, viewport.rect.width - content.rect.width);
        float maxX = 0f;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);

        return newPos;
    }

    private Vector3 GetItemCenterWorld(RectTransform item)
    {
        Vector3[] corners = new Vector3[4];
        item.GetWorldCorners(corners);
        return (corners[0] + corners[2]) * 0.5f;
    }

    public void RegisterItems(List<RectTransform> items)
    {
        tabItems = items;
    }

    public void AddItem(RectTransform item)
    {
        if (!tabItems.Contains(item))
            tabItems.Add(item);
    }
}