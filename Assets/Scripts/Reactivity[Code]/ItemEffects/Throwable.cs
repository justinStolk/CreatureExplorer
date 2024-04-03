using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Throwable : StatusEffect, IInteractable, IThrowable
{
    public Sprite InventoryGraphic { get { return inventoryGraphic; } }
    public Sprite HoverGraphic { get { return hoverGraphic; } }
    [field: SerializeField] public string InteractionPrompt { get; private set; } = "Pick Up [E]";

    [SerializeField] private float splatVelocity = 2;
    [SerializeField] private Sprite inventoryGraphic;
    [SerializeField] private Sprite hoverGraphic;

    [SerializeField] private float maxVelocity = 20;

    [SerializeField] private AudioClip throwSound;

    [ShowOnly] private bool isGrabbed;

    private Vector3[] previousPositions;

    private Rigidbody rb;
    private Collider throwCollider;

    private void Awake()
    {
        previousPositions = new Vector3[10];
        previousPositions[0] = transform.position;

        rb = GetComponent<Rigidbody>();
        throwCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.sqrMagnitude > splatVelocity && collision.gameObject.TryGetComponent(out Creature creature))
        {
            GetComponent<MeshRenderer>().enabled = false;

            TriggerStatusEffect(creature);
        }
    }

    private void FixedUpdate()
    {
        if (isGrabbed)
        {
            for (int x = 9; x > 0; x--)
            {
                previousPositions[x] = previousPositions[x - 1];
            }

            previousPositions[0] = transform.position;
        }
    }

    public void Grab(Transform handTransform)
    {
        Interact();

        //Debug.Log("grabbed");
        transform.SetParent(handTransform, true);

        isGrabbed = true;
    }

    public void Release()
    {
        //Debug.Log("released");
        isGrabbed = false;

        Vector3 throwVelocity = ThrowVelocity();

        Vector3.ClampMagnitude(throwVelocity, maxVelocity);
        Throw(throwVelocity);

        previousPositions = new Vector3[10];
    }

    public void Throw(Vector3 direction, float force =1)
    {
        if (TryGetComponent(out SoundPlayer player))
        {
            player.PlaySound(throwSound, true);
        }

        throwCollider.enabled = true;

        if (force == 1)
            rb.velocity = direction;
        else
            rb.AddForce(direction * force);

        if (TryGetComponent(out Food food))
        {
            food.ActivatePhysics();
        }
        else
        {
            transform.SetParent(null);
            throwCollider.isTrigger = false;
            rb.useGravity = true;
        }
    }

    public void Interact()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        throwCollider.enabled = false;
        rb.useGravity = false;
        if(TryGetComponent(out Food food))
        {
            food.StopAllCoroutines();
        }
    }

    private Vector3 ThrowVelocity()
    {
        float timeMultiplier = 1 / Time.fixedDeltaTime;
        Vector3 averageVelocity = Vector3.zero;// (previousPositions[0] - previousPositions[1]) * timeMultiplier;

        for (int x = 1; x < previousPositions.Length-1; x++)
        {
            averageVelocity += (previousPositions[x] - previousPositions[x + 1]) * timeMultiplier;
        }

        averageVelocity /= 8;

        return averageVelocity;
    }
}
