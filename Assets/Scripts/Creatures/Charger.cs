using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : Prey
{
    [Header("Charger")]
    [SerializeField] private MoodState reactionToOtherChargers;

    protected override void Start()
    {
        base.Start();
        surroundCheck += CheckForChargers;
    }

    protected override void ReactToPlayer(Vector3 playerPos)
    {
        base.ReactToPlayer(playerPos);

        ReactToThreat(playerPos);
    }

    protected override void ReactToPlayerLeaving(Vector3 playerPos)
    {
        base.ReactToPlayerLeaving(playerPos);

        //WaryOff = playerPos;
        SetConditionFalse(worldState, Condition.IsNearDanger);
    }


    /// <summary>
    /// Checks for food in neighbourhood and ups the hunger value with the amount of food nearby
    /// </summary>
    protected void CheckForChargers()
    {
        Charger charger = null;
        int herdCount = LookForObjects<Charger>.CheckForObjects(charger, transform.position, hearingSensitivity).Count;

        UpdateValues(StateType.Fear, reactionToOtherChargers.StateValue * herdCount, StateOperant.Subtract);
    }
}
