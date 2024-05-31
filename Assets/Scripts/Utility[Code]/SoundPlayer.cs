using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOnClick(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayIfPlayerSilent(AudioClip clip)
    {
        if (clip != null && !AlreadyPlaying())
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public bool AlreadyPlaying()
    {
        return audioSource.isPlaying;
    }

    public void PlaySound(AudioClip clip, bool playOneshot = false)
    {
        if (clip != null)
        {
            if (playOneshot)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    public void StopSounds()
    {
        audioSource.Stop();
    }
}
