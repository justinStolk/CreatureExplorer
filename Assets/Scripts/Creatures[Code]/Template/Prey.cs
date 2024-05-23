using UnityEngine;

public class Prey : Creature
{
    [Header("Prey")]
    [SerializeField] protected CreatureState reactionToDanger;
    [SerializeField] private float predatorAwarenessRange = 40;

    protected override void Start()
    {
        if (surroundCheck == null)
            surroundCheck = new CheckSurroundings(CheckForPredators);
        else
            surroundCheck += CheckForPredators;

        surroundCheck += CheckForFleeing;
        base.Start();
    }

    protected override void UpdateCreatureState()
    {
        CheckForFleeing();
        base.UpdateCreatureState();
    }

    protected void CheckForFleeing()
    {
        if (WaryOff == null || WaryOff == Vector3.zero|| waryLoudness == 0)
            return;
        bool nearDanger = (WaryOff - transform.position).sqrMagnitude < (waryLoudness + data.HearingSensitivity * CurrentAction.Awareness);
        worldState =  nearDanger? SetConditionTrue(worldState, Condition.IsNearDanger) : SetConditionFalse(worldState, Condition.IsNearDanger);
        CheckForInterruptions(StateType.Fear, GetComponentInChildren<Flee>(), "Terrified", 90);
    }

    protected virtual void ReactToThreat(Vector3 threatPosition, float threatLoudness = 1)
    {
        WaryOff = threatPosition;
        waryLoudness = threatLoudness;
    }

    protected void ReactToThreat(Vector3 threatPosition, CreatureState reaction, float threatLoudness = 1)
    {
        UpdateValues(reaction);
        ReactToThreat(threatPosition, threatLoudness);
    }
    
    protected void ReactToThreat(Torca predator, float predatorLoudness = 1) => ReactToThreat(predator.transform.position, reactionToDanger, predatorLoudness);

    protected void CheckForPredators()
    {
        Torca predator = null;

        if (LookForObjects<Torca>.TryGetClosestObject(predator, transform.position, predatorAwarenessRange*CurrentAction.Awareness, out predator))
        {
# if UNITY_EDITOR
            DebugMessage("Sees Torca");
#endif
            ReactToThreat(predator);
        }
    }

}
