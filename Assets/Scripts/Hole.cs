using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hole : MonoBehaviour
{
    [SerializeField] private int score = 20;
    public Transform endHoleStartPosition, endHoleFinalPosition;
    public AudioClip holePointSound;
    public float timeToMove = 0.5f;
    public UnityEvent onScore;
    [SerializeField] private int holeIndex;

    private void OnTriggerEnter(Collider other)
    {
        return;
        if (other.CompareTag("Disc"))
        {
            GameMechanics.Instance.AddScore(score);
            GameMechanics.Instance.NextTurnAfterScore();
            onScore.Invoke();

            ParticleSystem particleSystem;

            if (GameMechanics.Instance.GetCurrentTeam() == Player.Team.Team1)
            {
                particleSystem = Instantiate(GameMechanics.Instance.team1ScoreParticles, transform).GetComponent<ParticleSystem>();
            }
            else
            {
                particleSystem = Instantiate(GameMechanics.Instance.team2ScoreParticles, transform).GetComponent<ParticleSystem>();
            }

            particleSystem.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            Destroy(particleSystem.gameObject, particleSystem.main.startLifetime.constant + 0.1f);
            other.gameObject.SetActive(false);
            MakeHolePointAnimation(other.gameObject);
        }
    }
    public int GetHoleIndex()
    {
        return holeIndex;
    }
    public void ScorePoint(GameObject disc)
    {
        GameMechanics.Instance.AddScore(score);
        GameMechanics.Instance.NextTurnAfterScore();
        onScore.Invoke();

        GameMechanics.Instance.ShakeCameras();
        MakeHolePointAnimation(disc.gameObject);

         // Special treatment for the frog hole
        if (holeIndex == 4)
        {
            GameMechanics.Instance.specialShot = true;
            GameMechanics.Instance.Explosion(transform.position, true);
            return;
        }

        GameObject particleSystem;

        if (GameMechanics.Instance.GetCurrentTeam() == Player.Team.Team1)
        {
            particleSystem = Instantiate(GameMechanics.Instance.team1ScoreParticles);
        }
        else
        {
            particleSystem = Instantiate(GameMechanics.Instance.team2ScoreParticles);
        }

        // The shader inside MK/Toon/URP/Particles/Simple has  inside Input/Albedo a color which can be transparent. I need to get the material and edit thaat color so when
        // The particle spawns, the color is transparent and goes to alphja = 1 using dotween.
        Renderer renderer = particleSystem.GetComponent<Renderer>();
        renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0);
        // Make an animation so the material color tweens between 0 to 1 alpha in .2 seconds
        renderer.material.DOColor(new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, .9f), .35f);
        // Wait 2 seconds and fade color to 0 alpha
        renderer.material.DOColor(new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0), 1.75f).SetDelay(.55f);

        
        //particleSystem.transform.position = new Vector3(transform.position.x, transform.position.y + 4.69f, transform.position.z);
        Vector3 desiredScale = particleSystem.transform.localScale;
        Vector3 desiredPosition = new Vector3(transform.position.x, transform.position.y + 4.69f, transform.position.z);

        // Create a dotween sequence with the particleSystem transform
        particleSystem.transform.position = transform.position;
        particleSystem.transform.localScale = Vector3.zero;

        particleSystem.transform.DOScale(desiredScale, .33f).SetEase(Ease.InOutBack);
        particleSystem.transform.DOMove(desiredPosition, .4f).SetEase(Ease.InOutBack);

        Destroy(particleSystem, 3f);

        // CHANGED If disc is not null, set it to inactive.
        if (disc != null)
            disc.gameObject.SetActive(false);
    }
    public void MakeHolePointAnimation(GameObject objectToMove)
    {
        StartCoroutine(MakeHolePointAnimationCoroutine(objectToMove));
    }
    private IEnumerator MakeHolePointAnimationCoroutine(GameObject objectToMove)
    {
        // CHANGED If object to move is null, exit the coroutine
        if (objectToMove == null)
            yield break;

        objectToMove.SetActive(true);
        GameMechanics.Instance.audioSource.PlayOneShot(holePointSound);
        Vector3 startPos = endHoleStartPosition.position;
        Vector3 endPos = endHoleFinalPosition.position;

        float elapsedTime = 0f;

        while (elapsedTime < timeToMove)
        {
            objectToMove.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / timeToMove);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectToMove.transform.position = endPos;
    }
}
