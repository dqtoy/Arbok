using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour {

    public DG_Position position;

    GameObject visual;

    private void Awake() {
        visual = transform.Find("Visual").gameObject;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        position.x = (int) transform.position.x;
        position.y = (int) transform.position.z;
    }

    public void SetRotationOfVisual(Quaternion quaternion) {
        visual.transform.rotation = quaternion;
    }

    public Quaternion GetRotationOfVisual() {
        return visual.transform.rotation;
    }
}
