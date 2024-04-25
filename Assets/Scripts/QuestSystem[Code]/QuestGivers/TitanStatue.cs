using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TitanStatue : MonoBehaviour, IInteractable
{
    [field: Header("Interact")]
    [field: SerializeField] public string InteractionPrompt { get; private set; } = "Interact";
    [field: SerializeField] private float InteractHandTimer = 2;
    [SerializeField] private MeshRenderer interactRenderer;

    [field: Header("Quest")]
    [field: SerializeField] public Quest TitanQuest { get; private set; }
    [SerializeField] private UnityEvent onQuestCompleted;

    [SerializeField] private Material debugSwapMaterial;

    private bool questFinished = false;

    private float interactTime;

#if UNITY_EDITOR
    private void Start()
    {
        if (TitanQuest == null)
        {
            Debug.LogError("No quest found for: " + gameObject.name);
            this.enabled = false;
        }
    }
#endif


    public void Interact()
    {
        if (questFinished) return;
        /*
        Cursor.lockState = CursorLockMode.Confined;

        // Move this to the player and subscribe to the QuestHandler's event there.
        //input.SwitchCurrentActionMap("Scrapbook");

        //questInfoText.gameObject.SetActive(true);
        //questShowButton.gameObject.SetActive(true);

        StaticQuestHandler.OnPictureDisplayed += ShowPicture;
        StaticQuestHandler.CurrentQuestStatue = this;
        StaticQuestHandler.OnQuestClosed += () => 
        { 
            PagePicture.OnPictureClicked -= StaticQuestHandler.OnPictureClicked.Invoke;
            StaticQuestHandler.OnPictureDisplayed -= ShowPicture;
        };

        PagePicture.OnPictureClicked += StaticQuestHandler.OnPictureClicked.Invoke;

        StaticQuestHandler.OnQuestOpened?.Invoke();
        */

        DialogueUI.ShowText(TitanQuest.QuestDescription);
    }

    public void ShowPicture(PagePicture picture)
    {
        // Evaluate whether any of the objects in the picture info is the object that we're looking for/
        // Also check if there are additional conditions and evaluate these too.
        if (TitanQuest.EvaluateQuestStatus(picture.PictureInfo))
        {
            StaticQuestHandler.OnShrineCompleted?.Invoke();

            // Will be removed when correct visual feedback is implemented
            questFinished = true;
            InteractionPrompt = string.Empty;
            DebugChangeMaterialVisuals();

            Cursor.lockState = CursorLockMode.Locked;
            onQuestCompleted?.Invoke();

            PagePicture.OnPictureClicked = null;
            return;
        }
        StaticQuestHandler.OnQuestFailed?.Invoke();
    }

    // Will be removed when correct visual feedback is implemented
    public void DebugChangeMaterialVisuals()
    {
        foreach(MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = debugSwapMaterial;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out VRHandController hand))
        {
            hand.DoHaptics(0.3f, Time.fixedDeltaTime);

            interactTime += Time.deltaTime;

            if (interactRenderer.material != null)
            {
                interactRenderer.material.color = Color.Lerp(Color.gray, Color.cyan, interactTime / InteractHandTimer);
            }

            if (interactTime > InteractHandTimer)
            {
                Interact();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out VRHandController hand))
        {
            interactTime = 0;
            hand.StopHaptics();

            if (interactRenderer.material != null)
            {
                interactRenderer.material.color = Color.gray;
            }
        }
    }
}
