using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedUIMove : MonoBehaviour
{
    [SerializeField] private Vector3 inPosition, outPosition;

    [SerializeField] private float moveDuration;
    [SerializeField] private AnimationCurve moveCurve;

    [SerializeField] private bool unfurled;

    private Transform targetTransform;

    private void Start()
    {
        if (targetTransform == null)
        {
            targetTransform = transform;
        }
    }

    public void Close()
    {
        if (!unfurled)
        {
            StopAllCoroutines(); 
            
            StartCoroutine(Move(targetTransform.localPosition, inPosition));
        }
    }

    public void Open()
    {
        if (unfurled)
        {
            StopAllCoroutines();

            StartCoroutine(Move(targetTransform.localPosition, outPosition));

            unfurled = !unfurled;
        }
    }

    public void ToggleMove()
    {
        StopAllCoroutines();

        if (unfurled)
            StartCoroutine(Move(targetTransform.localPosition, inPosition));
        else
            StartCoroutine(Move(targetTransform.localPosition, outPosition));

        unfurled = !unfurled;
    }

    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float timer = 0;

        while (timer <= moveDuration)
        {
            targetTransform.localPosition = Vector3.Lerp(from, to, moveCurve.Evaluate(timer / moveDuration));
            timer += Time.deltaTime;
            yield return null;
        }

        targetTransform.localPosition = to;
    }
}
