using System.Threading.Tasks;
using UnityEngine;

public class Search : Action
{
    [Header("Search")]
    [SerializeField] private SearchTarget searchTarget;
    [SerializeField] private float searchRadius = 1000;

    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        FailCheck(failToken);

        float distance = searchRadius;
        Collider nearest = null;
        
        switch (searchTarget)
        {
            case (SearchTarget.Food):

                foreach (Collider c in Physics.OverlapSphere(creature.transform.position, searchRadius))
                {
                    if ((c.gameObject.GetComponent(creature.data.FoodSource) != null) && (c.transform.position - creature.transform.position).sqrMagnitude < distance)
                    {
                        distance = (c.transform.position - creature.transform.position).sqrMagnitude;
                        nearest = c;
                    }
                }
                break;

            case (SearchTarget.Tree):
                IBreakable t = new FruitTree();
                if (LookForObjects<IBreakable>.TryGetClosestObject(t, creature.transform.position, searchRadius, out t))
                {
                    DoAction();
                    return t.gameObject;
                }
                break;

            case (SearchTarget.AttackTarget):

                CanBeAttacked tempTarget = null;
                if (LookForObjects<CanBeAttacked>.TryGetClosestObject(tempTarget, creature.transform.position, searchRadius, creature.gameObject, out tempTarget))
                {
                    DoAction();
                    return tempTarget.gameObject;
                }
                break;
            case (SearchTarget.SleepingSpot):

                foreach (Collider c in Physics.OverlapSphere(creature.transform.position, searchRadius))
                {
                    if ((c.gameObject.GetComponent(creature.data.SleepSpot) != null) && (c.transform.position - creature.transform.position).sqrMagnitude < distance)
                    {
                        distance = (c.transform.position - creature.transform.position).sqrMagnitude;
                        nearest = c;
                    }
                }
                break;
        }
        
        if (nearest != null)
        {
            DoAction();
            return nearest.gameObject;
        }

        return target;
    }

    public override void CalculateCostAndReward(CreatureState currentState, MoodState targetMood, float targetMoodPrio)
    {
        base.CalculateCostAndReward(currentState, targetMood, targetMoodPrio);
    }

    protected override async void DoAction(GameObject target = null)
    {
        await Task.Delay((int)actionDuration * 1000);

        base.DoAction();
    }

    public enum SearchTarget
    {
        Food,
        Tree,
        AttackTarget,
        SleepingSpot
    }
}
