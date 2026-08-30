using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextAnimation : MonoBehaviour
{
    public string[] textFrames;
    public float frameTime = 0.1f;
    private TextMeshProUGUI textMesh;
    private IEnumerator animateTextCoroutine;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        animateTextCoroutine = AnimateText();
    }
    private void Start()
    {
        if (textFrames.Length > 0)
        {
            //StartCoroutine(animateTextCoroutine);
        }
    }

    private IEnumerator AnimateText()
    {
        int frameIndex = 0;
        while (true)
        {
            textMesh.text = textFrames[frameIndex];
            frameIndex = (frameIndex + 1) % textFrames.Length;
            yield return new WaitForSecondsRealtime(frameTime);
        }
    }

    private void OnEnable()
    {
        if (animateTextCoroutine != null)
        {
            StartCoroutine(animateTextCoroutine);
        }
    }
    private void OnDisable()
    {
        if (animateTextCoroutine != null)
        {
            StopCoroutine(animateTextCoroutine);
        }
    }
}