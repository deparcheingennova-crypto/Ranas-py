using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamemodeCard : MonoBehaviour
{
    public RectTransform infoPanelRectTransform;
    public Transform contentTransform;
    public Button nextPageButton, lastPageButton;
    private int currentPage = 0;
    private float originalWidth;

    public float animationSpeed = 4500f;

    private bool canTrigger = false;
    private bool isOpen = false;

    void Start()
    {
        originalWidth = infoPanelRectTransform.sizeDelta.x;

        // Info panel starts closed
        infoPanelRectTransform.sizeDelta = new Vector2(0f, infoPanelRectTransform.sizeDelta.y);
        infoPanelRectTransform.anchoredPosition3D = new Vector3(0, infoPanelRectTransform.anchoredPosition3D.y, infoPanelRectTransform.anchoredPosition3D.z);

        canTrigger = true;
        isOpen = false;

        for (int i = 0; i < contentTransform.childCount; i++)
        {
            contentTransform.GetChild(i).gameObject.SetActive(i == 0);
        }
        UpdatePageButtons();
    }

    private void UpdatePageButtons()
    {
        nextPageButton.interactable = currentPage < contentTransform.childCount - 1;
        lastPageButton.interactable = currentPage > 0;
    }
    public void GoToNextPage()
    {
        if (currentPage < contentTransform.childCount - 1)
        {
            contentTransform.GetChild(currentPage).gameObject.SetActive(false);  // Hide current page
            currentPage++;
            contentTransform.GetChild(currentPage).gameObject.SetActive(true);   // Show next page
            UpdatePageButtons();
        }
    }

    public void GoToPreviousPage()
    {
        if (currentPage > 0)
        {
            contentTransform.GetChild(currentPage).gameObject.SetActive(false);  // Hide current page
            currentPage--;
            contentTransform.GetChild(currentPage).gameObject.SetActive(true);   // Show previous page
            UpdatePageButtons();
        }
    }


    public void TriggerAnimation()
    {
        if (!canTrigger)
            return;

        if (isOpen)
        {
            CloseInfoPanel();
        }
        else
        {
            OpenInfoPanel();
        }
    }

    public void OpenInfoPanel()
    {
        if (!canTrigger)
            return;

        StartCoroutine(OpenInfoPanelCoroutine());
    }

    public void CloseInfoPanel()
    {
        if (!canTrigger)
            return;

        StartCoroutine(CloseInfoPanelCoroutine());
    }

    private IEnumerator OpenInfoPanelCoroutine()
    {
        Transform cardParent = transform.parent;

        // Initially set alpha for each child, fade out all except the opening panel
        for (int i = 0; i < cardParent.childCount; i++)
        {
            Transform currentChildTransform = cardParent.GetChild(i);
            CanvasGroup currentCanvasGroup = currentChildTransform.GetComponent<CanvasGroup>();
            if (cardParent.GetChild(i) == transform)
            {
                currentCanvasGroup.alpha = 1f;
                currentCanvasGroup.interactable = true; currentCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                currentCanvasGroup.alpha = 1f;
                currentCanvasGroup.interactable = false; currentCanvasGroup.blocksRaycasts = false;
            }
        }

        canTrigger = false;
        float elapsedTime = 0f;
        float duration = originalWidth / animationSpeed;

        while (infoPanelRectTransform.sizeDelta.x < originalWidth)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Apply easing with an ease-out curve
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);

            // Calculate width and position
            float newWidth = Mathf.Lerp(0, originalWidth, easedT);
            infoPanelRectTransform.sizeDelta = new Vector2(newWidth, infoPanelRectTransform.sizeDelta.y);
            infoPanelRectTransform.anchoredPosition3D = new Vector3(newWidth / 2, infoPanelRectTransform.anchoredPosition3D.y, infoPanelRectTransform.anchoredPosition3D.z);

            // Fade out other children
            for (int i = 0; i < cardParent.childCount; i++)
            {
                Transform currentChildTransform = cardParent.GetChild(i);
                CanvasGroup currentCanvasGroup = currentChildTransform.GetComponent<CanvasGroup>();

                if (currentChildTransform != transform)
                {
                    currentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, easedT);  // Fade out
                }
            }

            yield return null;
        }

        canTrigger = true;
        isOpen = true;
    }

    private IEnumerator CloseInfoPanelCoroutine()
    {
        Transform cardParent = transform.parent;

        canTrigger = false;
        float elapsedTime = 0f;
        float duration = originalWidth / animationSpeed;

        while (infoPanelRectTransform.sizeDelta.x > 0)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Apply easing with an ease-out curve
            float easedT = Mathf.Sin((1 - t) * Mathf.PI * 0.5f);

            // Calculate width and position
            float newWidth = Mathf.Lerp(0, originalWidth, easedT);
            infoPanelRectTransform.sizeDelta = new Vector2(newWidth, infoPanelRectTransform.sizeDelta.y);
            infoPanelRectTransform.anchoredPosition3D = new Vector3(newWidth / 2, infoPanelRectTransform.anchoredPosition3D.y, infoPanelRectTransform.anchoredPosition3D.z);

            // Gradually fade in other children
            for (int i = 0; i < cardParent.childCount; i++)
            {
                Transform currentChildTransform = cardParent.GetChild(i);
                CanvasGroup currentCanvasGroup = currentChildTransform.GetComponent<CanvasGroup>();

                if (currentChildTransform != transform)
                {
                    // Fade in smoothly using the easedT
                    currentCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                }
            }

            yield return null;
        }

        // Set final alpha to prevent any visibility glitches
        for (int i = 0; i < cardParent.childCount; i++)
        {
            Transform currentChildTransform = cardParent.GetChild(i);
            CanvasGroup currentCanvasGroup = currentChildTransform.GetComponent<CanvasGroup>();
            
            currentCanvasGroup.interactable = true; currentCanvasGroup.blocksRaycasts = true;
            if (currentChildTransform != transform)
            {
                currentCanvasGroup.alpha = 1f;  // Ensure all panels are fully visible
            }
        }

        canTrigger = true;
        isOpen = false;
    }


}
