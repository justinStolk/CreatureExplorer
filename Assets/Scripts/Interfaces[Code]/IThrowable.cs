using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IThrowable: IGrabbable
{
    new public void Grab(Transform handTransform);
    new public void Release();
}
