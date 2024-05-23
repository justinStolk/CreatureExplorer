using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiramisu : Creature
{
    protected override void Start()
    {        
        base.Start();
        surroundCheck -= TiltWithGround;
    }
}
