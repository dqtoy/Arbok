using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour {

    GameObject visual;

    private void Awake() {
        visual = transform.Find("Visual").gameObject;
    }

    public void SetPos(Vector3 newPos) {
        transform.position = newPos;
    }

    public void Move(Direction direction) {
        transform.position = transform.position + direction.GetMoveVector();
    }

    public void SetRotationOfVisual(Quaternion quaternion) {
        visual.transform.rotation = quaternion;
    }
}
