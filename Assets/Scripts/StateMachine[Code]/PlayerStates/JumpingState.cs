using UnityEngine;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(Rigidbody))]
public class JumpingState : State
{
    [SerializeField] private float jumpForce = 100f;
    [SerializeField] private float aerialSpeed;
    [SerializeField] private float maxHorizontalVelocity = 5f;

    [SerializeField] private LayerMask playerLayer;

    private Vector2 moveInput;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private Rigidbody rb;

    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        //rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnStateEnter()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    public override void OnStateUpdate()
    {
        if(!Physics.CheckSphere(transform.position, 0.25f, ~playerLayer, QueryTriggerInteraction.Ignore) && rb.velocity.y <= 0)
        {
            Owner.SwitchState(typeof(FallingState));
            return;
        }
        if(Physics.CheckSphere(transform.position, 0.25f, ~playerLayer, QueryTriggerInteraction.Ignore))
        {
            Owner.SwitchState(typeof(WalkingState));
        }
    }

    public override void OnStateFixedUpdate()
    {
        Move();
    }
    
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

            if (horizontalVelocity.sqrMagnitude >= maxHorizontalVelocity * maxHorizontalVelocity)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxHorizontalVelocity;
                rb.velocity = new(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.y);
            }
        }
    }
}
