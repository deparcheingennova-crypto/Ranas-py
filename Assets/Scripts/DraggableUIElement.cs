using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using static PlayerTeamImageCard;
using TotalCreations.UI;

public class DraggableUIElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Image playerPhoto; // The player's photo from PlayerCard
    [SerializeField] private Player player; // The Player information from PlayerCardComponent
    private Image dragImage; // The image that follows the mouse during drag

    private EventSystem eventSystem;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    private int clickCount = 0;
    private float clickTime = 0f;
    private const float doubleClickThreshold = 0.5f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;

        // Get player info and photo from PlayerCardComponent
        PlayerCard playerCard = GetComponent<PlayerCard>();
        playerPhoto = playerCard.GetPlayerPhoto();
        player = playerCard.player;  // Player object contains player details

        eventSystem = EventSystem.current;

        // Create the drag image (copy of the player's photo)
        CreateDragImage();
    }

    public void SetPlayerData(Player player)
    {
        // Set the player's photo and name
        playerPhoto.sprite = player.playerPhoto; // Assuming this method exists
        //playerNameText.text = player.playerName; // Assuming player has a playerName property
        this.player = player;
    }

    private void CreateDragImage()
    {
        GameObject imageObject = new GameObject("DragImage");
        imageObject.transform.SetParent(transform.root, false); // Attach to root canvas

        dragImage = imageObject.AddComponent<Image>();
        dragImage.sprite = playerPhoto.sprite;
        dragImage.rectTransform.sizeDelta = playerPhoto.rectTransform.sizeDelta;
        dragImage.gameObject.SetActive(false); // Hidden initially
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false; // Disable raycasts so we can drag
        canvasGroup.alpha = 0.5f;

        // Show drag image and follow the mouse position
        dragImage.gameObject.SetActive(true);
        dragImage.rectTransform.position = Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the drag image along with the mouse
        dragImage.rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;

        bool foundTarget = false;
        canvasGroup.blocksRaycasts = true; // Re-enable raycasts after dragging

        dragImage.gameObject.SetActive(false);

        // Perform raycast to check if we're over a PlayerTeamImageCard
        eventSystem.RaycastAll(eventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            PlayerTeamImageCard targetCard = result.gameObject.GetComponent<PlayerTeamImageCard>();
            if (targetCard != null)
            {
                if (targetCard.player != null)
                {
                    break;
                }
                AssignPlayerToTeam(targetCard);
                JumpInJumpOut jumpInJumpOut = targetCard.GetComponent<JumpInJumpOut>();
                if (jumpInJumpOut)
                    jumpInJumpOut.JumpIn();
                foundTarget = true;
                break;
            }
        }

        VerticalLayoutGroup verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();

        // This makes the object go to another parent so it no longer affects the vertical layout
        if (foundTarget)
        {
            transform.SetParent(null);
        }
        else
        {
            // Return the element to its original position
            rectTransform.anchoredPosition = originalPosition;
        }

        // This forces the vertical layout to update
        // Thanks to https://stackoverflow.com/questions/60201481/unity-3d-vertical-layout-group-not-placing-elements-where-they-should-be
        verticalLayoutGroup.enabled = false;
        verticalLayoutGroup.CalculateLayoutInputVertical();
        LayoutRebuilder.ForceRebuildLayoutImmediate(verticalLayoutGroup.GetComponent<RectTransform>());
        verticalLayoutGroup.enabled = true;

        GameManager.Instance.CheckIfAllPlayersInATeam();

        // Disable this object so it's no longer draggable and visible
        if (foundTarget)
            gameObject.SetActive(false);

        GameManager.Instance.secondScreenScript.LoadTeamsCard();
    }

    private void AssignPlayerToTeam(PlayerTeamImageCard targetCard)
    {
        if (targetCard.team == PlayerTeamImageCard.Team.Team1)
        {
            targetCard.playerName.color = GameManager.team1TextColor;
            GameManager.Instance.AddPlayerToTeam1(player);
        }
        else if (targetCard.team == PlayerTeamImageCard.Team.Team2)
        {
            targetCard.playerName.color = GameManager.team2TextColor;
            GameManager.Instance.AddPlayerToTeam2(player);
        }

        targetCard.player = player;
        targetCard.SetPlayerCardInfo();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickCount++;

        if (clickCount == 1)
        {
            clickTime = Time.time;
        }
        else if (clickCount == 2 && Time.time - clickTime <= doubleClickThreshold)
        {
            HandleDoubleClick();
            clickCount = 0;
        }
        else if (Time.time - clickTime > doubleClickThreshold)
        {
            clickCount = 0;
        }
    }

    private void HandleDoubleClick()
    {
        PlayerTeamImageCard playerTeamImageCard = GameManager.Instance.GetNextEmptyPlayerTeamImageCard();

        if (playerTeamImageCard == null)
            return;

        playerTeamImageCard.player = player;
        playerTeamImageCard.SetPlayerCardInfo();

        JumpInJumpOut jumpInJumpOut = playerTeamImageCard.GetComponent<JumpInJumpOut>();
        if (jumpInJumpOut)
            jumpInJumpOut.JumpIn();

        if (playerTeamImageCard.team == Team.Team1)
        {
            playerTeamImageCard.playerName.color = GameManager.team1TextColor;
            GameManager.Instance.team1.Add(player);
        }
        else
        {
            playerTeamImageCard.playerName.color = GameManager.team2TextColor;
            GameManager.Instance.team2.Add(player);
        }

        VerticalLayoutGroup verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();
        transform.SetParent(null);

        verticalLayoutGroup.enabled = false;
        verticalLayoutGroup.CalculateLayoutInputVertical();
        LayoutRebuilder.ForceRebuildLayoutImmediate(verticalLayoutGroup.GetComponent<RectTransform>());
        verticalLayoutGroup.enabled = true;

        GameManager.Instance.CheckIfAllPlayersInATeam();
        GameManager.Instance.secondScreenScript.LoadTeamsCard();

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up the drag image when this object is destroyed
        if (dragImage != null)
            Destroy(dragImage.gameObject);
    }
}
