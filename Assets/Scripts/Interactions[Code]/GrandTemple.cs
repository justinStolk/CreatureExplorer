using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandTemple : MonoBehaviour
{
    public static GrandTemple Instance { get; private set; }
    
    public static System.Action OnAltarCleared;
    public static System.Action OnRingExtended;

    [SerializeField] private Animator templeAnimator;

    [SerializeField] private string[] firstStageDialogue, secondStageDialogue, thirdStageDialogue;

    [SerializeField] private string[] rewardAvailableDialogue;

    [SerializeField] private int firstStageRequirement = 2, secondStageRequirement = 5, thirdStageRequirement = 9;
    [SerializeField] private LayerMask playerLayer;

    private bool giveReward = false;

    private int AltarsCleared;

    private void Awake()
    {
        Instance = this;
        if (templeAnimator == null)
            templeAnimator = GetComponentInChildren<Animator>();

        OnAltarCleared += EvaluateExtention;
    }

    private void OnDestroy()
    {
        OnAltarCleared = null;
        OnRingExtended = null;
    }

    public void EvaluateExtention()
    {
        AltarsCleared++;


        if ((AltarsCleared == firstStageRequirement) || (AltarsCleared == secondStageRequirement) || (AltarsCleared == thirdStageRequirement))
        {
            if (Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius, playerLayer).Length > 0)
            {
                TriggerExtention();
            }
            else
            {
                giveReward = true;
                DialogueUI.ShowText(rewardAvailableDialogue);
            }
        }
    }

    // TODO: have altar give the reward?
    private void TriggerExtention()
    {
            giveReward = false;
            if (AltarsCleared == thirdStageRequirement)
            {
                OnRingExtended?.Invoke();
                templeAnimator.SetTrigger("FullyComplete");

                DialogueUI.ShowText(thirdStageDialogue);
            }
            else if (AltarsCleared >= secondStageRequirement)
            {
                OnRingExtended?.Invoke();
                templeAnimator.SetTrigger("2ndLayer");

                DialogueUI.ShowText(secondStageDialogue);
            }
            else if (AltarsCleared >= firstStageRequirement)
            {
                OnRingExtended?.Invoke();
                templeAnimator.SetTrigger("1stLayer");

                DialogueUI.ShowText(firstStageDialogue);
            }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (giveReward && other.TryGetComponent(out PlayerCamera player))
        {
            TriggerExtention();
        }
    }
}
