using System.Threading.Tasks;
using UnityEngine;

public class Pounce : Action
{
    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        failToken = failSource.Token;
        DoAction();
        FailCheck(failToken);
        return target;
    }

    protected override async void DoAction(GameObject target = null)
    {
        // TODO: check for bugs near tail
        await Task.Delay((int)actionDuration * 1000);

        base.DoAction();
    }
}
