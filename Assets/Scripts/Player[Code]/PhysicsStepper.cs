using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsStepper : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float maxStepHeight = 0.3f;
    [SerializeField, Min(0.1f)] private float maxStepDistance = 0.2f;
    [SerializeField, Min(0.1f)] private float stepSmooth = 0.1f;

    [SerializeField] private LayerMask ignoredStepLayers;
    [SerializeField] private CapsuleCollider playerCollider;

    public void HandleStep(ref Rigidbody rigidbody, Vector3 direction)
    {
        Vector3 playerPosition = rigidbody.position + new Vector3(playerCollider.center.x, 0, playerCollider.center.z);

        if (Physics.Raycast(playerPosition, direction,maxStepDistance, ~ignoredStepLayers))
        {
            if (!Physics.Raycast(playerPosition + Vector3.up * maxStepHeight, direction, maxStepDistance, ~ignoredStepLayers))
            {
                rigidbody.MovePosition(rigidbody.position + stepSmooth * Time.fixedDeltaTime * Vector3.up);
            }
        }
    }

}
