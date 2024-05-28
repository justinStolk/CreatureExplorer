using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class Play : NavigatedAction
{
    private NavMeshAgent moveAgent;

    protected override void Awake()
    {
        base.Awake();
        moveAgent = gameObject.GetComponentInParent<NavMeshAgent>();
    }

    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        moveAgent = gameObject.GetComponentInParent<NavMeshAgent>();
        //Task.Run(() => DoAction(), failToken);

        // Navmeshagent doesn't play nice with threading
        //DoAction();
        base.PerformAction(creature, target);
        FailCheck(failToken);
        return target;
    }

    public override void CalculateCostAndReward(CreatureState currentState, MoodState targetMood, float targetMoodPrio)
    {
        base.CalculateCostAndReward(currentState, targetMood, targetMoodPrio);
    }

    protected override void SetPathDestination()
    {
        moveAgent.SetDestination(moveAgent.transform.position + (moveAgent.transform.forward * 3 + moveAgent.transform.right));
    }

    protected override void MoveAction(GameObject target = null)
    {
        DoAction(target);
    }

    protected override async void DoAction(GameObject target = null)
    {
        float playTimer = actionDuration;

        while (playTimer > 0)
        {
            if (token.IsCancellationRequested)
                return;

            // TODO: make more performant?
            moveAgent.SetDestination(moveAgent.transform.position + (moveAgent.transform.forward*3 + moveAgent.transform.right));

            playTimer -= Time.deltaTime;
            await Task.Delay(200);// Yield();
        }

        base.DoAction();
    }
}
