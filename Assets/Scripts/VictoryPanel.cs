using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VictoryPanel : MonoBehaviour
{
    public GameObject team1VictoryPanel, team2VictoryPanel, tiePanel;
    public TextMeshProUGUI timerText;

    private void Start()
    {
        team1VictoryPanel.SetActive(false);
        team2VictoryPanel.SetActive(false);
        tiePanel.SetActive(false);
    }
}
