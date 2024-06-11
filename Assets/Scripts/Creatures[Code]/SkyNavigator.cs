using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyNavigator : MonoBehaviour
{
    [Header("Gizmos")]
    [SerializeField] protected Color gizmoColour;

    public Vector3 Destination { get; private set; }

    [field: Header("Movement")]
    [field:SerializeField] public float speed { get; private set; }
    [field: SerializeField] public float angularSpeed { get; private set; }
    [field: SerializeField] public float acceleration { get; private set; }

    [field: Header("Flying")]
    [SerializeField] private float minimumFlyHeight;
    [SerializeField] private float maxAscentionAngle;
    [SerializeField] private LayerMask groundLayer;

    private List<Vector3> path;

    private float currentVelocity;
    private Vector3 moveDirection;
    private float maxYIncline;

    private float originalSpeed, originalRotationSpeed, originalAcceleration;

    private readonly RaycastHit[] rayHit = new RaycastHit[100];

    private void Awake()
    {
        path = new List<Vector3>();
        originalSpeed = speed;
        originalRotationSpeed = angularSpeed;
        originalAcceleration = acceleration;

        maxYIncline = Mathf.Sin(maxAscentionAngle);
    }

    private void FixedUpdate()
    {
        if (path.Count < 1)
        {
            currentVelocity = 0;
            return;
        }

        if (currentVelocity < speed)
        {
            currentVelocity += acceleration * Time.deltaTime;
        }

        if ((transform.position - path[0]).magnitude < 1 * currentVelocity * Time.deltaTime)
        {
            transform.position = path[0];

            path.RemoveAt(0);

            if (path.Count > 0)
            {
                moveDirection = (transform.position - path[0]).normalized;
                //moveDirection = new Vector3(moveDirection.x, Mathf.Clamp(moveDirection.y, -maxYIncline, maxYIncline), moveDirection.z);

                if (moveDirection != Vector3.zero && Mathf.Abs(moveDirection.y) > maxYIncline)
                    transform.forward = moveDirection;
            }
        }
        else
        {
            transform.Translate(moveDirection * currentVelocity * Time.deltaTime, Space.World);
        }
    }

    public void AlterSpeed(float multiplier)
    {
        speed *= multiplier;
        angularSpeed *= multiplier;
        acceleration *= multiplier;
    }

    public void ResetSpeed()
    {
        speed = originalSpeed;
        angularSpeed = originalRotationSpeed;
        acceleration = originalAcceleration;
    }

    public void SetDestination(Vector3 destination, bool stayAirborne = true)
    {
        if (stayAirborne)
        {
            Checkheight(destination, out destination);
        }
        Destination = destination;
        
        CalculatePath(stayAirborne);
    }

    // TODO: generate path that avoids obstacles
    private void CalculatePath(bool stayAirborne = true)
    {
        path.Clear();

        path.Add(Destination);

        moveDirection = (transform.position - path[0]).normalized;
        if (Mathf.Abs(moveDirection.y) > maxYIncline)
        {
            moveDirection = new Vector3(moveDirection.x, Mathf.Clamp(moveDirection.y, -maxYIncline, maxYIncline), moveDirection.z);

            // TODO: fly up in circles until desired height is reached
            path.Add(transform.position + moveDirection*(transform.position - path[0]).magnitude);
        }

        if (moveDirection != Vector3.zero)
            transform.forward = moveDirection;
    }

    private void Checkheight(Vector3 checkPosition, out Vector3 newHeight)
    {
        if (Physics.RaycastNonAlloc(checkPosition, Vector3.down, rayHit, minimumFlyHeight, groundLayer) > 0)
        {
            checkPosition += new Vector3(0, minimumFlyHeight - rayHit[0].distance, 0);
        }
        newHeight = checkPosition;
    }

#if UNITY_EDITOR
protected virtual void OnDrawGizmosSelected()
{
    GizmoDrawer.DrawPrimitive(transform.position, moveDirection*currentVelocity, GizmoType.Line, gizmoColour);
}
#endif
}
