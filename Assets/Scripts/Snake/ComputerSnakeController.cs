using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerSnakeController : MonoBehaviour, ISnakeController {

    Direction nextDirection = Up.I;

    Direction[] directionLoop = new Direction[] { Right.I, Down.I, Left.I, Up.I };
    int index = 0;
    public float secondsPerMove = 1f;

    // Use this for initialization
    void Start() {
        StartCoroutine(Foo());
    }

    // Update is called once per frame
    void Update() { }

    IEnumerator Foo() {
        yield return new WaitForSeconds(secondsPerMove);
        nextDirection = directionLoop[index];
        index++;
        if (index == directionLoop.Length) {
            index = 0;
        }
        StartCoroutine(Foo());
    }

    public Direction GetDirection() {
        return nextDirection;
    }

    public Vector3 GetNextHeadPosition() {
        throw new System.NotImplementedException();
    }
}
