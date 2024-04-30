using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManagerTemp : MonoBehaviour
{
   public static AudioManagerTemp instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            UnityEngine.Debug.Log("Found more than one AudioManager in the scene.");

        }
        instance = this;

    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos); 
    }
}
