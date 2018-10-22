using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour {

    GameObject visual;

    private void Awake() {
        visual = transform.Find("Visual").gameObject;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void SetRotationOfVisual(Quaternion quaternion) {
        visual.transform.rotation = quaternion;
    }

    public Quaternion GetRotationOfVisual() {
        return visual.transform.rotation;
    }
}
