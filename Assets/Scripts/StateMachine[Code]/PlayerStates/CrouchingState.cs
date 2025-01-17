using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(PhysicsStepper))]
public class CrouchingState : State
{
    [SerializeField] private float sneakSpeed = 3f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchEyeHeight = 0.8f;

    [SerializeField] private float sneakLoudness = 1f;

    [SerializeField] private LayerMask playerLayer;

    private float defaultEyeHeight;
    private float standardColliderHeight;

    private Camera firstPersonCamera;

    private Vector2 moveInput;

    private new Rigidbody rigidbody;
    private PhysicsStepper stepper;

    private CapsuleCollider capsuleCollider;


    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        stepper = GetComponent<PhysicsStepper>();

        capsuleCollider = GetComponent<CapsuleCollider>();

        standardColliderHeight = capsuleCollider.height;

        firstPersonCamera = Camera.main;
    }

    private void Start()
    {
        //if (!VRChecker.IsVR)
            //defaultEyeHeight = firstPersonCamera.GetComponent<FollowTarget>().TrueOffset.y;
    }

    public override void OnStateEnter()
    {
        capsuleCollider.height = crouchHeight;
        capsuleCollider.center = Vector3.up * (crouchHeight * 0.5f);

        if (firstPersonCamera.TryGetComponent(out FollowTarget target))
            target.ChangeOffset(new Vector3(0, crouchEyeHeight, 0));
        else
            firstPersonCamera.GetComponentInParent<FollowTarget>().ChangeOffset(new Vector3(0, crouchEyeHeight, 0));
    }

    public override void OnStateFixedUpdate()
    {
        Move();
    }

    public override void OnStateExit()
    {
        capsuleCollider.height = standardColliderHeight;
        capsuleCollider.center = Vector3.up * (standardColliderHeight * 0.5f);

        if (!VRChecker.IsVR)
            firstPersonCamera.GetComponent<FollowTarget>().ChangeOffset(new Vector3(0, defaultEyeHeight, 0));
    }

    public void GetMoveInput(InputAction.CallbackContext callbackContext)
    {
        moveInput = callbackContext.ReadValue<Vector2>().normalized;
    }

    public void GetCrouchInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            if(Owner.CurrentState.GetType() == typeof(WalkingState))
            {
                Owner.SwitchState(GetType());
                return;
            }
            if (Owner.CurrentState.GetType() == GetType())
            {
                Owner.SwitchState(typeof(WalkingState));
            }
        }
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
        if (moveInput.sqrMagnitude >= 0.1f)
        {
            PlayerController.SetLoudness(sneakLoudness);

            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + rigidbody.transform.eulerAngles.y;
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            stepper.HandleStep(ref rigidbody, moveDirection);

            float verticalVelocity = rigidbody.velocity.y;

            Vector3 newVelocity = moveDirection.normalized * sneakSpeed;

            newVelocity.y = verticalVelocity;

            rigidbody.velocity = newVelocity;

            return;
        }
        else
        {
            PlayerController.SetLoudness(0);
        }
        rigidbody.velocity = rigidbody.velocity.y * Vector3.up;
    }

}
