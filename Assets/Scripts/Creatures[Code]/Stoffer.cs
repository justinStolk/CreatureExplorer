using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stoffer : Prey
{
    protected override void Start()
    {
        base.Start();
        surroundCheck += CheckForThreats;
    }

    /// <summary>
    /// Checks for chargers in neighbourhood and makes stoffer wary of them
    /// </summary>
    protected void CheckForThreats()
    {
        Charger threat = null;

        if (LookForObjects<Charger>.TryGetClosestObject(transform.position, data.HearingSensitivity * CurrentAction.Awareness, out threat))
        {
            ReactToThreat(threat.transform.position, reactionToDanger);
        }
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
}
