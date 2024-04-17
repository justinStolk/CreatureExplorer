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
    [SerializeField] private UnityEvent onBookGesture;
    [SerializeField] private UnityEvent onFaint;
    [SerializeField] private UnityEvent onGrab;
    [SerializeField] private UnityEvent onRelease;

    [Header("General Settings")]
    [SerializeField] private UnityEngine.XR.XRNode handSource;
    [SerializeField] private VRHandController otherHand;
    //[SerializeField] private LayerMask PointingInteractionLayers;

    [field: HideArrow, SerializeField] private bool faintSettings;
    [field: ConditionalHide("faintSettings", true), SerializeField] private float faintingPalmAngle = 170;
    [field: ConditionalHide("faintSettings", true), SerializeField]  private float sqrDistanceHandToForehead = 0.05f;
    [field: ConditionalHide("faintSettings", true), SerializeField] private float secondsToFaint = 5;
    [field: ConditionalHide("faintSettings", true), SerializeField] private Volume volume;

    [field: HideArrow, SerializeField] private bool palmLookSettings;
    [field: ConditionalHide("palmLookSettings", true), SerializeField] private float lookAtPalmAngle = 45;
    [field: ConditionalHide("palmLookSettings", true), SerializeField] private float lookFromPalmAngle = 60;

    [field: HideArrow, SerializeField] private bool palmParallelSettings;
    [field: ConditionalHide("palmParallelSettings", true), SerializeField] private float palmAlignmentAccuracy = 0.8f;
    [field: ConditionalHide("palmParallelSettings", true), SerializeField] private float handUpAccuracy = 0.9f;
    [field: ConditionalHide("palmParallelSettings", true), SerializeField] private float sqrMaxHandDistance = 0.3f;
    [field: ConditionalHide("palmParallelSettings", true), SerializeField] private float sqrMinHandDistance = 0.15f;
    [field: ConditionalHide("palmParallelSettings", true), SerializeField] private float bookOpenAngle = 30;

    [field: HideArrow, SerializeField] private bool grabSettings;
    [field: ConditionalHide("grabSettings", true), SerializeField] private Vector3 grabOffset;
    [field: ConditionalHide("grabSettings", true), SerializeField] private float grabRadius;

    private UnityEngine.XR.InputDevice handDevice;

    private Transform cameraTransform;
    private bool lookingAtPalm = false;
    private bool palmsParallel = false;
    private bool checkBookOpening = false;

    private bool holding = false;
    private bool grabbing = false;

    private bool recievedGripInput;
    //private bool recievedTriggerInput;
    //private bool holdingTriggerInput;

    public IGrabbable grabbedObj { get; private set; }

    private float faintingTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = Camera.main.transform;

        handDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(handSource);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckHandOrientation();

        if (checkBookOpening)
        {
            CheckForBookGesture();
        }
    }

    private void LateUpdate()
    {
        if ((grabbing && !recievedGripInput) || (holding && grabbedObj == null))
        {
            ReleaseGrip();
        }

        /*
        if (holdingTriggerInput && !recievedTriggerInput)
        {
            holdingTriggerInput = false;
        }
        */
        recievedGripInput = false;
        //recievedTriggerInput = false;
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
            if (handRotationAngle < lookAtPalmAngle && onLookAtPalm.GetPersistentEventCount() > 0)
            {
                lookingAtPalm = true;
                onLookAtPalm?.Invoke();
            }
            // if there is an event to be called when palms are parallel, check whether palms are parallel
            else if (onPalmsParallel.GetPersistentEventCount()>0)
            {
                float handDistance = (transform.position - otherHand.transform.position).sqrMagnitude;
                if (HandsAligned() && Vector3.Dot(transform.forward, Vector3.up) > palmAlignmentAccuracy && handDistance < sqrMaxHandDistance && handDistance > sqrMinHandDistance)
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
            float handDistance = (transform.position - otherHand.transform.position).sqrMagnitude;
            if (!HandsAligned(0.7f) || handDistance > sqrMaxHandDistance || handDistance < sqrMinHandDistance)
            {
                palmsParallel = false;

                onPalmsUnaligned?.Invoke();
            }
        }
    }

    private void CheckForBookGesture()
    {
        if (Vector3.Angle(transform.right, otherHand.transform.right) > bookOpenAngle)
        {
            onBookGesture.Invoke();
            checkBookOpening = false;
            lookingAtPalm = true;
        }
    }

    public void BookOpenCheck(bool shouldCheck)
    {
        checkBookOpening = shouldCheck;
    }

    /*
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
    */

    public void PressGrip(InputAction.CallbackContext callbackContext)
    {
        recievedGripInput = true;
 
        if (!grabbing)
        {
            grabbing = true;

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

            if (grabbedObj != null)
            {
                grabbedObj.Release();
                onRelease.Invoke();
            }

            grabbedObj = null;
            holding = false;
        }
    }

    public void DoHaptics(float duration)
    {
       handDevice.SendHapticImpulse(0, 1, duration);
    }

    public void DoHaptics(float strength, float duration)
    {
       handDevice.SendHapticImpulse(0, strength, duration);
    }

    public void StopHaptics()
    {
        handDevice.StopHaptics();
    }

    private bool HandsAligned(float angleMultiplier = 1)
    {
        return (Vector3.Dot(transform.up, otherHand.transform.up) > palmAlignmentAccuracy * angleMultiplier && 
            Vector3.Dot(transform.forward, cameraTransform.up) > handUpAccuracy * angleMultiplier && 
            Vector3.Dot(otherHand.transform.forward, cameraTransform.up) > handUpAccuracy * angleMultiplier);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent(out Button button))
        {
            DoHaptics(0.4f);
            button.onClick.Invoke();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        GizmoDrawer.DrawPrimitive(transform.position + grabOffset, Vector3.one * grabRadius, GizmoType.WireSphere, Color.blue);
    }
#endif
}
