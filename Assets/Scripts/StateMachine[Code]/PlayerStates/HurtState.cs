using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//[RequireComponent(typeof(Rigidbody))]
public class HurtState : State
{
    [SerializeField] private float hurtTime = 5f;
    [SerializeField] private float hurtMoveSpeed = 3.5f;
    [SerializeField] private float vignetteStrength = 0.3f;

    [SerializeField] private AudioSource sharedPlayerSource;
    [SerializeField] private AudioClip hurtSound;

    [SerializeField] private Volume volume;

    private float timer = 0f;

    private Vector2 moveInput;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private Rigidbody rb;
    private PhysicsStepper stepper;


    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        //rigidbody = GetComponent<Rigidbody>();
        stepper = GetComponent<PhysicsStepper>();
    }

    public override void OnStateEnter()
    {
        sharedPlayerSource.clip = hurtSound;
        sharedPlayerSource.Play();
        if(volume == null)
        {
            return;
        }
        if(volume.profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.value = vignetteStrength;
        }
    }

    public override void OnStateUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= hurtTime)
        {
            timer = 0f;
            Owner.SwitchState(typeof(WalkingState));
        }

        Move();
    }
    public override void OnStateExit()
    {
        if (volume == null)
        {
            return;
        }
        if (volume.profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.value = 0;
        }
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

            stepper.HandleStep(ref rb, moveDirection);

            float verticalVelocity = rb.velocity.y;

            Vector3 newVelocity = moveDirection.normalized * hurtMoveSpeed;

            newVelocity.y = verticalVelocity;

            rb.velocity = newVelocity;

            return;
        }
        //rigidbody.velocity = rigidbody.velocity.y * Vector3.up;
    }
}
