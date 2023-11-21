using System.Threading.Tasks;
using UnityEngine;

public class Sleep : Action
{
    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        failToken = failSource.Token;
        DoAction();
        FailCheck(failToken);
        return target;
    }

    public override void CalculateCostAndReward(CreatureState currentState, MoodState targetMood, float targetMoodPrio)
    {
        base.CalculateCostAndReward(currentState, targetMood, targetMoodPrio);
    }

    protected override async void DoAction( GameObject target = null)
    {
        await Task.Delay((int)actionDuration * 1000);

        base.DoAction();
    }
}
