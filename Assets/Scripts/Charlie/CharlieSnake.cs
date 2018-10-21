using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CharlieSnake : NetworkBehaviour {

    public float movesPerSecond = 5;

    InvisibleHead invisibleHead;
    SnakeHead snakeHead;
    SnakeTailContainer tailContainer;
    Direction currentDirection = Up.I;
    float elapsedTime = 0;

    private void Awake() {
        invisibleHead = GetComponentInChildren<InvisibleHead>();
        snakeHead = GetComponentInChildren<SnakeHead>();
        tailContainer = GetComponentInChildren<SnakeTailContainer>();
    }

    public override void OnStartLocalPlayer() {
        MainCamera.I.target = snakeHead.transform;
    }

    // Update is called once per frame
    void Update () {
        if (isLocalPlayer) {
            ProcessInput();

            elapsedTime += Time.deltaTime;
            if (elapsedTime > (1 / movesPerSecond)) {
                elapsedTime -= 1 / movesPerSecond;
                Tick();
            }
        }
        else {
            AlignWithInvisibleHead();
        }
    }

    void ProcessInput() {
        var newDirection = currentDirection;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) { newDirection = Up.I; }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { newDirection = Right.I; }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) { newDirection = Down.I; }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) { newDirection = Left.I; }

        CmdSyncDirection(newDirection.Serialize());

        if (!invalidTurn(currentDirection, newDirection)) {
            currentDirection = newDirection;
        }

        snakeHead.SetRotationOfVisual(currentDirection.GetHeadRotation());
    }

    void Tick() {
        var oldHeadPos = invisibleHead.transform.position;
        invisibleHead.Move(currentDirection);
        snakeHead.Move(currentDirection);
        tailContainer.Move(oldHeadPos);
    }

    void AlignWithInvisibleHead() {
        snakeHead.SetPos(invisibleHead.transform.position);
        tailContainer.AlignWithHeadPosition(snakeHead.transform.position, currentDirection);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.HasComponent<NetworkApple>()) {
            var apple = other.gameObject.GetComponent<NetworkApple>();
            apple.SetVisible(false);
            apple.DestroyOnServer();
            CmdGrow();
        }
    }

    // ==============================
    // Network
    // ==============================
    [Command]
    private void CmdSyncDirection(byte newDirectionAsByte) {
        RpcSyncDirection(newDirectionAsByte);
    }

    [ClientRpc]
    public void RpcSyncDirection(byte newDirectionAsByte) {
        if (!isLocalPlayer) {
            currentDirection = Direction.Deserialize(newDirectionAsByte);
        }
    }

    [Command]
    private void CmdGrow() {
        RpcGrow();
    }

    [ClientRpc]
    public void RpcGrow() {
        if (isLocalPlayer) {
            tailContainer.GrowForLocalPlayer();
        }
        else {
            tailContainer.GrowForRemotePlayer(snakeHead.transform.position);
        }
    }

    // ==============================
    // Helper method
    // ==============================
    private static bool invalidTurn(Direction currentDir, Direction newDir) {
        if ((newDir == Down.I && currentDir == Up.I) ||
            (newDir == Left.I && currentDir == Right.I) ||
            (newDir == Up.I && currentDir == Down.I) ||
            (newDir == Right.I && currentDir == Left.I)) {

            return true;
        }
        else {
            return false;
        }
    }
}
