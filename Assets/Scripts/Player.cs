using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public string playerName;
    public Sprite playerPhoto;
    public int playerID;

    public enum Team
    {
        Team1,
        Team2,
    }
    public Team team;

    // Method to initialize the player
    public void Initialize(string name, Sprite photo, int id)
    {
        playerName = name;
        playerPhoto = photo;
        playerID = id;
    }
}