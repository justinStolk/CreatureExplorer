using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class Ram : NavigatedAction
{
    protected override void MoveAction(GameObject target = null) 
    {
        DoAction(target);
    }

    protected override async void DoAction(GameObject target = null)
    {
        await CheckDistanceToDestination();

        if (target.TryGetComponent(out IBreakable broken) && !token.IsCancellationRequested)
        {
            broken.Break();
        }

        base.DoAction();
    }

    protected override void SetPathDestination()
    {
        moveAgent.SetDestination(targetTransform.position);
    }
}

