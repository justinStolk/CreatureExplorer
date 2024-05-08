using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    

    [field: Header("CREATURES")]
    [field: Space(10)] 

    [field: Header("-- Creature: CHARGER --")] 
    [field: Space(10)]
    [field: Header("ChargerCall")]
    [field: SerializeField] public EventReference ChargerCall {get; private set;}
    [field: Header("ChargerIdle")]
    [field: SerializeField] public EventReference ChargerIdle {get; private set;}
    [field: Header("ChargerEat")]
    [field: SerializeField] public EventReference ChargerEat {get; private set;}
    [field: Header("ChargerSleep")]
    [field: SerializeField] public EventReference ChargerSleep {get; private set;}
    [field: Header("ChargerDeath")]
    [field: SerializeField] public EventReference ChargerDeath {get; private set;}
    [field: Header("ChargerScared")]
    [field: SerializeField] public EventReference ChargerScared {get; private set;}  
    [field: Space(20)]


    [field: Header("-- Creature: TORCA --")] 
    [field: Space(10)]
    [field: Header("TorcaCall")]
    [field: SerializeField] public EventReference TorcaCall {get; private set;}
    [field: Header("TorcaIdle")]
    [field: SerializeField] public EventReference TorcaIdle {get; private set;}
    [field: Header("TorcaEat")]
    [field: SerializeField] public EventReference TorcaEat {get; private set;}
    [field: Header("TorcaSleep")]
    [field: SerializeField] public EventReference TorcaSleep {get; private set;}
    [field: Header("TorcaDeath")]
    [field: SerializeField] public EventReference TorcaDeath {get; private set;}
    [field: Header("TorcaScared")]
    [field: SerializeField] public EventReference TorcaScared {get; private set;} 
    [field: Header("TorcaGrowl")]
    [field: SerializeField] public EventReference TorcaGrowl {get; private set;} 
    [field: Header("TorcaAttack")]
    [field: SerializeField] public EventReference TorcaAttack {get; private set;} 
    [field: Space(20)]


    [field: Header("-- Creature: STOFFER --")] 
    [field: Space(10)]
    [field: Header("StofferCall")]
    [field: SerializeField] public EventReference StofferCall {get; private set;}
    [field: Header("StofferIdle")]
    [field: SerializeField] public EventReference StofferIdle {get; private set;}
    [field: Header("StofferEat")]
    [field: SerializeField] public EventReference StofferEat {get; private set;}
    [field: Header("StofferSleep")]
    [field: SerializeField] public EventReference StofferSleep {get; private set;}
    [field: Header("StofferDeath")]
    [field: SerializeField] public EventReference StofferDeath {get; private set;}
    [field: Header("StofferScared")]
    [field: SerializeField] public EventReference StofferScared {get; private set;} 
    [field: Space(20)]    


    [field: Header("-- Creature: TIRAMISU --")] 
    [field: Space(10)]
    [field: Header("(NOT DONE)")] 
    [field: Space(20)]

    //UI ELEMENTS
    [field: Header("UI")]
    [field: Space(10)] 

    [field: Header("-- UI: JOURNAL --")] 
    [field: Space(10)]
    [field: Header("JournalClick")]
    [field: SerializeField] public EventReference JournalClick { get; private set; }
    [field: Header("JournalMove")]
    [field: SerializeField] public EventReference JournalMove { get; private set; }  
    [field: Header("JournalWrite")]
    [field: SerializeField] public EventReference JournalWrite { get; private set; }    
    [field: Header("JournalStick")]
    [field: SerializeField] public EventReference JournalStick { get; private set; }    
    [field: Header("JournalPageTurn")]
    [field: SerializeField] public EventReference JournalPageTurn { get; private set; }    
    [field: Header("ScrapbookOpen")]
    [field: SerializeField] public EventReference ScrapbookOpenSound { get; private set; } //to be changed
    [field: Header("ScrapbookClose")]
    [field: SerializeField] public EventReference ScrapbookCloseSound { get; private set; }
    [field: Header("JournalThrow")]
    [field: SerializeField] public EventReference JournalThrow { get; private set; }  //delete photo
    [field: Space(20)]


    [field: Header("-- UI: Camera --")] 
    [field: Space(10)]
    [field: Header("CameraTakePicture")]
    [field: SerializeField] public EventReference CameraTakePicture { get; private set; }
    [field: Header("CameraZoomIn")]
    [field: SerializeField] public EventReference CameraZoomIn { get; private set; }
    [field: Header("CameraZoomOut")]
    [field: SerializeField] public EventReference CameraZoomOut { get; private set; }
    [field: Header("CameraStorageFull")]
    [field: SerializeField] public EventReference CameraStorageFull { get; private set; }
    [field: Header("CameraEquip")]
    [field: SerializeField] public EventReference CameraEquip { get; private set; }
    [field: Header("CameraUnEquip")]
    [field: SerializeField] public EventReference CameraUnEquip { get; private set; }
    [field: Space(20)]


    [field: Header("-- UI: MainMenu --")] 
    [field: Space(10)]
    [field: Header("MenuHover")]
    [field: SerializeField] public EventReference MenuHover { get; private set; }
    [field: Header("MenuClick")]
    [field: SerializeField] public EventReference MenuClick { get; private set; }




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
