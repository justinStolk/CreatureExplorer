using UnityEngine;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(Rigidbody), typeof(PhysicsStepper))]
public class WalkingState : State
{
    [SerializeField] private float speedMultiplier = 5f;
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float strafeSprintSpeed = 8f;
    [SerializeField] private float maxSprintAngle = 15f;

    [SerializeField] private float minWalkLoudness = 5f;
    [SerializeField] private float maxWalkLoudness = 15f;

    [SerializeField] private LayerMask playerLayer;

    private bool isSprinting;

    private Vector2 moveInput;

    [SerializeField] private Rigidbody rb;
    private PhysicsStepper stepper;

    private void Awake()
    {
        //rigidbody = GetComponent<Rigidbody>();
        stepper = GetComponent<PhysicsStepper>();
        if(strafeSprintSpeed >= sprintSpeed)
        {
            throw new System.Exception("Strafe Sprint Speed can't be higher than or as fast as sprint speed! Strafing must be slower than forward sprinting!");
        }
    }

    public override void OnStateUpdate()
    {
        if (!Physics.CheckSphere(rb.transform.position, 0.25f, ~playerLayer, QueryTriggerInteraction.Ignore))
        {
            Owner.SwitchState(typeof(FallingState));
        }
    }
    public override void OnStateFixedUpdate()
    {

        Move();
    }

    public void GetSprintInput(InputAction.CallbackContext callbackContext)
    {
        isSprinting = callbackContext.performed;
    }

    public void GetMoveInput(InputAction.CallbackContext callbackContext)
    {
        moveInput = callbackContext.ReadValue<Vector2>();//.normalized;
    }

    public void GetJumpInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started && Physics.CheckSphere(transform.position, 0.25f, ~playerLayer, QueryTriggerInteraction.Ignore) && Owner.CurrentState.GetType() == GetType())
        {
            Owner.SwitchState(typeof(JumpingState));
        }
    }

    private void Move()
    {
        float inputMagnitute = moveInput.magnitude;
        inputMagnitute = speedCurve.Evaluate(inputMagnitute);

        if (moveInput.sqrMagnitude >= 0.1f)
        {
            isSprinting = inputMagnitute * speedMultiplier >= sprintSpeed;

            PlayerController.SetLoudness(Mathf.Lerp(minWalkLoudness, maxWalkLoudness, inputMagnitute));

            float speed = speedMultiplier * inputMagnitute;
            float inputAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
            float targetAngle = inputAngle + transform.eulerAngles.y;

            if (isSprinting)
            {
                speed = Mathf.Abs(inputAngle) <= maxSprintAngle ? sprintSpeed : strafeSprintSpeed;
            }

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * (VRChecker.IsVR? transform.forward : Vector3.forward);

            stepper.HandleStep(ref rb, moveDirection);

            float verticalVelocity = rb.velocity.y;

            Vector3 newVelocity = moveDirection.normalized * speed;

            newVelocity.y = verticalVelocity;

            rb.velocity = newVelocity;
        } else
        {
            rb.velocity = Vector3.up * rb.velocity.y;
            PlayerController.SetLoudness(1);
        }
    }
}
