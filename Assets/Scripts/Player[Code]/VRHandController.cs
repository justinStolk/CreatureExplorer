using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;

public class VRHandController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent onLookAtPalm;
    [SerializeField] private UnityEvent onLookFromPalm;
    [SerializeField] private UnityEvent onPalmsParallel;
    [SerializeField] private UnityEvent onPalmsUnaligned;
    [SerializeField] private UnityEvent onFaint;
    [SerializeField] private UnityEvent onGrab;
    [SerializeField] private UnityEvent onRelease;

    [Header("Settings")]
    [SerializeField] private VRHandController otherHand;
    [SerializeField] private LayerMask PointingInteractionLayers;
    [Header("Fainting")]
    [SerializeField] private float faintingPalmAngle = 170;
    [SerializeField] private float sqrDistanceHandToForehead = 0.05f;
    [SerializeField] private float secondsToFaint = 5;
    [SerializeField] private Volume volume;
    [Header("Looking at palm")]
    [SerializeField] private float lookAtPalmAngle = 45;
    [SerializeField] private float lookFromPalmAngle = 60;
    [Header("Palms up and facing each other")]
    [SerializeField] private float palmAlignmentAccuracy = 0.8f;
    [SerializeField] private float handUpAccuracy = 0.9f;
    [SerializeField] private float sqrMaxHandDistance = 0.3f;
    [Header("Grabbing things")]
    [SerializeField] private Vector3 grabOffset;
    [SerializeField] private float grabRadius;

    private Transform cameraTransform;
    private bool lookingAtPalm = false;
    private bool palmsParallel = false;
    private bool holding = false;
    private bool grabbing = false;

    private bool recievedGripInput;
    private bool recievedTriggerInput;
    private bool holdingTriggerInput;
    private bool wasPointing;

    public IGrabbable grabbedObj { get; private set; }

    private float faintingTimer = 0;

    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;

        TryGetComponent(out line);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckHandOrientation();
        if (line != null)
            Point();

    }

    private void LateUpdate()
    {
        if ((grabbing && !recievedGripInput) || (holding && grabbedObj == null))
        {
            ReleaseGrip();
        }

        if (holdingTriggerInput && !recievedTriggerInput)
        {
            holdingTriggerInput = false;
        }

        recievedGripInput = false;
        recievedTriggerInput = false;
    }

    void CheckHandOrientation()
    {
        if (holding)
            return;

        float handRotationAngle = Vector3.Angle(transform.up, transform.position - cameraTransform.position);

        // check for fainting posture
        if (handRotationAngle > faintingPalmAngle && (transform.position - cameraTransform.position).sqrMagnitude < sqrDistanceHandToForehead)
        {
            faintingTimer += Time.deltaTime;

            if (volume.profile.TryGet(out Vignette vignette))
            {
                vignette.intensity.value = faintingTimer/secondsToFaint;
            }

            if (faintingTimer > secondsToFaint)
            {
                faintingTimer = 0;
                if (vignette != null)
                {
                    vignette.intensity.value = 0;
                }
                onFaint.Invoke();
            }
        }
        else if (faintingTimer != 0)
        {
            faintingTimer = 0;
            if (volume.profile.TryGet(out Vignette vignette))
            {
                vignette.intensity.value = 0;
            }
        }

        if (!lookingAtPalm && !palmsParallel)
        {
            // check for looking at palm posture
            if (handRotationAngle < lookAtPalmAngle)
            {
                lookingAtPalm = true;
                onLookAtPalm?.Invoke();
            }
            // if there is an event to be called when palms are parallel, check whether palms are parallel
            else if (onPalmsParallel.GetPersistentEventCount()>0)
            {
                if (HandsAligned() && Vector3.Dot(transform.forward, Vector3.up) > palmAlignmentAccuracy && (transform.position - otherHand.transform.position).sqrMagnitude < sqrMaxHandDistance)
                {
                    onPalmsParallel?.Invoke();
                    palmsParallel = true;
                }
            }
        }
        else if (lookingAtPalm)
        {
            if (handRotationAngle > lookFromPalmAngle)
            {
                lookingAtPalm = false;
                onLookFromPalm?.Invoke();
            }
        }
        else if (palmsParallel)
        {
            if (!HandsAligned(0.7f) || (transform.position - otherHand.transform.position).sqrMagnitude > sqrMaxHandDistance)
            {
                palmsParallel = false;
                onPalmsUnaligned?.Invoke();
            }
        }
    }

    private void Point()
    {
        if (line.enabled)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100, PointingInteractionLayers))
            {
                line.SetPosition(1, new Vector3(0, 0, hit.distance));

                //Debug.Log($"pointing at: {hit.collider.gameObject.name}");
                if (hit.collider.TryGetComponent(out Selectable uiElement))
                {
                    uiElement.Select();
                    return;
                }
            }
            else
            {
                line.SetPosition(1, new Vector3(0, 0, 10));
            }
        }
    }

    public void PressTrigger(InputAction.CallbackContext callbackContext)
    {
        recievedTriggerInput = true;

        if (!holdingTriggerInput)
        {
            holdingTriggerInput = true;
            if (line == null || !line.enabled)
                return;

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100, PointingInteractionLayers))
            {
                //Debug.Log($"hit {hit.collider.gameObject.name}");
                if (hit.collider.TryGetComponent(out Button button))
                {
                    button.onClick.Invoke();
                }
                else if (hit.collider.TryGetComponent(out PageComponent component))
                {
                    component.OnBeginDrag();
                }
            }
        }
    }

    public void PressGrip(InputAction.CallbackContext callbackContext)
    {
        recievedGripInput = true;
 
        if (!grabbing)
        {
            grabbing = true;

            wasPointing = line.enabled;
            line.enabled = false;

            //Debug.Log("grabbing");
            if (LookForObjects<IGrabbable>.TryGetClosestObject(transform.position + grabOffset, transform.position, grabRadius, out IGrabbable grabbable))
            {
                if (otherHand.grabbedObj == grabbable)
                {
                    otherHand.ReleaseGrip();
                }

                if (grabbedObj != null)
                    grabbedObj.Release();

                onGrab.Invoke();

                grabbedObj = grabbable;

                grabbedObj.Grab(transform);
                holding = true;
                return;
            }
        } 
    }

    public void ReleaseGrip()
    {
        if (grabbing)
        {
            grabbing = false;

            line.enabled = wasPointing;

            //Debug.Log("releasing");

            onRelease.Invoke();

            if (grabbedObj != null)
                grabbedObj.Release();

            grabbedObj = null;
            holding = false;
        }
    }

    private bool HandsAligned(float angleMultiplier = 1)
    {
        return (Vector3.Dot(transform.up, otherHand.transform.up) > palmAlignmentAccuracy * angleMultiplier && 
            Vector3.Dot(transform.forward, cameraTransform.up) > handUpAccuracy * angleMultiplier && 
            Vector3.Dot(otherHand.transform.forward, cameraTransform.up) > handUpAccuracy * angleMultiplier);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        GizmoDrawer.DrawPrimitive(transform.position + grabOffset, Vector3.one * grabRadius, GizmoType.WireSphere, Color.blue);
    }
#endif
}
