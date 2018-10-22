using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkSnakeController : NetworkBehaviour {
    public Text netIdUI;

    public string[] receivedEvents = new string[0];

    Snake snake;

    public void Awake() {
        snake = GetComponent<Snake>();
    }

    public void Start() {
        netIdUI.text = netId.ToString();
    }

    public override void OnStartLocalPlayer() {
        CmdRequestSnakePositions();
        netIdUI.text = netId.ToString() + "*";
        MainCamera.I.target = snake.cameraTarget;
        CmdRequestApplePositions();
    }

    [Command]
    public void CmdRequestApplePositions() {
        Toolbox.Log("CmdRequestApplePositions");
        TargetReceiveApplePositions(
            connectionToClient,
            JsonConvert.SerializeObject(AppleManager.all.Select(x => x.ToState()).ToArray())
        );
    }

    [TargetRpc]
    public void TargetReceiveApplePositions(NetworkConnection connection, string appleStatesJson) {
        Toolbox.Log("TargetReceiveApplePositions: isServer: " + isServer);
        if (!isServer) {
            AppleManager.DestroyAll();
            JsonConvert.DeserializeObject<AppleState[]>(appleStatesJson).ToList().ForEach(apple => {
                AppleManager.I.SpawnApple(apple);
            });
        } else {
            AppleManager.EnableAll();
        }
    }

    [Command]
    public void CmdRequestSnakePositions() {
        Snake.all.ForEach(
            x => {
                var linksJson = JsonConvert.SerializeObject(x.links.Select(y => y.transform.position).ToArray());
                TargetReceiveSnakePosition(
                    connectionToClient,
                    x.head.transform.position,
                    x.currentDirection.Serialize(),
                    linksJson,
                    x.GetComponent<NetworkIdentity>().netId
                );
            }
        );
    }

    [TargetRpc]
    public void TargetReceiveSnakePosition(NetworkConnection connection, Vector3 position, short direction, string linksJson, NetworkInstanceId netId) {
        if (netId == this.netId) return;

        var snakeToModify = Snake.all.First(x => x.GetComponent<NetworkIdentity>().netId == netId);

        snakeToModify.SetSnakeData(new SnakeState() {
            linkPositions = JsonConvert.DeserializeObject<Vector3[]>(linksJson),
                headPosition = position,
                direction = Direction.Deserialize(direction)
        });
    }

    void Update() {
        if (!isLocalPlayer) return;

        if (isLocalPlayer && Input.GetKeyDown(KeyCode.J)) {
            Destroy(snake.gameObject);
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
        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            Toolbox.Log(snake.snakeEvents.ToString());
        }
    }

    void SendNewDirection(Direction direction) {
        snake.ChangeDirectionAtNextTick(direction);
        CmdKeyDown(direction.Serialize(), GlobalTick.I.currentTick + 1);
    }

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
            var snakeEvent = new SnakeChangeDirectionEvent(Direction.Deserialize(newDirection));
            var x = receivedEvents.ToList();
            x.Add(snakeEvent.newDirection.GetType().ToString());
            receivedEvents = x.ToArray();
            snake.CorrectEventAtTick(snakeEvent, tick);
        }
    }
}
