using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public abstract class NavigatedAction : Action
{
    [Header("Navigation")]
    [SerializeField] protected float speedMultiplier = 1;
    [SerializeField] protected float targetingPrecision = 0.5f;

    protected NavMeshAgent moveAgent;
    protected Transform targetTransform;

    protected float originalSpeed, originalRotationSpeed, originalAcceleration;

    private void Start()
    {
        moveAgent = gameObject.GetComponentInParent<NavMeshAgent>();
        originalSpeed = moveAgent.speed;
        originalRotationSpeed = moveAgent.angularSpeed;
        originalAcceleration = moveAgent.acceleration;
    }

    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        moveAgent = gameObject.GetComponentInParent<NavMeshAgent>();

        if (targetTransform == null && target != null)
        {
            targetTransform = target.transform;
        }

        originalSpeed = moveAgent.speed;
        originalRotationSpeed = moveAgent.angularSpeed;
        originalAcceleration = moveAgent.acceleration;

        moveAgent.speed *= speedMultiplier;
        moveAgent.angularSpeed *= speedMultiplier;
        moveAgent.acceleration *= speedMultiplier;
        moveAgent.autoBraking = false;

        SetPathDestination();

        if (animator != null)
            animator.speed = moveAgent.speed / originalSpeed;

        //Task.Run(() => DoAction(), token);
        MoveAction(target);
        FailCheck(failToken);

        return target;
    }

    protected abstract void SetPathDestination();

    public override void CalculateCostAndReward(CreatureState currentState, MoodState targetMood, float targetMoodPrio)
    {
        base.CalculateCostAndReward(currentState, targetMood, targetMoodPrio);
    }

    public override void Reset()
    {
        base.Reset();

        targetTransform = null;

        if (moveAgent != null && moveAgent.isActiveAndEnabled)
        {
            moveAgent.speed = originalSpeed;
            moveAgent.angularSpeed = originalRotationSpeed;
            moveAgent.acceleration = originalAcceleration;
            moveAgent.autoBraking = true;

            moveAgent.ResetPath();
            moveAgent.SetDestination(transform.position);
        }

        if (animator != null)
            animator.speed = 1;

    }

    public override void Stop()
    {
        targetTransform = null;

        moveAgent.speed = originalSpeed;
        moveAgent.angularSpeed = originalRotationSpeed;
        moveAgent.acceleration = originalAcceleration;
        moveAgent.autoBraking = true;

        moveAgent.ResetPath();
        moveAgent.SetDestination(transform.position);

        if (animator != null)
            animator.speed = 1;

        base.Stop();
    }

    protected abstract void MoveAction(GameObject target);

    protected override async void DoAction(GameObject target = null)
    {
        ResetNavigation();
        base.DoAction();
    }

    protected void ResetNavigation()
    {
        if (creatureDeactivated)
            return;

        moveAgent.speed = originalSpeed;
        moveAgent.angularSpeed = originalRotationSpeed;
        moveAgent.acceleration = originalAcceleration;
        moveAgent.autoBraking = true;

        if (animator != null)
            animator.speed = 1;

        if (GetComponentInParent<NavMeshAgent>().enabled)
        {
            moveAgent.ResetPath();
            moveAgent.SetDestination(transform.position);
        }
    }

    protected async Task CheckDistanceToDestination(float extraMargin = 0)
    {
        while ((moveAgent.destination - moveAgent.transform.position).magnitude > (targetingPrecision+extraMargin))
        {
            if (token.IsCancellationRequested)
                return;

            if (creatureDeactivated)
            {
                break;
            }

            await Task.Yield();
        }
    }
}

