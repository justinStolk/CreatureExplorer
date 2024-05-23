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

    protected override void ReactToPlayer(Vector3 playerPos, float playerLoudness)
    {
        base.ReactToPlayer(playerPos, playerLoudness);

        ReactToThreat(playerPos, playerLoudness);
    }

    protected override void ReactToPlayerLeaving(Vector3 playerPos)
    {
        base.ReactToPlayerLeaving(playerPos);

        //WaryOff = playerPos;
        worldState = SetConditionFalse(worldState, Condition.IsNearDanger);
    }

    /// <summary>
    /// Checks for chargers in neighbourhood and reduces fear for each charger nearby
    /// </summary>
    protected void CheckForChargers()
    {
        int herdCount = LookForObjects<Charger>.CheckForObjects(transform.position, data.HearingSensitivity).Count;

        UpdateValues(StateType.Fear, reactionToOtherChargers.StateValue * herdCount, StateOperant.Subtract);
    }
}
