using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private float offsetDistance = 0.5f;
    [SerializeField] private TextMeshProUGUI textMesh;

    private LineRenderer line;

    private void Awake()
    {
        if (!gameObject.TryGetComponent(out line))
            line = gameObject.AddComponent<LineRenderer>();
        gameObject.SetActive(false);
    }

    public void Activate(string prompt, Vector3 interactionOrigin)
    {
        textMesh.text = prompt;
        
        if (prompt == "")
        {
            gameObject.SetActive(false);
            return;
        }
        else if (VRChecker.IsVR)
        {
            transform.position = interactionOrigin;
            transform.LookAt(Camera.main.transform.position, Vector3.up);

            Vector3 uiOffset = (Camera.main.transform.position - transform.position).normalized + Vector3.up* offsetDistance;
            transform.position += uiOffset;
            transform.LookAt(Camera.main.transform.position, Vector3.up);

            Vector3[] linePoints = { interactionOrigin, transform.position };
            line.SetPositions(linePoints);
        }

        gameObject.SetActive(true);
    }
}
