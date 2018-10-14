using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSnakeController : NetworkBehaviour {

    // Vector3 nextHeadPos;
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
            SendNewDirection(Up.I);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            SendNewDirection(Right.I);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            SendNewDirection(Down.I);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            SendNewDirection(Left.I);
        }
    }

    void SendNewDirection(Direction direction) {
        snake.ChangeDirectionAtNextTick(direction);
        Debug.Log("direction: " + direction.Serialize());
        CmdKeyDown(direction.Serialize(), snake.currentTick + 1);
        // SendHeadPosition();
    }

    // public void SendHeadPosition() {
    //     CmdSendHeadPosition(snake.head.transform.position);
    // }

    // ==============================
    // Network
    // ==============================
    [Command]
    private void CmdKeyDown(byte newDirection, int tick) {
        RpcKeyDown(newDirection, tick);
    }

    [ClientRpc]
    public void RpcKeyDown(byte newDirection, int tick) {
        if (!isLocalPlayer) {
            snake.ChangeDirectionAtTick(Direction.Deserialize(newDirection), tick);
        }
    }

    // [Command]
    // private void CmdSendHeadPosition(Vector3 headPosition) {
    //     RpcReceiveHeadPosition(headPosition);
    // }

    // [ClientRpc]
    // public void RpcReceiveHeadPosition(Vector3 headPosition) {
    //     if (!isLocalPlayer) {
    //         snake.head.transform.position = headPosition;
    //     }
    // }

    // ==============================
    // ISnakeController
    // ==============================

    // public Vector3 GetNextHeadPosition() {
    //     return nextHeadPos;
    // }
}
