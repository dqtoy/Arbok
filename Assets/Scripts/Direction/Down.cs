using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Down : Direction 
{
    public static Down I = new Down();

    public override Quaternion GetHeadRotation()
    {
        return Quaternion.Euler(0, 180, 0);
    }

    public override Vector3 GetMoveVector()
    {
        return new Vector3(0, 0, -1);
    }
}
