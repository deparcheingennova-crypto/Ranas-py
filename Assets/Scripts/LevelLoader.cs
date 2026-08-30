using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;
    public void LoadNextLevel(bool loadScene)
    {
        GameManager.Instance.isOnMainMenu = false;
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1, loadScene));
    }

    private IEnumerator LoadLevel(int levelIndex, bool loadScene)
    {
        transition.SetTrigger("Start");
        if (!loadScene)
            yield break;

        yield return new WaitForSeconds(transitionTime);
        GameManager.Instance.SetPlayersAsChildren();
        SceneManager.LoadScene(levelIndex);
    }
}
