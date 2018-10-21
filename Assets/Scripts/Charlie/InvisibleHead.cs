using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleHead : MonoBehaviour {

    public void Move(Direction direction) {
        transform.position = transform.position + direction.GetMoveVector();
    }
}
