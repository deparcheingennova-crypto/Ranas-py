using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public Button button;

    public void Initialize(string text)
    {
        textMesh.text = text;
    }
    public void DestroyPopup()
    {
        Destroy(gameObject);
    }
}
