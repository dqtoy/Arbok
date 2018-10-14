using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Direction {
    public static Direction Deserialize(short directionShort) {
        switch (directionShort) {
            case 0:
                return Up.I;
            case 1:
                return Right.I;
            case 2:
                return Down.I;
            case 3:
                return Left.I;
            default:
                throw new Exception("Invalid direction: " + directionShort);
        }
    }

    public abstract byte Serialize();

    public abstract Vector3 GetMoveVector();

    public abstract Quaternion GetHeadRotation();
}
