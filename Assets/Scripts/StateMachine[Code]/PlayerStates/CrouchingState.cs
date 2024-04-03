using UnityEngine;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(Rigidbody), typeof(PhysicsStepper))]
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

    [SerializeField] private Rigidbody rb;
    private PhysicsStepper stepper;

    [SerializeField] private CapsuleCollider capsuleCollider;


    private void Awake()
    {
        //rigidbody = GetComponent<Rigidbody>();
        stepper = GetComponent<PhysicsStepper>();
    }

    private void Start()
    {
        if (!VRChecker.IsVR) 
        {
            try
            {
                standardColliderHeight = capsuleCollider.height;
        
                firstPersonCamera = Camera.main;

                if (firstPersonCamera.TryGetComponent(out FollowTarget target))
                    defaultEyeHeight = target.TrueOffset.y;
                else
                    defaultEyeHeight = firstPersonCamera.GetComponentInParent<FollowTarget>().TrueOffset.y;
            }
            catch (System.NullReferenceException e)
            {
#if UNITY_EDITOR
                Debug.LogError("Crouchingstate: No followtarget found");
#endif
            }

        }
    }

    public override void OnStateEnter()
    {
        if (VRChecker.IsVR)
            return;

        capsuleCollider.height = crouchHeight;
        capsuleCollider.center = Vector3.up * Mathf.Max((crouchHeight * 0.5f),capsuleCollider.radius);

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
        if (VRChecker.IsVR)
            return;

        capsuleCollider.height = standardColliderHeight;
        capsuleCollider.center = Vector3.up * (standardColliderHeight * 0.5f);


        if (firstPersonCamera.TryGetComponent(out FollowTarget target))
            target.ChangeOffset(new Vector3(0, defaultEyeHeight, 0));
        else
            firstPersonCamera.GetComponentInParent<FollowTarget>().ChangeOffset(new Vector3(0, defaultEyeHeight, 0));
    }

    public void GetMoveInput(InputAction.CallbackContext callbackContext)
    {
        moveInput = callbackContext.ReadValue<Vector2>().normalized;
    }

    // TODO: remove
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

    public void ToggleCrouch(float eyeHeight)
    {
        if (Owner.CurrentState.GetType() == typeof(WalkingState) && eyeHeight <= crouchEyeHeight)
        {
            Owner.SwitchState(GetType());
            return;
        }
        if (Owner.CurrentState.GetType() == typeof(CrouchingState) && eyeHeight > crouchEyeHeight)
        {
            Owner.SwitchState(typeof(WalkingState));
        }
    }

    private void Move()
    {
        if (moveInput.sqrMagnitude >= 0.1f)
        {
            PlayerController.SetLoudness(sneakLoudness);

            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + transform.eulerAngles.y;
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * (VRChecker.IsVR ? transform.forward : Vector3.forward);

            stepper.HandleStep(ref rb, moveDirection);

            float verticalVelocity = rb.velocity.y;

            Vector3 newVelocity = moveDirection.normalized * sneakSpeed;

            newVelocity.y = verticalVelocity;

            rb.velocity = newVelocity;

            return;
        }
        else
        {
            PlayerController.SetLoudness(1);
        }
        rb.velocity = rb.velocity.y * Vector3.up;
    }

}
