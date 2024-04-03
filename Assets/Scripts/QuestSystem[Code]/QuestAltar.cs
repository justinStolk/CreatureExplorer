using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestAltar : MonoBehaviour, IInteractable
{
    [field:Header("Interact")]
    [field: SerializeField] public string InteractionPrompt { get; private set; } = "Interact";
    [field: SerializeField] private float InteractHandTimer = 2;

    [field:Header("Main Quest Settings")]
    [field: SerializeField] public MainQuest AltarQuest { get; private set; }
    private string questPrompt { get => AltarQuest.QuestDescription;}

    [SerializeField] private Material debugSwapMaterial;
    [SerializeField] private UnityEvent onAltarCompleted;

    private bool altarActivated = false;
    private bool AltarFinished = false;

    private MeshRenderer meshRenderer;
    private float interactTime;

    private void Start()
    {
        TryGetComponent(out meshRenderer);
    }

    public void Interact()
    {
        if (AltarFinished) return;

        DialogueUI.ShowText(questPrompt);

        Activate();
    }

    public void Activate()
    {
        // TODO: add quest to quest tracker
        altarActivated = true;

        StaticQuestHandler.OnPictureInScrapbook += AddPicture;

        StaticQuestHandler.OnAltarActivated.Invoke(AltarQuest);
    }

    public void AddPicture(PagePicture picture)
    {
        if (!altarActivated || AltarQuest.HasBeenEvaluated(picture.PictureInfo.PicturePath) || AltarFinished)
            return;

        // To do: Evaluate whether any of the objects in the picture info is the object that we're looking for/
        // Also check if there are additional conditions and evaluate these too.
        if (AltarQuest.EvaluateQuestStatus(picture.PictureInfo))
        {
            AltarFinished = true;

            // Will be removed when correct visual feedback is implemented
            InteractionPrompt = string.Empty;
            DebugChangeMaterialVisuals();

            GrandTemple.OnAltarCleared?.Invoke();

            onAltarCompleted?.Invoke();

            //PagePicture.OnPictureClicked = null;
        }
        StaticQuestHandler.OnAltarProgress.Invoke(AltarQuest);
    }

    // Will be removed when correct visual feedback is implemented
    public void DebugChangeMaterialVisuals()
    {
        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = debugSwapMaterial;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out VRHandController hand))
        {
            interactTime += Time.deltaTime;

            if (meshRenderer.material != null)
            {
                meshRenderer.material.color = Color.Lerp(Color.gray, Color.cyan, interactTime / InteractHandTimer);
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

            if (meshRenderer.material != null)
            {
                meshRenderer.material.color = Color.gray;
            }
        }
    }

    private void OnApplicationQuit()
    {
        AltarQuest.Initialise();
    }
}
