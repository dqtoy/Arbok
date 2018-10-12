using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Left : Direction 
{
    public static Left I = new Left();

    public override Quaternion GetHeadRotation()
    {
        return Quaternion.Euler(0, 270, 0);
    }

    public override Vector3 GetMoveVector()
    {
        return new Vector3(-1, 0, 0);
    }
}
