using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameIntro : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip footstepsClip, doorKickClip;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void TriggerFootstepSound()
    {
        audioSource.PlayOneShot(footstepsClip);
    }
    public void TriggerDoorKickSound()
    {
        audioSource.PlayOneShot(doorKickClip);
    }
    public void TriggerCameraShake()
    {
        GameMechanics.Instance.ShakeCameras();
    }
}
