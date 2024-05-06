using System.Threading.Tasks;
using UnityEngine;

public class Dig : Action
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
        // TODO: dig? could be fully through animation
        await Task.Delay((int)actionDuration * 1000);

        base.DoAction();
    }
}
