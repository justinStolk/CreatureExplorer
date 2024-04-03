using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour, IDialogue
{
    public static System.Action OnDialogueTriggered;
    [SerializeField] private bool triggerEvent;

    [SerializeField] private string[] dialogueText;
    [SerializeField] private bool showOnce;
    [SerializeField] private bool hideOnExit;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColour = Color.cyan;
    [SerializeField] private bool drawSolid = true;

    [ShowOnly] private bool hasBeenShown;

    public string[] DialogueText => dialogueText;

    private void OnTriggerEnter(Collider other)
    {
        if ((!showOnce || (showOnce && !hasBeenShown)) && (other.TryGetComponent(out PlayerCamera player) || other.GetComponentInChildren<PlayerCamera>() !=  null))
        {
            if (triggerEvent)
            {
                OnDialogueTriggered.Invoke();
            }

            DialogueUI.ShowText(dialogueText);
            hasBeenShown = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hideOnExit && other.TryGetComponent(out PlayerCamera player))
        {
            DialogueUI.HideText();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Color originalGizmoColour = Gizmos.color;
        Gizmos.color = gizmoColour;

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            if (drawSolid)
            {
                Gizmos.DrawCube(Vector3.zero, collider.bounds.size);
            }
            else 
            { 
                Gizmos.DrawWireCube(Vector3.zero, collider.bounds.size); 
            }
        }

        Gizmos.color = originalGizmoColour;
        Gizmos.matrix = originalMatrix;
    }
#endif
}
