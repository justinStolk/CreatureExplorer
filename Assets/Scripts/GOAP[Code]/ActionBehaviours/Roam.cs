using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class Roam : NavigatedAction
{
    [Header("Roam")]
    [SerializeField] private float roamRange;


    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        targetTransform = creature.transform;
        base.PerformAction(creature, target);

        return target;
    }

    protected override void SetPathDestination()
    {
        moveAgent.SetDestination(targetTransform.position + Random.insideUnitSphere * Random.Range(1, roamRange));
    }

    protected override void MoveAction(GameObject target = null)
    {
        DoAction(target);
    }

    protected override async void DoAction(GameObject target = null)
    {
        Task[] check = { CheckDistanceToDestination(), Task.Delay((int)(actionDuration *1000), token)};
        await Task.WhenAny(check);
        {
            base.DoAction();
        }
    }
}
