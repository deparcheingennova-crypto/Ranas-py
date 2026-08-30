using System.Collections;
using System.Collections.Generic;
using TMPro;
using TotalCreations.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerTeamImageCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum Team
    {
        Team1,
        Team2,
    }

    public Team team;
    public Player player;
    public Image playerPhoto;
    public TextMeshProUGUI playerName;
    private Image dragImage;
    private CanvasGroup canvasGroup;
    private EventSystem eventSystem;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    public Sprite defaultSprite;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        eventSystem = EventSystem.current;
        canvasGroup = GetComponent<CanvasGroup>();

        CreateDragImage();
    }

    public void SetPlayerCardInfo()
    {
        if (player == null)
        {
            playerPhoto.sprite = defaultSprite;
            playerName.text = "Vacio";
            // #1C1C1C Color
            playerName.color = new Color(0.1098039f, 0.1098039f, 0.1098039f);
            return;
        }
        playerPhoto.sprite = player.playerPhoto;
        playerName.text = player.playerName;
    }

    private void CreateDragImage()
    {
        GameObject imageObject = new GameObject("DragImage");
        imageObject.transform.SetParent(transform.root, false); // Attach to root canvas

        dragImage = imageObject.AddComponent<Image>();
        dragImage.rectTransform.sizeDelta = new Vector2(100, 100);
        dragImage.gameObject.SetActive(false); // Hidden initially
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (player == null)
            return;

        // Update pos here
        originalPosition = rectTransform.anchoredPosition;

        // Update image here
        dragImage.sprite = playerPhoto.sprite;

        canvasGroup.alpha = 0.5f;
        JumpInJumpOut jumpInJumpOut = GetComponent<JumpInJumpOut>();
        //if (jumpInJumpOut)
            //jumpInJumpOut.JumpIn();

        canvasGroup.blocksRaycasts = false; // Disable raycasts so we can drag

        // Show drag image and follow the mouse position
        dragImage.gameObject.SetActive(true);
        dragImage.rectTransform.position = Input.mousePosition;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (player == null)
            return;

        // Move the drag image along with the mouse
        dragImage.rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (player == null)
            return;

        canvasGroup.alpha = 1f;
        bool foundTarget = false;
        canvasGroup.blocksRaycasts = true; // Re-enable raycasts after dragging

        // Hide the drag image when done
        dragImage.gameObject.SetActive(false);

        // Perform raycast to check if we're over a PlayerTeamImageCard
        eventSystem.RaycastAll(eventData, raycastResults);

        PlayerTeamImageCard targetCard = null;

        foreach (RaycastResult result in raycastResults)
        {
            targetCard = result.gameObject.GetComponent<PlayerTeamImageCard>();
            if (targetCard != null && targetCard != this.GetComponent<PlayerTeamImageCard>())
            {
                // We found a valid target card to swap players with
                foundTarget = true;
                break;
            }
        }

        if (foundTarget && targetCard != null)
        {
            if (!targetCard.player)
            {
                // Asign player to the empty card
                targetCard.player = player;
                //targetCard.team = team;

                if (targetCard.team == Team.Team1)
                {
                    targetCard.playerName.color = GameManager.team1TextColor;
                    GameManager.Instance.team1.Add(player);
                    GameManager.Instance.team2.Remove(player);
                }
                else
                {
                    targetCard.playerName.color = GameManager.team2TextColor;
                    GameManager.Instance.team2.Add(player);
                    GameManager.Instance.team1.Remove(player);
                }

                targetCard.SetPlayerCardInfo();

                JumpInJumpOut jumpInJumpOut = targetCard.GetComponent<JumpInJumpOut>();
                if (jumpInJumpOut)
                    jumpInJumpOut.JumpIn();

                player = null;
                SetPlayerCardInfo();
            }
            else
            {
                // Swap players between the dragged card and the target card
                SwapPlayers(targetCard);
            }
        }
        else
        {
            // Return the element to its original position if no valid target is found
            rectTransform.anchoredPosition = originalPosition;
        }

        // Force vertical layout update in case the positions changed within a layout
        VerticalLayoutGroup verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();
        if (verticalLayoutGroup != null)
        {
            verticalLayoutGroup.enabled = false;
            LayoutRebuilder.ForceRebuildLayoutImmediate(verticalLayoutGroup.GetComponent<RectTransform>());
            verticalLayoutGroup.enabled = true;
        }

        GameManager.Instance.CheckIfAllPlayersInATeam();

        GameManager.Instance.secondScreenScript.LoadTeamsCard();
    }

    private void SwapPlayers(PlayerTeamImageCard targetCard)
    {
        // Save the target player and team temporarily
        Player tempPlayer = targetCard.player;
        Team tempTeam = targetCard.team;

        // Get the current teams from the GameManager
        List<Player> team1 = GameManager.Instance.team1;
        List<Player> team2 = GameManager.Instance.team2;

        int draggingPlayerIndex = team == Team.Team1 ? team1.IndexOf(player) : team2.IndexOf(player);
        int targetPlayerIndex = tempTeam == Team.Team1 ? team1.IndexOf(tempPlayer) : team2.IndexOf(tempPlayer);

        if (team != targetCard.team)
        {
            if (team == Team.Team1)
            {
                team1[draggingPlayerIndex] = tempPlayer;
                team2[targetPlayerIndex] = player;
                //targetCard.team = Team.Team1;
                //team = Team.Team2;
            }
            else
            {
                team2[draggingPlayerIndex] = tempPlayer;
                team1[targetPlayerIndex] = player;
                //targetCard.team = Team.Team2;
                //team = Team.Team1;
            }
        }
        else
        {
            if (team == Team.Team1)
            {
                team1[draggingPlayerIndex] = tempPlayer;
                team1[targetPlayerIndex] = player;
            }
            else
            {
                team2[draggingPlayerIndex] = tempPlayer;
                team2[targetPlayerIndex] = player;
            }
        }

        // Assign the dragged card's player to the target card
        targetCard.player = player;
        targetCard.SetPlayerCardInfo();

        JumpInJumpOut jumpInJumpOut = targetCard.GetComponent<JumpInJumpOut>();
        if (jumpInJumpOut)
            jumpInJumpOut.JumpIn();

        // Assign the target card's previous player to the dragged card
        player = tempPlayer;
        SetPlayerCardInfo();

        //Debug.Log("Players swapped between teams and updated in GameManager.");
    }

    private void OnDestroy()
    {
        // Clean up the drag image when this object is destroyed
        if (dragImage != null)
            Destroy(dragImage.gameObject);
    }
}
