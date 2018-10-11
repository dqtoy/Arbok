using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Down : Direction 
{
    public static Down I = new Down();

    public override Vector3 GetMoveVector()
    {
        return new Vector3(0, 0, -1);
    }
}
