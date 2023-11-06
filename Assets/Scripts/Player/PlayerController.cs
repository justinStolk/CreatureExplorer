using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float Loudness { get; private set; }

    [SerializeField] private float maximumViewAngle = 70f;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private float interactionRadius = 1.25f;

    [SerializeField] private GameSettings gameSettings;

    [SerializeField] private LayerMask interactionLayers;

    [SerializeField] private UnityEvent onScrapbookOpened;
    [SerializeField] private UnityEvent onCameraOpened;
    [SerializeField] private UnityEvent onCameraClosed;

    [SerializeField] private UnityEvent<string> onInteractableFound;
    [SerializeField] private UnityEvent onInteractableOutOfRange;

    //[SerializeField] private Camera pictureCamera;

    private float verticalRotation;

    private Vector2 rotationInput;

    private FiniteStateMachine stateMachine;

    private Camera firstPersonCamera;

    private PlayerInput playerInput;

    private IInteractable interactableInRange;

    private void Awake()
    {
        firstPersonCamera = Camera.main;
        verticalRotation = firstPersonCamera.transform.eulerAngles.x;

        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        stateMachine = new FiniteStateMachine(typeof(WalkingState), GetComponents<IState>());
        onCameraClosed?.Invoke();
    }

    // Update is called once per frame
    private void Update()
    {
        stateMachine.OnUpdate();
        HandleRotation(rotationInput);

        if (Physics.SphereCast(transform.position, interactionRadius, transform.forward, out RaycastHit hit, interactionDistance, interactionLayers))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                onInteractableFound?.Invoke(interactable.InteractionPrompt);
                interactableInRange = interactable;
            }
        }
        else if(interactableInRange != null)
        {
            interactableInRange = null;
            onInteractableOutOfRange?.Invoke();
        }
    }

    private void FixedUpdate()
    {
        stateMachine.OnFixedUpdate();
        firstPersonCamera.transform.rotation = Quaternion.Euler(new Vector3(verticalRotation, transform.eulerAngles.y, 0));
        //pictureCamera.transform.rotation = Quaternion.Euler(new Vector3(verticalRotation, transform.eulerAngles.y, 0));
    }

    public void SwapToCamera(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            playerInput.SwitchCurrentActionMap("Camera");
            onCameraOpened?.Invoke();
        }
    }

    public void SwapFromCamera(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            playerInput.SwitchCurrentActionMap("Overworld");
            onCameraClosed?.Invoke();
        }
    }

    public void GetInteractionInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started && interactableInRange != null)
        {
            //if(interactableInRange.GetType() == typeof(JellyfishLadder))
            //{
            //    stateMachine.SwitchState(typeof(ClimbingState));
            //    return;
            //}
            interactableInRange.Interact();
            
        }
    }

    public void GetRotationInput(InputAction.CallbackContext callbackContext)
    {
        rotationInput = callbackContext.ReadValue<Vector2>();
    }
    
    public void GetCloseScrapbookInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            playerInput.SwitchCurrentActionMap("Overworld");
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public void GetOpenScrapbookInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            playerInput.SwitchCurrentActionMap("Scrapbook");
            onScrapbookOpened?.Invoke();
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SetLoudness(float newLoudness) => Loudness = newLoudness;

    private void HandleRotation(Vector2 lookInput)
    { 
        verticalRotation = Mathf.Clamp(verticalRotation - (lookInput.y * gameSettings.LookSensitivity), -maximumViewAngle, maximumViewAngle);
        transform.Rotate(new Vector3(0, lookInput.x * gameSettings.LookSensitivity, 0));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.25f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + (transform.forward * interactionDistance), interactionRadius);
    }
}
