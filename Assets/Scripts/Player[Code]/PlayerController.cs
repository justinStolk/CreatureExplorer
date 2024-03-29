using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR;

//[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public static float Loudness { get; private set; } = 5;
    public InputSystemUIInputModule InputModule { get { return module; } }

    [Header("Interaction and Physicality")]
    [SerializeField] private float maximumViewAngle = 70f;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private float interactHeight = 0.875f;
    [SerializeField] private float interactionRadius = 1.25f;
    [SerializeField] private float climbDistance = 0.25f;
    [SerializeField] private float readingDistance = 15f;
    [SerializeField] private float throwForce = 4f;
    [SerializeField] private Transform throwPoint;

    [SerializeField] private LayerMask interactionLayers;
    [SerializeField] private LayerMask waterLayer;

    [SerializeField] private GameSettings gameSettings;

    [Header("Events")]
    [SerializeField] private UnityEvent onScrapbookOpened;
    [SerializeField] private UnityEvent onScrapbookClosed;
    [SerializeField] private UnityEvent onCameraOpened;
    [SerializeField] private UnityEvent onCameraClosed;
    [SerializeField] private UnityEvent<string, Vector3> onInteractableFound;
    [SerializeField] private UnityEvent onInteractableOutOfRange;
    [SerializeField] private UnityEvent onPouchUnlocked;
    [SerializeField] private UnityEvent onClimbingUnlocked;
    [SerializeField] private UnityEvent onHurt;
    [SerializeField] private UnityEvent onBerryThrown;
    [SerializeField] private UnityEvent onBerryPickup;

    [Header("Climbing UI")]
    [SerializeField] private UnityEngine.UI.Image climbControlImage;
    [SerializeField] private Sprite climbDisabledSprite;
    [SerializeField] private Sprite climbEnabledSprite;


    [Header("Death and Respawn")]
    [SerializeField] private float respawnDuration = 0.5f;
    [SerializeField] private float drowningHeight = 1.2f;
    [SerializeField] private Transform respawnTransform;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject respawnOccluder;

    private BerryPouch pouch;
    //[SerializeField] private Camera pictureCamera;
    [Tooltip("Serialized for testing purposes")]
    [SerializeField] private bool climbingUnlocked;
    [Tooltip("Only present for testing purposes")]
    [SerializeField] private bool pouchUnlocked;

    [SerializeField] private InputSystemUIInputModule module;

    private bool died;
    private float verticalRotation;
    private float horizontalRotation;

    private Vector2 rotationInput;
    private float rotationSpeed = 1f;
    private bool berryPouchIsOpen;

    [SerializeField] private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private FiniteStateMachine stateMachine;

    private Camera firstPersonCamera;
    private MeshRenderer respawnFadeRenderer;

    private PlayerInput playerInput;

    private IInteractable interactableInRange;
    private Throwable heldThrowable;

    private void Awake()
    {
        if (!module)
        {
            throw new System.Exception("No Input Module assigned, this will break the interface handling and should not be skipped!");
        }

        //rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponentInParent<CapsuleCollider>();
        stateMachine = new FiniteStateMachine(typeof(WalkingState), GetComponents<IState>());
        firstPersonCamera = Camera.main;
        verticalRotation = firstPersonCamera.transform.eulerAngles.x;
        horizontalRotation = firstPersonCamera.transform.eulerAngles.y;

        respawnFadeRenderer = Instantiate(respawnOccluder, firstPersonCamera.transform).GetComponent<MeshRenderer>();
        deathScreen = Instantiate(deathScreen);
        deathScreen.SetActive(false);

        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        pouch = GetComponentInChildren<BerryPouch>();

        GrandTemple.OnRingExtended += UnlockPouch;

        /*
        StaticQuestHandler.OnShrineCompleted += () =>
        {
            playerInput.SwitchCurrentActionMap("Await");
            //module
            //if (playerInput.currentActionMap.name != "Dialogue")
            //    playerInput.SwitchCurrentActionMap("Overworld");
            //rb.isKinematic = true;
        };
        */
        StaticQuestHandler.OnQuestOpened += () =>
        {
            LinkModuleToScrapbook();
            playerInput.SwitchCurrentActionMap("Scrapbook");
            onInteractableOutOfRange?.Invoke();
        };
        StaticQuestHandler.OnQuestClosed += () =>
        {
            if (playerInput.currentActionMap.name != "Dialogue")
            {
                playerInput.SwitchCurrentActionMap("Overworld");
                LinkModuleToOverworld();
            }
            // rb.isKinematic = false;
            Cursor.lockState = CursorLockMode.Locked;
            stateMachine.SwitchState(typeof(WalkingState));
        };

        if (pouchUnlocked) UnlockPouch();

        if (climbingUnlocked) UnlockClimb();
    }

    private void Start()
    {
        onCameraClosed?.Invoke();

        Scrapbook.OnBeginType += StartTyping;
        Scrapbook.OnEndType += StopTyping;
    }

    // Update is called once per frame
    private void Update()
    {
        if (died) return;

        stateMachine.OnUpdate();
        HandleRotation(rotationInput);
        HandleInteract();

        if (Physics.CheckSphere(transform.position + Vector3.up * drowningHeight, 0.2f, waterLayer))
        {
            StartCoroutine(Die());
        }
        //if (Physics.SphereCast(transform.position, interactionRadius, transform.forward, out RaycastHit hit, interactionDistance, interactionLayers))
        //{
        //    if (hit.transform.TryGetComponent(out IInteractable interactable))
        //    {
        //        onInteractableFound?.Invoke(interactable.InteractionPrompt);
        //        interactableInRange = interactable;
        //    }
        //}
        //else if (interactableInRange != null)
        //{
        //    interactableInRange = null;
        //    onInteractableOutOfRange?.Invoke();
        //}
    }

    private void FixedUpdate()
    {
        stateMachine.OnFixedUpdate();

        firstPersonCamera.transform.rotation = Quaternion.Euler(new Vector3(verticalRotation, horizontalRotation, 0));
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

    public void StartTyping()
    {
        playerInput.actions.FindAction("QuickCloseBook").Disable();
    }

    public void StopTyping()
    {
        playerInput.actions.FindAction("QuickCloseBook").Enable();
    }

    public void GetInteractionInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started && interactableInRange != null)
        {
            if (climbingUnlocked && interactableInRange.GetType() == typeof(JellyfishLadder))
            {
                JellyfishLadder ladder = interactableInRange as JellyfishLadder;
                onInteractableOutOfRange?.Invoke();
                stateMachine.SwitchState(typeof(ClimbingState));
                transform.SetParent(ladder.transform);
                climbControlImage.sprite = climbDisabledSprite;
                return;
            } else if (interactableInRange.GetType() == typeof(JellyfishLadder))
            {
                onHurt?.Invoke();
            }

            if (interactableInRange.GetType() == typeof(Throwable))
            {
                Throwable berry = interactableInRange as Throwable;
                if (heldThrowable == null)
                {
                    CarryThrowable(berry);
                    pouch.HoldingBerry = true;
                    onBerryPickup?.Invoke();
                    return;
                }
                else if (pouch.AddBerry(berry))
                {
                    berry.gameObject.SetActive(false);
                    onBerryPickup?.Invoke();
                    return;
                }
            }
            else
            {
                interactableInRange.Interact();
            }
        }
    }



    public void GetRotationInput(InputAction.CallbackContext callbackContext)
    {
        rotationInput = callbackContext.ReadValue<Vector2>();
    }
    
    public void GetCloseQuestInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            playerInput.SwitchCurrentActionMap("Overworld");
            StaticQuestHandler.OnQuestClosed.Invoke();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void GetCloseScrapbookInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            playerInput.SwitchCurrentActionMap("Overworld");
            Cursor.lockState = CursorLockMode.Locked;
            onScrapbookClosed?.Invoke();
        }
    }
    public void GetOpenScrapbookInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            LinkModuleToScrapbook();
            playerInput.SwitchCurrentActionMap("Scrapbook");
            Cursor.lockState = CursorLockMode.None;
            onScrapbookOpened?.Invoke();
        }
    }

    public static void SetLoudness(float newLoudness) => Loudness = newLoudness;

    public void GetThrowInput(InputAction.CallbackContext context)
    {
        if (context.started && heldThrowable != null)
        {
            heldThrowable.GetComponent<Rigidbody>().isKinematic = false;
            heldThrowable.Throw(firstPersonCamera.transform.forward, throwForce);
            pouch.HoldingBerry = false;
            heldThrowable = null;
            onBerryThrown?.Invoke();
        }
    }
    public void ReceiveRetrievedBerry(Throwable berry)
    {
        CarryThrowable(berry);
    }

    public void GoDie() => StartCoroutine(Die());

    public void SetRotationSpeed(float newSpeed) => rotationSpeed = newSpeed;

    public void ToggleBerryPouch(InputAction.CallbackContext context)
    {
        if (!pouchUnlocked) return;

        if (context.started)
        {
            berryPouchIsOpen = !berryPouchIsOpen;
            if (berryPouchIsOpen)
            {
                pouch.OpenPouch();
                return;
            }
            pouch.ClosePouch();
        }
    }

    public void ToggleBerryPouch(bool newState)
    {
        if (!pouchUnlocked || berryPouchIsOpen == newState) return;

        berryPouchIsOpen = newState;
        if (berryPouchIsOpen)
        {
            pouch.OpenPouch();
            return;
        }
        pouch.ClosePouch();
    }

    public void LinkModuleToOverworld()
    {
        // The move element needs to be set for gamepad controls. Implement this later, as you won't be able to select berries without this.
        module.leftClick = InputActionReference.Create(playerInput.actions.FindActionMap("Overworld").FindAction("Click"));
        module.point = InputActionReference.Create(playerInput.actions.FindActionMap("Overworld").FindAction("Point"));
    }

    public void LinkModuleToScrapbook()
    {
        module.leftClick = InputActionReference.Create(playerInput.actions.FindActionMap("Scrapbook").FindAction("Click"));
        module.point = InputActionReference.Create(playerInput.actions.FindActionMap("Scrapbook").FindAction("Point"));
        module.move = InputActionReference.Create(playerInput.actions.FindActionMap("Scrapbook").FindAction("Move"));
    }
    
    public void LinkModuleToDialogue()
    {
        module.leftClick = InputActionReference.Create(playerInput.actions.FindActionMap("Dialogue").FindAction("Click"));
        module.point = InputActionReference.Create(playerInput.actions.FindActionMap("Dialogue").FindAction("Point"));
        module.move = InputActionReference.Create(playerInput.actions.FindActionMap("Dialogue").FindAction("Move"));
    }

    public void LinkModuleToPauseMenu()
    {
        module.leftClick = InputActionReference.Create(playerInput.actions.FindActionMap("Menu").FindAction("Click"));
        module.point = InputActionReference.Create(playerInput.actions.FindActionMap("Menu").FindAction("Point"));
        module.move = InputActionReference.Create(playerInput.actions.FindActionMap("Menu").FindAction("Move"));
    }

    private void CarryThrowable(Throwable throwable)
    {
        heldThrowable = throwable;
        heldThrowable.gameObject.SetActive(true);
        heldThrowable.transform.SetParent(throwPoint);
        heldThrowable.transform.rotation = Quaternion.identity;
        heldThrowable.transform.localPosition = Vector3.zero;
        heldThrowable.GetComponent<Rigidbody>().isKinematic = true;
        heldThrowable.Interact();
    }

    private void HandleRotation(Vector2 lookInput)
    {
        if (berryPouchIsOpen) return;

        verticalRotation = Mathf.Clamp(verticalRotation - (lookInput.y * gameSettings.LookSensitivity * rotationSpeed), -maximumViewAngle, maximumViewAngle);
        horizontalRotation += lookInput.x * gameSettings.LookSensitivity * rotationSpeed;
    }
    private void HandleInteract()
    {
        System.Type stateType = stateMachine.CurrentState.GetType();

        if ((stateType != typeof(WalkingState) && stateType != typeof(FallingState) && stateType != typeof(JumpingState)) || playerInput.currentActionMap.name != "Overworld")
        {
            onInteractableOutOfRange?.Invoke();
            return;
        }

        interactableInRange = null;

        if (Physics.Raycast(transform.position + Vector3.up * interactHeight, transform.forward, out RaycastHit climb, climbDistance, interactionLayers))
        {
            if (climb.transform.TryGetComponent(out JellyfishLadder ladder) && climbingUnlocked)
            {
                ladder.ContactPoint = climb.point;
                interactableInRange = ladder;
                onInteractableFound?.Invoke(interactableInRange.InteractionPrompt, climb.transform.position);
                climbControlImage.sprite = climbEnabledSprite;
                return;
            }
            else
            {
                climbControlImage.sprite = climbDisabledSprite;
            }
        }

        if (Physics.Raycast(transform.position + Vector3.up * interactHeight, transform.forward, out RaycastHit mural, readingDistance, interactionLayers))
        {
                if (mural.transform.TryGetComponent(out InteractableDialogue muralText))
                {
                    interactableInRange = muralText;
                    onInteractableFound?.Invoke(interactableInRange.InteractionPrompt, mural.transform.position);
                }

        }
        
        Collider[] collisions = Physics.OverlapSphere(transform.position + transform.forward * interactionDistance + Vector3.up * interactHeight, interactionRadius, interactionLayers);
        Collider closest = null;
        if (collisions.Length > 0)
        {
            foreach (Collider c in collisions)
            {
                // First, we check if the collisions we found can actually be seen from the player's perspective and aren't obscured by another object
                Vector3 interactOrigin = transform.position + Vector3.up * interactHeight;
                if (Physics.Raycast(interactOrigin, c.transform.position - interactOrigin, out RaycastHit hit, interactionDistance, interactionLayers))
                {
                    if (hit.transform.gameObject != c.gameObject)
                    {
                        continue;
                    }
                }
                if (c.TryGetComponent(out IInteractable interactable))
                {
                    if (interactable.GetType() == typeof(JellyfishLadder)) continue;

                    if (closest == null || Vector3.Distance(c.transform.position, transform.position) < Vector3.Distance(closest.transform.position, transform.position))
                    {
                        closest = c;
                        interactableInRange = interactable;
                    }
                }
            }
        }

        if (interactableInRange != null)
        {
            onInteractableFound?.Invoke(interactableInRange.InteractionPrompt, closest.transform.position);
            return;
        }
        onInteractableOutOfRange?.Invoke();
    }

    private void UnlockClimb()
    {
        climbingUnlocked = true;
        onClimbingUnlocked?.Invoke();
        GrandTemple.OnRingExtended -= UnlockClimb;
    }
    private void UnlockPouch()
    {
        pouchUnlocked = true;
        pouch.Unlock();
        onPouchUnlocked?.Invoke();
        GrandTemple.OnRingExtended -= UnlockPouch;
        GrandTemple.OnRingExtended += UnlockClimb;
        LinkModuleToOverworld();
    }
    private IEnumerator Die()
    {
        died = true;
        rb.velocity = Vector3.zero;
        verticalRotation = 0;
        //verticalSpeed = -0.5f;

        GameObject canvas = GetComponentInChildren<Canvas>().gameObject;
        canvas.SetActive(false);

        StartCoroutine(deathScreen.GetComponent<RandomMessage>().FadeIn(respawnDuration * 0.1f));
        deathScreen.SetActive(true);

        GetComponent<PlayerCamera>().DeleteCameraRoll();

        Material fadeMaterial = respawnFadeRenderer.material;
        Color fadeColor = fadeMaterial.color;

        float timer = 0.001f;

        // Fade in vision obscurer, move player, then fade it out again
        while (timer < respawnDuration * 0.3f)
        {
            fadeColor.a = Mathf.InverseLerp(0, 0.3f * respawnDuration, timer);
            fadeMaterial.color = fadeColor;
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = respawnTransform.position;

        onCameraClosed?.Invoke();
        onScrapbookClosed?.Invoke();

        StartCoroutine(deathScreen.GetComponent<RandomMessage>().FadeOut(respawnDuration * 0.1f, respawnDuration * 0.6f));

        while (timer < respawnDuration)
        {
            fadeColor.a = Mathf.InverseLerp(respawnDuration, 0.6f * respawnDuration, timer);
            fadeMaterial.color = fadeColor;

            timer += Time.deltaTime;
            yield return null;
        }

        deathScreen.SetActive(false);

        canvas.SetActive(true);
        died = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.25f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + (transform.forward * interactionDistance) + Vector3.up * interactHeight, interactionRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * drowningHeight, 0.2f);
    }
}
