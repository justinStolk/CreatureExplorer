using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowRotate : MonoBehaviour
{
    [SerializeField] private float rotationSensitivity;
    [SerializeField] private Transform rotationTarget;

    
    void LateUpdate()
    {
        Vector3 rotatedDirection = transform.position - rotationTarget.position;
        float currentAngle = Vector3.SignedAngle(new Vector3(rotatedDirection.x, 0, rotatedDirection.z), new Vector3(rotationTarget.forward.x ,  0 , rotationTarget.forward.z), Vector3.up);

#if UNITY_EDITOR
        //Debug.Log($"Angle: {currentAngle}, rotated direction: {rotatedDirection}, camRotation: {rotationTarget.rotation.y}");
#endif

        if (Mathf.Abs(currentAngle) > rotationSensitivity)
            transform.RotateAround(rotationTarget.position, transform.parent.up, (currentAngle * 0.1f));

        transform.LookAt(Camera.main.transform.position, Vector3.up);
        transform.Rotate(new Vector3(0, 180, 0));

        if (TryGetComponent(out FollowTarget target))
        {
            target.UpdateOffset();
        }
    }
}
