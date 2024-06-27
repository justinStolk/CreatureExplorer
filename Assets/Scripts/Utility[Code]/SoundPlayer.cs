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
        if (clip != null && this.enabled)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayIfPlayerSilent(AudioClip clip)
    {
        if (clip != null && !AlreadyPlaying() && this.enabled)
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
        if (clip != null && this.enabled)
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

    public void PlayRandomFromArray(AudioClip[] clips, float duration)
    {
        if (clips.Length > 0)
        {
            StartCoroutine(PlayRandom(clips, duration));
        }
    }

    public void StopSounds()
    {
        audioSource.Stop();
    }

    private IEnumerator PlayRandom(AudioClip[] clips, float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            if (audioSource.isPlaying) 
            {
                timer += Time.deltaTime;
                yield return null;
            } else
            {
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }

        }

    }
}
