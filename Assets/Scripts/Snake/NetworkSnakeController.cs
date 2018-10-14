using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSnakeController : NetworkBehaviour, ISnakeController {

    Direction nextDirection = Up.I;
    Vector3 nextHeadPos;
    Snake snake;

    // Use this for initialization
    void Start() {
        snake = GetComponent<Snake>();
    }

    // Update is called once per frame
    void Update() {
        if (!isLocalPlayer) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            CmdKeyDown(Up.I.Serialize());
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            CmdKeyDown(Right.I.Serialize());
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            CmdKeyDown(Down.I.Serialize());
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            CmdKeyDown(Left.I.Serialize());
        }
    }

    // ==============================
    // Network
    // ==============================
    [Command]
    private void CmdKeyDown(byte newDirection) {
        RpcKeyDown(newDirection);
    }

    [ClientRpc]
    public void RpcKeyDown(byte newDirection) {
        nextDirection = Direction.Deserialize(newDirection);
    }

    // ==============================
    // ISnakeController
    // ==============================
    public Direction GetDirection() {
        return nextDirection;
    }

    public Vector3 GetNextHeadPosition() {
        return nextHeadPos;
    }
}
