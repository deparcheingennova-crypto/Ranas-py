using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondScreen : MonoBehaviour
{
    public GameObject playerCardPrefab, SCplayerPrefabCard;
    public Transform contentTransform, team1Transform, team2Transform;
    public GameObject asignationTxt1, asignationTxt2;
    public void LoadPlayersCard()
    {
        GameManager.DestroyChildren(contentTransform);
        GridLayoutGroup gridLayoutGroup = contentTransform.GetComponent<GridLayoutGroup>();
        switch (GameManager.Instance.players.Count)
        {
            case 2:
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = 1;
                gridLayoutGroup.cellSize = new Vector2(720, 180);
                gridLayoutGroup.spacing = new Vector2(100, 100);
                break;
            case 4:
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = 2;
                gridLayoutGroup.cellSize = new Vector2(700, 180);
                gridLayoutGroup.spacing = new Vector2(100, 30);
                break;
            case 6:
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = 2;
                gridLayoutGroup.cellSize = new Vector2(700, 180);
                gridLayoutGroup.spacing = new Vector2(100, 0);
                break;
            case 12:
                gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayoutGroup.constraintCount = 3;
                gridLayoutGroup.cellSize = new Vector2(550, 170);
                gridLayoutGroup.spacing = new Vector2(10, 0);
                break;
            default:
                break;
        }

        foreach (Player player in GameManager.Instance.players)
        {
            PlayerCard playerCard = Instantiate(playerCardPrefab, contentTransform).GetComponent<PlayerCard>();
            playerCard.SetPlayerData(player);
        }
    }
    public void LoadTeamsCard()
    {
        GameManager.DestroyChildren(team1Transform); GameManager.DestroyChildren(team2Transform);
        foreach (Player player in GameManager.Instance.team1)
        {
            PlayerCard playerCard = Instantiate(SCplayerPrefabCard, team1Transform).GetComponent<PlayerCard>();
            playerCard.SetPlayerData(player);
        }
        foreach (Player player in GameManager.Instance.team2)
        {
            PlayerCard playerCard = Instantiate(SCplayerPrefabCard, team2Transform).GetComponent<PlayerCard>();
            playerCard.SetPlayerData(player);
        }

        if (team1Transform.childCount == 0)
            asignationTxt1.SetActive(true);
        else
            asignationTxt1.SetActive(false);

        if (team2Transform.childCount == 0)
            asignationTxt2.SetActive(true);
        else
            asignationTxt2.SetActive(false);
    }
}
