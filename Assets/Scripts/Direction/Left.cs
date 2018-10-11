using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Left : Direction 
{

    public static Left I = new Left();

    public override Vector3 GetMoveVector()
    {
        return new Vector3(-1, 0, 0);
    }
}
