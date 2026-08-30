using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamemodePage : MonoBehaviour
{
    public GameObject secondScreenPage;

    private void OnEnable()
    {
        if (secondScreenPage != null)
            secondScreenPage.SetActive(true);
    }
    private void OnDisable()
    {
        if (secondScreenPage != null)
            secondScreenPage.SetActive(false);
    }
}
