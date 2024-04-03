using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

// TODO: change to have herd consist of all chargers of same spawner
public class FindHerdCenter : Action
{
    [Header("Find Herd\nWarning: only made for Chargers")]
    [SerializeField] private float searchRadius = 1000;

    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        FailCheck(failToken);

        List<Charger> herd = LookForObjects<Charger>.CheckForObjects(creature.transform.position, searchRadius);

        if (herd.Count > 0)
        {
            Vector3 averagepos = new Vector3();

            foreach (Charger c in herd)
            {
                averagepos += c.transform.position;
            }

            averagepos /= herd.Count;

            GameObject g = new GameObject();
            g.transform.position = averagepos;

            DoAction();

            return g;
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
}
