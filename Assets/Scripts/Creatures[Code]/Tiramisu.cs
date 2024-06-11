using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiramisu : Creature
{
    protected override void Start()
    {        
        base.Start();
        //surroundCheck -= TiltWithGround;
    }

    protected override void StartInvoking()
    {
        InvokeRepeating("UpdateValues", 0, 1);
        //InvokeRepeating("TiltWithGround", 0, data.GroundTiltTimer);

        if (surroundCheck != null)
            InvokeRepeating("LookAtSurroundings", 0, data.CheckSurroundingsTimer);
    }
}
