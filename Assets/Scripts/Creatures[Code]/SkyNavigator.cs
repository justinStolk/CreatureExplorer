using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyNavigator : MonoBehaviour
{
    [Header("Gizmos")]
    [SerializeField] protected Color gizmoColour;

    [field: Header("Movement")]
    [field:SerializeField] public float speed { get; private set; }
    [field: SerializeField] public float angularSpeed { get; private set; }
    [field: SerializeField] public float acceleration { get; private set; }
    public Vector3 Destination { get; private set; }

    [field: Header("Flying")]
    [SerializeField] private float minimumFlyHeight;
    [SerializeField] private float maximumFlyHeight;
    [SerializeField] private float maxAscentionAngle;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private List<Vector3> path;

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

        maxYIncline = Mathf.Sin(Mathf.Deg2Rad* maxAscentionAngle);
    }

    private void FixedUpdate()
    {
        if (path.Count < 1)
        {
            currentVelocity = 0;
            return;
        }

        Vector3 pathToNode = path[0] - transform.position;

        if(pathToNode.magnitude < 2 * speed * Time.deltaTime)
        {
            currentVelocity = Mathf.Lerp(currentVelocity, 0, 0.8f);
        }
        else if (currentVelocity < speed)
        {
            currentVelocity += acceleration * Time.deltaTime;
            Mathf.Clamp(currentVelocity, 0, speed);
        }

        if (pathToNode.magnitude < 1 * currentVelocity * Time.deltaTime || (pathToNode.normalized - moveDirection).magnitude > 0.8f)
        {
            //transform.position = path[0];

            path.RemoveAt(0);

            if (path.Count > 0)
            {
                moveDirection = (path[0]-transform.position).normalized;
                //moveDirection = new Vector3(moveDirection.x, Mathf.Clamp(moveDirection.y, -maxYIncline, maxYIncline), moveDirection.z);
            }
        }
        else
        {
            transform.Translate(transform.forward * currentVelocity * Time.deltaTime, Space.World);
            TurnAgent();
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
            Checkheight(destination, out Vector3 newDestination);
            Destination = newDestination;
        }
        else
        {
            Destination = destination;
        }
        
        CalculatePath(stayAirborne);
    }

    public float PathLength()
    {
        if (path.Count < 1 || path == null)
            return 0;

        float result = 0;
        int x = 0;
        result += (transform.position - path[0]).magnitude;

        while (x < path.Count-1)
        {
            result += (path[x] - path[x+1]).magnitude;
            x++;
        }
        // TODO: return for full length
        return result;
    }

    private void TurnAgent()
    {
        if (moveDirection != Vector3.zero)
        {
            Vector3 lookDirection = Vector3.Slerp(transform.forward, moveDirection, angularSpeed*Time.deltaTime);

            transform.LookAt(transform.position + lookDirection);
        }
    }

    // TODO: generate path that avoids obstacles
    private void CalculatePath(bool stayAirborne = true)
    {
        path.Clear();


        moveDirection = (Destination - transform.position).normalized;
        if (Mathf.Abs(moveDirection.y) > maxYIncline)
        {
            moveDirection = new Vector3(moveDirection.x, Mathf.Clamp(moveDirection.y, -maxYIncline, maxYIncline), moveDirection.z);

            // TODO: fly up in circles until desired height is reached
            path.Add(transform.position + moveDirection * (transform.position - Destination).magnitude);
        }
        else
        {
            path.Add(Destination);
        }
    }

    private void Checkheight(Vector3 checkPosition, out Vector3 newHeight)
    {
        float heightFromTerrain = Terrain.activeTerrain.SampleHeight(checkPosition);
        if (checkPosition.y < heightFromTerrain)
        {
            newHeight = new Vector3(checkPosition.x, heightFromTerrain + minimumFlyHeight, checkPosition.z);
        }
        else if (Physics.RaycastNonAlloc(checkPosition, Vector3.down, rayHit, minimumFlyHeight, groundLayer) > 0)
        {
            //Debug.Log($"checkedpoint: {checkPosition}, height from ground: {rayHit[0].distance}, adjusted height {minimumFlyHeight - rayHit[0].distance}, new position: {checkPosition + new Vector3(0, minimumFlyHeight - rayHit[0].distance, 0)}");
            checkPosition += Vector3.up * ((checkPosition.y - rayHit[0].distance) + minimumFlyHeight);
            newHeight = checkPosition;
        }
        // TODO: set a max height
        else if (checkPosition.y > maximumFlyHeight)
        {
            newHeight = new Vector3(checkPosition.x, maximumFlyHeight, checkPosition.z);
        }
        else
        {
            newHeight = checkPosition;
        }
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && path.Count > 0)
        { 
            GizmoDrawer.DrawPrimitive(transform.position, moveDirection, GizmoType.Line, gizmoColour);
        }
    }
#endif
}
