using System.Threading.Tasks;
using UnityEngine;

public class Fly : Action
{
    [Header("Fly")]
    [SerializeField] private float roamRange;
    [SerializeField] private float minimumFlyHeight;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] protected float speedMultiplier = 1;
    [SerializeField] protected float targetingPrecision = 1f;

    private Vector3 destination;

    private SkyNavigator flyAgent;

    private readonly RaycastHit[] rayHit = new RaycastHit[100];

    private void Start()
    {
        flyAgent = gameObject.GetComponentInParent<SkyNavigator>();
    }

    public override GameObject PerformAction(Creature creature, GameObject target)
    {
        if (target == null)
        {
            destination = creature.transform.position + Random.insideUnitSphere * Random.Range(1, roamRange);
        }
        else
        {
            destination = target.transform.position;
        }

        //Checkheight(destination, out destination);
        flyAgent.AlterSpeed(speedMultiplier);
        flyAgent.SetDestination(destination);

        failToken = failSource.Token;
        DoAction();
        FailCheck(failToken);
        return null;
    }

    private void Checkheight(Vector3 checkPosition, out Vector3 newHeight)
    {        
        float terrainHeight = Terrain.activeTerrain.SampleHeight(checkPosition);

        // TODO: figure out why checkposition = transform.position instead of destination
#if UNITY_EDITOR
        Debug.Log($"terrainHeight: {terrainHeight}, Destinationheight: {checkPosition.y}");
#endif
        if (checkPosition.y < terrainHeight)
        {
#if UNITY_EDITOR
            Debug.Log("trying to fly below ground");
#endif
            checkPosition = new Vector3(checkPosition.x, terrainHeight + minimumFlyHeight, checkPosition.z);
        } 
        else if (Physics.RaycastNonAlloc(checkPosition, Vector3.down, rayHit, minimumFlyHeight, groundLayer) > 0)
        {
            checkPosition += new Vector3(0, minimumFlyHeight - rayHit[0].distance, 0);
        }
        newHeight = checkPosition;
    }

    public override void Reset()
    {
        base.Reset();

        if (flyAgent != null && flyAgent.isActiveAndEnabled)
        {
            flyAgent.ResetSpeed();

            flyAgent.SetDestination(transform.position);
        }

        if (animator != null)
            animator.speed = 1;
    }

    protected override async void DoAction(GameObject target = null)
    {
        Task[] check = { CheckDistanceToDestination(), Task.Delay((int)(actionDuration * 1000), token) };

        await Task.WhenAny(check);
        {
            base.DoAction();
        }

        flyAgent.ResetSpeed();
    }

    protected async Task CheckDistanceToDestination(float extraMargin = 0)
    {
        //while ((destination - transform.position).magnitude > (targetingPrecision + extraMargin))
        while (flyAgent.PathLength() > (targetingPrecision + extraMargin))
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
