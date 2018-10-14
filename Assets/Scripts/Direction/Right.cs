using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Right : Direction {

    public static Right I = new Right();

    public override byte Serialize() {
        return 1;
    }

    public override Quaternion GetHeadRotation() {
        return Quaternion.Euler(0, 90, 0);
    }

    public override Vector3 GetMoveVector() {
        return new Vector3(1, 0, 0);
    }

}
