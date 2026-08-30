using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerPhoto;
    public Player player;
    
    public void SetPlayerData(Player player)
    {
        this.player = player;
        playerNameText.text = player.playerName;

        if (player.playerPhoto != null)
            playerPhoto.sprite = player.playerPhoto;
    }
    public Image GetPlayerPhoto()
    {
        return playerPhoto;
    }
}
