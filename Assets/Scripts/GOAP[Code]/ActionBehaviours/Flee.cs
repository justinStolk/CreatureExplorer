using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class Flee : NavigatedAction
{
    [Header("Flee")]
    [SerializeField] private float runDistance= 10;

    private Vector3 fearSource;

    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        targetTransform = creature.transform;
        fearSource = creature.GetComponent<Creature>().WaryOff;

        base.PerformAction(creature, target);

        return target;
    }

    protected override void SetPathDestination()
    {
        if (fearSource == null)
        {
            fearSource = transform.position - transform.forward;
        }

        moveAgent.SetDestination(targetTransform.position +(targetTransform.position - fearSource).normalized*runDistance);
    }
    protected override void MoveAction(GameObject target = null)
    {
        DoAction(target);
    }

    protected override async void DoAction(GameObject target = null)
    {
        Task[] tasks = {Task.Delay((int)(actionDuration * 1000), token), CheckDistanceToDestination()};

        await Task.WhenAny(tasks);// .Delay((int)(actionDuration * 1000));
        {
            base.DoAction();
        }
    }
}
