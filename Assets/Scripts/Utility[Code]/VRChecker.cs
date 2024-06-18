using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRChecker : MonoBehaviour
{
    [Header("Debugging")]
    [field: HideArrow, SerializeField] private bool overwriteVRCheck;
    [field: ConditionalHide("overwriteVRCheck", true), SerializeField] private bool shouldBeVR;
    // TODO: set to false for flatscreen builds
    private static bool isVR = true;

    private void OnApplicationFocus(bool focus)
    {
        if (overwriteVRCheck)
        {
            isVR = shouldBeVR;
            return;
        }

        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);
        /*
        System.Collections.Generic.List<UnityEngine.XR.InputDevice> inputDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeadMounted, inputDevices);
        */
        isVR = inputDevices.Count > 0;
#if UNITY_EDITOR
        Debug.Log(isVR);
#endif
    }

    public static bool IsVR
    {
        get
        {
            return false;
            /*
            if (!isVR)
            {
                var inputDevices = new List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevices(inputDevices);
                
                //System.Collections.Generic.List<UnityEngine.XR.InputDevice> inputDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                //UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeadMounted, inputDevices);
                
                isVR = inputDevices.Count > 0;
#if UNITY_EDITOR
                Debug.Log(isVR);
#endif
            }*/
            //return isVR;
        }
    }
}
