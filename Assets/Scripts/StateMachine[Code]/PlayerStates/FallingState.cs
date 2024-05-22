using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(Rigidbody))]
public class FallingState : State
{
    [SerializeField] private float painfulVelocity = 3f;
    [SerializeField] private float cripplingVelocity = 5f;
    [SerializeField] private float lethalVelocity = 10f;

    [SerializeField] private float aerialSpeed = 4f;
    [SerializeField] private float maxHorizontalVelocity = 5f;

    [SerializeField] private AudioSource dbg_SharedSource;
    [SerializeField] private AudioClip dbg_DeathSound;

    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private UnityEvent onLethalLanding;

    //private float fallVelocity;
    [SerializeField] private Rigidbody rb;

    private Vector2 moveInput;
    [SerializeField] private Transform cameraTransform;

    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        //rigidbody = GetComponent<Rigidbody>();
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.gameObject != gameObject)
    //    {
    //        Debug.Log(rigidbody.velocity.y);
    //    }
    //}

    public override void OnStateUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.up * -1f, out RaycastHit hit, 1f, ~playerLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.TryGetComponent(out BounceSurface surface))
            {
                if (surface.Bounce(rb.velocity.y * -1, out float exitForce))
                {
                    rb.velocity = new(rb.velocity.x, 0, rb.velocity.z);
                    rb.AddForce(exitForce * Vector3.up, ForceMode.VelocityChange);
                    return;
                }
            }
        }
        if (Physics.CheckSphere(transform.position, 0.25f, ~playerLayer, QueryTriggerInteraction.Ignore))
        {
            if (rb.velocity.y <= -lethalVelocity)
            {
                // Die
                dbg_SharedSource.clip = dbg_DeathSound;
                dbg_SharedSource.Play();
                onLethalLanding?.Invoke();
                return;
            }
            if (rb.velocity.y <= -cripplingVelocity)
            {
                // Incapacitate the player for a while
                Owner.SwitchState(typeof(CrippledState));
                return;
            }
            if(rb.velocity.y <= -painfulVelocity)
            {
                // This hurts the player a bit for a short while.
                Owner.SwitchState(typeof(HurtState));
                return;
            }
            Owner.SwitchState(typeof(WalkingState));
        }
    }
    public override void OnStateFixedUpdate()
    {
        Move();
    }

    //public override void OnStateExit()
    //{
    //    fallVelocity = 0;
    //}

    public void GetMoveInput(InputAction.CallbackContext callbackContext)
    {
        moveInput = callbackContext.ReadValue<Vector2>().normalized;
    }

    private void Move()
    {
        if (moveInput.sqrMagnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + rb.transform.eulerAngles.y;

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * cameraTransform.forward;
            //Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * (VRChecker.IsVR ? transform.forward : Vector3.forward);

            Vector3 newVelocity = moveDirection.normalized * aerialSpeed;

            rb.AddForce(newVelocity, ForceMode.Acceleration);

            Vector2 horizontalVelocity = new(rb.velocity.x, rb.velocity.z);

            if(horizontalVelocity.sqrMagnitude >= maxHorizontalVelocity * maxHorizontalVelocity)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxHorizontalVelocity;
                rb.velocity = new(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.y);
            }

        }
    }
}
