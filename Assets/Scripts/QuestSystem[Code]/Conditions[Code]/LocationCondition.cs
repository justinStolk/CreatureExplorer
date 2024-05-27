using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocationCondition", menuName = "Titan Quests/New Location Condition")]
public class LocationCondition : QuestCondition
{
    [SerializeField] private Vector3 locationCenter;
    [SerializeField] private float acceptableRadius;
    
    public override bool Evaluate(PictureInfo pictureInfo)
    { 
        if ((locationCenter-pictureInfo.PictureLocation).magnitude<acceptableRadius)
        {
            return true;
        }
        return false;
    }
}
