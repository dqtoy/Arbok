using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Up : Direction 
{

    public static Up I = new Up();

    public override Vector3 GetMoveVector()
    {
        return new Vector3(0, 0, 1);
    }
}
