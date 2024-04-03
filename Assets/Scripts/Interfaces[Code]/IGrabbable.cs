using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbable 
{
    public void Grab(Transform handTransform);
    public void Release();
}
