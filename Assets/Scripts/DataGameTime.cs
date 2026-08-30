using UnityEngine;

[CreateAssetMenu(menuName = "Game/GameData")]
public class DataGameTime : ScriptableObject
{
    public int timeCurrent;
    public int timeOfGame;
    public int idApi;
    public bool isDataReady = false;
    public bool isTimeOver = false;

    private void OnEnable()
    {
        //timeCurrent = 0;
        isDataReady = false;
        isTimeOver = false;
    }
}
