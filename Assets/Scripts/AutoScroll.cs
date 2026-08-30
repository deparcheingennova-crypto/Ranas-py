using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 1f;
    public float bottomThreshold = 0.01f;

    private bool isPointerOver = false;

    void Update()
    {
        if (!isPointerOver)
        {
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;

            if (scrollRect.verticalNormalizedPosition <= bottomThreshold)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }

    // Handle pointer events
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }

}
