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
    [SerializeField] private float sprintLoudness = 15f;

    [SerializeField] private LayerMask playerLayer;

    private bool isSprinting;

    private Vector2 moveInput;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private Rigidbody rb;
    private PhysicsStepper stepper;

    private bool inputByKeyboard = false;

    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

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
        inputByKeyboard = (callbackContext.control.device.name == "Keyboard");

        moveInput = callbackContext.ReadValue<Vector2>();//.normalized;
    }

    public void GetJumpInput(InputAction.CallbackContext callbackContext)
    {
        inputByKeyboard = (callbackContext.control.device.name == "Keyboard");

        if (callbackContext.started && Physics.CheckSphere(transform.position, 0.25f, ~playerLayer, QueryTriggerInteraction.Ignore) && Owner.CurrentState.GetType() == GetType())
        {
            Owner.SwitchState(typeof(JumpingState));
        }
    }

    private void Move()
    {
        float inputMagnitute = moveInput.magnitude;
        inputMagnitute = speedCurve.Evaluate(inputMagnitute);

        if (inputMagnitute >= 0.1f)
        {
            float speed;
            speed = speedMultiplier * inputMagnitute;

            PlayerController.SetLoudness(Mathf.Lerp(minWalkLoudness, maxWalkLoudness, inputMagnitute));

            float inputAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
            float targetAngle = inputAngle + transform.eulerAngles.y;

            if (isSprinting)
            {
                PlayerController.SetLoudness(sprintLoudness);
                speed = Mathf.Abs(inputAngle) <= maxSprintAngle ? sprintSpeed : strafeSprintSpeed;
            }

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * cameraTransform.forward;

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
