using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class Move : NavigatedAction
{
    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        if (target == null)
        {
            failed = true;
            return target;
        }

        base.PerformAction(creature, target);

        return target;
    }
    protected override void SetPathDestination()
    {
        moveAgent.SetDestination(targetTransform.position);
    }

    protected override void MoveAction(GameObject target = null)
    {
        DoAction(target);
    }

    protected override async void DoAction(GameObject target = null)
    {
        Task check = CheckDistanceToDestination();
        
        while (!check.IsCompletedSuccessfully && !token.IsCancellationRequested) 
        {
            if (targetTransform == null)
            {
                failed = true;
                return;
            }

            if ((moveAgent.destination - targetTransform.position).sqrMagnitude > 1f)
            {
                SetPathDestination();
            }
            // wait half a second before updating again
            await Task.Delay(500);
        }

        base.DoAction();
    }
}
