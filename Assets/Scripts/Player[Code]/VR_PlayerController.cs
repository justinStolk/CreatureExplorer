using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR;

public class VR_PlayerController : MonoBehaviour
{
    public static float Loudness { get; private set; } = 5;
    public InputSystemUIInputModule InputModule { get { return module; } }

    [field: HideArrow, SerializeField] private bool interactionSettings;
    [field: ConditionalHide("interactionSettings", true), SerializeField] private float maximumViewAngle = 70f;
    [field: ConditionalHide("interactionSettings", true), SerializeField] private float interactionDistance = 2f;
    [field: ConditionalHide("interactionSettings", true), SerializeField] private float interactionRadius = 1.25f;
    //[field: ConditionalHide("interactionSettings", true), SerializeField] private float climbDistance = 0.25f;
    [field: ConditionalHide("interactionSettings", true), SerializeField] private float readingDistance = 15f;

    [field: ConditionalHide("interactionSettings", true), SerializeField] private LayerMask interactionLayers;
    [SerializeField] private LayerMask waterLayer;

    [SerializeField] private GameSettings gameSettings;

    [Header("Events")]
    [SerializeField] private UnityEvent onScrapbookOpened;
    [SerializeField] private UnityEvent onScrapbookClosed;
    [SerializeField] private UnityEvent onCameraOpened;
    [SerializeField] private UnityEvent onCameraClosed;
    [SerializeField] private UnityEvent<string, Vector3> onInteractableFound;
    [SerializeField] private UnityEvent onInteractableOutOfRange;
    [SerializeField] private UnityEvent onScrapbookUnlocked;
    [SerializeField] private UnityEvent onPouchUnlocked;
    [SerializeField] private UnityEvent onClimbingUnlocked;
    [SerializeField] private UnityEvent onHurt;
    [SerializeField] private UnityEvent onBerryThrown;
    [SerializeField] private UnityEvent onBerryPickup;
    [SerializeField] private UnityEvent onDeath;

    [field: HideArrow, SerializeField] private bool respawnSettings;
    [field: ConditionalHide("respawnSettings", true), SerializeField] private float respawnDuration = 0.5f;
    [field: ConditionalHide("respawnSettings", true), SerializeField] private AnimationCurve respawnFade;
    [field: ConditionalHide("respawnSettings", true), SerializeField] private float drowningHeight = 1.2f;
    [field: ConditionalHide("respawnSettings", true), SerializeField] private GameObject uiCanvas;
    [field: ConditionalHide("respawnSettings", true), SerializeField] private Transform respawnTransform;
    [field: ConditionalHide("respawnSettings", true), SerializeField] private GameObject deathScreen;
    [field: ConditionalHide("respawnSettings", true), SerializeField] private GameObject respawnOccluder;

    [field: HideArrow, SerializeField] private bool upgradesSettings;
    [Tooltip("Serialized for testing purposes")]
    [field: ConditionalHide("upgradesSettings", true), SerializeField] private bool scrapbookUnlocked;
    [Tooltip("Serialized for testing purposes")]
    [field: ConditionalHide("upgradesSettings", true), SerializeField] private bool climbingUnlocked;
    [Tooltip("Only Serialized for testing purposes")]
    [field: ConditionalHide("upgradesSettings", true), SerializeField] private bool pouchUnlocked;

    [SerializeField] private InputSystemUIInputModule module;

    private bool died;
    
    private BerryPouch pouch;
    private bool berryPouchIsOpen;

    [SerializeField] private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    private FiniteStateMachine stateMachine;

    private Camera firstPersonCamera;
    private MeshRenderer respawnFadeRenderer;

    private PlayerInput playerInput;

    private IInteractable interactableInRange;
    private Throwable heldThrowable;

    // TODO: refactor?
    private string previousActionmap;

    private void Awake()
    {
        if (!module)
        {
            throw new System.Exception("No Input Module assigned, this will break the interface handling and should not be skipped!");
        }

        stateMachine = new FiniteStateMachine(typeof(WalkingState), GetComponents<IState>());
        firstPersonCamera = Camera.main;

        capsuleCollider = GetComponentInParent<CapsuleCollider>();

        respawnFadeRenderer = Instantiate(respawnOccluder, firstPersonCamera.transform).GetComponent<MeshRenderer>();
        deathScreen = Instantiate(deathScreen);
        deathScreen.GetComponent<Canvas>().worldCamera = firstPersonCamera;
        deathScreen.GetComponent<Canvas>().planeDistance = 0.3f;
        deathScreen.SetActive(false);

        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        pouch = GetComponentInChildren<BerryPouch>();

        // TODO: Change how upgrades are given
        //GrandTemple.OnRingExtended += UnlockPouch;
        DialogueTrigger.OnDialogueTriggered += UnlockNotebook;

        StaticQuestHandler.OnQuestOpened += () =>
        {
            LinkModule("Scrapbook");
            onInteractableOutOfRange?.Invoke();
        };
        StaticQuestHandler.OnQuestClosed += () =>
        {
            if (playerInput.currentActionMap.name != "Dialogue")
            {
                LinkModule("Overworld");
            }
            Cursor.lockState = CursorLockMode.Locked;
            stateMachine.SwitchState(typeof(WalkingState));
        };

        if (scrapbookUnlocked) UnlockNotebook();

#if UNITY_EDITOR
        //if (pouchUnlocked) UnlockPouch();

        //if (climbingUnlocked) UnlockClimb();
#endif
    }

    private void Start()
    {
        onCameraClosed?.Invoke();
    }

    // Update is called once per frame
    private void Update()
    {
        if (died) return;

        stateMachine.OnUpdate();
        HandleHeadsetMovement();
        HandleInteract();

        if (Physics.CheckSphere(transform.position + Vector3.up * drowningHeight, 0.2f, waterLayer))
        {
            StartCoroutine(Die());
        }
    }

    private void FixedUpdate()
    {
        stateMachine.OnFixedUpdate();
    }

    private void OnDestroy()
    {
        onScrapbookOpened.RemoveAllListeners();
        onScrapbookClosed.RemoveAllListeners();
        onCameraOpened.RemoveAllListeners();
        onCameraClosed.RemoveAllListeners();
        onInteractableFound.RemoveAllListeners();
        onInteractableOutOfRange.RemoveAllListeners();
        onPouchUnlocked.RemoveAllListeners();
        onClimbingUnlocked.RemoveAllListeners();
        onHurt.RemoveAllListeners();
        onBerryThrown.RemoveAllListeners();
        onBerryPickup.RemoveAllListeners();
        onDeath.RemoveAllListeners();

        GrandTemple.OnRingExtended = null;

        StaticQuestHandler.OnQuestOpened = null;
        StaticQuestHandler.OnQuestClosed = null;

        StaticQuestHandler.OnShrineCompleted = null;
        StaticQuestHandler.OnQuestCompleted = null;
        StaticQuestHandler.OnQuestFailed = null;

        StaticQuestHandler.OnPictureClicked = null;
        StaticQuestHandler.OnPictureDisplayed = null;

        StaticQuestHandler.OnAltarActivated = null;

        StaticQuestHandler.OnPictureInScrapbook = null;
        StaticQuestHandler.OnAltarProgress = null;
    }

    public void SwapToCamera(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            OpenCamera();
        }
    }

    public void OpenCamera() 
    {
        if (playerInput.currentActionMap.name != "Camera" && playerInput.currentActionMap.name != "Scrapbook")
        {
            LinkModule("Camera");
            onCameraOpened?.Invoke();
        }
    }

    public void SwapFromCamera(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            CloseCamera();
        }
    }

    public void CloseCamera()
    {
        if (playerInput.currentActionMap.name == "Camera")
        {
            LinkModule(previousActionmap);
            onCameraClosed?.Invoke();
        }
    }

    #region flatscreen interaction
    /*
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
            // TODO: change to wall?
            /*
            if (climbingUnlocked && interactableInRange.GetType() == typeof(JellyfishLadder))
            {
                JellyfishLadder ladder = interactableInRange as JellyfishLadder;
                onInteractableOutOfRange?.Invoke();
                stateMachine.SwitchState(typeof(ClimbingState));
                transform.SetParent(ladder.transform);
                return;
            }
            else if (interactableInRange.GetType() == typeof(JellyfishLadder))
            {
                onHurt?.Invoke();
            }
            */
    /*
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

    public void GetCloseQuestInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            playerInput.SwitchCurrentActionMap("Overworld");
            StaticQuestHandler.OnQuestClosed.Invoke();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // TODO: move to throwable script?
    /*
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
    */
    /*
    public void ReceiveRetrievedBerry(Throwable berry)
    {
        CarryThrowable(berry);
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

    private void CarryThrowable(Throwable throwable)
    {
        heldThrowable = throwable;
        heldThrowable.gameObject.SetActive(true);
        //heldThrowable.transform.SetParent(throwPoint);
        heldThrowable.transform.rotation = Quaternion.identity;
        heldThrowable.transform.localPosition = Vector3.zero;
        heldThrowable.GetComponent<Rigidbody>().isKinematic = true;
        heldThrowable.Interact();
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
        LinkModule("Overworld");
    }
    */
    #endregion


    public void GetCloseScrapbookInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            CloseScrapbook();
        }
    }

    public void CloseScrapbook()
    {
        if (playerInput.currentActionMap.name == "Scrapbook")
        {
            LinkModule(previousActionmap);
            //playerInput.SwitchCurrentActionMap("Overworld");
            //Cursor.lockState = CursorLockMode.Locked;
            onScrapbookClosed?.Invoke();
        }
    }

    public void GetOpenScrapbookInput(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            OpenScrapbook();
        }
    }

    public void OpenScrapbook()
    {
        if (playerInput.currentActionMap.name != "Scrapbook" && playerInput.currentActionMap.name != "Camera")
        {
            LinkModule("Scrapbook");
            //Cursor.lockState = CursorLockMode.None;
            onScrapbookOpened?.Invoke();
        }
    }

    public static void SetLoudness(float newLoudness) => Loudness = newLoudness;

    public void GoDie() => StartCoroutine(Die());

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

    public void LinkModule(string linkTo)
    {
        previousActionmap = playerInput.currentActionMap.name;
        playerInput.SwitchCurrentActionMap(linkTo);
        module.leftClick = InputActionReference.Create(playerInput.actions.FindActionMap(linkTo).FindAction("Click"));
        module.point = InputActionReference.Create(playerInput.actions.FindActionMap(linkTo).FindAction("Point"));
        module.move = InputActionReference.Create(playerInput.actions.FindActionMap(linkTo).FindAction("Move"));
    }

    // TODO: get rid of redundant code
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

    public void Teleport(Vector3 teleportTo)
    {
        rb.MovePosition(teleportTo);
    }

    private void HandleHeadsetMovement()
    {
        if (berryPouchIsOpen) return;

        Vector3 lookingForward = firstPersonCamera.transform.forward;
        lookingForward.y = 0;

        transform.forward = lookingForward.normalized;

        float heightDiff = Mathf.Abs(firstPersonCamera.transform.position.y - rb.transform.position.y);
        capsuleCollider.height = heightDiff;
        capsuleCollider.center = new Vector3(firstPersonCamera.transform.localPosition.x, heightDiff * 0.5f, firstPersonCamera.transform.localPosition.z);

        GetComponent<CrouchingState>().ToggleCrouch(heightDiff);
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
        Vector3 interactOrigin = firstPersonCamera.transform.position;

        // TODO: change to wall?
        /*
        if (Physics.Raycast(interactOrigin, firstPersonCamera.transform.forward, out RaycastHit climb, climbDistance, interactionLayers))
        {
            if (climb.transform.TryGetComponent(out JellyfishLadder ladder) && climbingUnlocked)
            {
                ladder.ContactPoint = climb.point;
                interactableInRange = ladder;
                onInteractableFound?.Invoke(interactableInRange.InteractionPrompt, climb.transform.position);
                return;
            }
        }
        */

        if (Physics.Raycast(interactOrigin, firstPersonCamera.transform.forward, out RaycastHit mural, readingDistance, interactionLayers))
        {
            if (mural.transform.TryGetComponent(out InteractableDialogue muralText))
            {
                interactableInRange = muralText;
                onInteractableFound?.Invoke(interactableInRange.InteractionPrompt, mural.transform.position);
                return;
            }
        }

        Collider[] collisions = Physics.OverlapSphere(interactOrigin + firstPersonCamera.transform.forward * interactionDistance, interactionRadius, interactionLayers);
        Collider closest = null;
        if (collisions.Length > 0)
        {
            foreach (Collider c in collisions)
            {
                // First, we check if the collisions we found can actually be seen from the player's perspective and aren't obscured by another object
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

    private void UnlockNotebook()
    {
        scrapbookUnlocked = true;
        onScrapbookUnlocked.Invoke();
        DialogueTrigger.OnDialogueTriggered -= UnlockNotebook;
    }

    // TODO: make into state?
    private IEnumerator Die()
    {
        died = true;
        rb.velocity = Vector3.zero;
        onDeath.Invoke();
        GetComponentInChildren<SoundPlayer>().enabled = false;

        //GameObject canvas = transform.root.GetComponentInChildren<Canvas>().gameObject;
        uiCanvas.SetActive(false);

        StartCoroutine(deathScreen.GetComponent<RandomMessage>().FadeIn(respawnDuration * 0.1f));
        deathScreen.SetActive(true);

        GetComponent<PlayerCamera>().DeleteCameraRoll();

        respawnFadeRenderer.gameObject.SetActive(true);
        Material fadeMaterial = respawnFadeRenderer.material;
        Color fadeColor = fadeMaterial.color;

        float timer = 0.001f;

        // Fade in vision obscurer, move player, then fade it out again
        while (timer < respawnDuration * 0.3f)
        {
            fadeColor.a = respawnFade.Evaluate(timer / respawnDuration);// Mathf.InverseLerp(0, 0.3f * respawnDuration, timer);
            fadeMaterial.color = fadeColor;
            timer += Time.deltaTime;
            yield return null;
        }

        rb.transform.position = respawnTransform.position;

        onCameraClosed?.Invoke();
        onScrapbookClosed?.Invoke();

        StartCoroutine(deathScreen.GetComponent<RandomMessage>().FadeOut(respawnDuration * 0.1f, respawnDuration * 0.5f));

        while (timer < respawnDuration)
        {
            fadeColor.a = respawnFade.Evaluate(timer / respawnDuration);//  Mathf.InverseLerp(respawnDuration, 0.6f * respawnDuration, timer);
            fadeMaterial.color = fadeColor;

            timer += Time.deltaTime;
            yield return null;
        }

        GetComponentInChildren<SoundPlayer>().enabled = true;

        deathScreen.SetActive(false);
        respawnFadeRenderer.gameObject.SetActive(false);

        uiCanvas.SetActive(true);
        died = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.25f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Camera.main.transform.position + Camera.main.transform.forward * interactionDistance, interactionRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * drowningHeight, 0.2f);
    }
#endif
}
