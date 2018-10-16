using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyDirection : Direction {
    public static DummyDirection I = new DummyDirection();

    public override byte Serialize() {
        return byte.MaxValue;
    }

    public override Quaternion GetHeadRotation() {
        throw new NotImplementedException();
    }

    public override Vector3 GetMoveVector() {
        throw new NotImplementedException();
    }
}
