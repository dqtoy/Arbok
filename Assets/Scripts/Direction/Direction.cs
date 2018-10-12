using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Direction 
{
    public abstract Vector3 GetMoveVector();

    public abstract  Quaternion GetHeadRotation();
}
