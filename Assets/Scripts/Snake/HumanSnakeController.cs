using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSnakeController : MonoBehaviour, ISnakeController {

    Direction nextDirection = Up.I;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            nextDirection = Up.I;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            nextDirection = Right.I;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            nextDirection = Down.I;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            nextDirection = Left.I;
        }
    }

    public Direction GetDirection() {
        return nextDirection;
    }

    public Vector3 GetNextHeadPosition() {
        throw new NotImplementedException();
    }
}
