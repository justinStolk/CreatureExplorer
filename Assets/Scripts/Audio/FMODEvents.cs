using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("ScrapbookOpen")]
    [field: SerializeField] public EventReference ScrapbookOpenSound { get; private set; }
    //[SerializeField] private EventReference ScrapbookOpenSound;

    public static FMODEvents instance { get; private set;}
 
    private void Awake()
    {
        if (instance != null)
        {
           UnityEngine.Debug.Log("Found more than one FMOD events instance");
        }
        instance = this;
    }
}
