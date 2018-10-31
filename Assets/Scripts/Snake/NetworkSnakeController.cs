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

    bool gotApples = false;
    bool gotSnakes = false;
    bool initialized = false;
    bool ready = false;

    public void Awake() {
        snake = GetComponent<Snake>();
    }

    public void Start() {
        netIdUI.text = netId.ToString();
    }

    public override void OnStartLocalPlayer() {
        Toolbox.Log("NetworkSnakeController OnStartLocalPlayer " + Time.frameCount);

        GlobalTick.OnInitialized += OnGlobalTickInitialized;

        if (!isServer) {
            CmdRequestSnakePositions();
            CmdRequestApplePositions();
        } else {
            initialized = true;
        }

        netIdUI.text = netId.ToString() + "*";
        MainCamera.I.target = snake.cameraTarget;
    }

    void OnGlobalTickInitialized(int tick) {
        Debug.Log("NetworkSnakeController OnGlobalTickInitialized");
        initialized = true;
        snake.SpawnOnNextTick();
    }

    [Command]
    public void CmdRequestApplePositions() {
        Toolbox.Log("NetworkSnakeController CmdRequestApplePositions");
        TargetReceiveApplePositions(
            connectionToClient,
            JsonConvert.SerializeObject(AppleManager.I.all.Select(x => x.ToState()).ToArray())
        );
    }

    [TargetRpc]
    public void TargetReceiveApplePositions(NetworkConnection connection, string appleStatesJson) {
        Toolbox.Log("TargetReceiveApplePositions: isServer: " + isServer);
        if (isServer) {
            AppleManager.I.EnableAll();
        } else {
            AppleManager.I.DestroyAll();
            JsonConvert.DeserializeObject<AppleState[]>(appleStatesJson).ToList().ForEach(apple => {
                AppleManager.I.SpawnApple(apple);
            });
        }

        gotApples = true;
    }

    [Command]
    public void CmdRequestSnakePositions() {

        // TODO Pass all snake in one call
        Snake.all.ForEach(
            x => {
                var linksJson = JsonConvert.SerializeObject(x.links.Select(y => y.transform.position).ToArray());
                TargetReceiveSnakePosition(
                    connectionToClient,
                    x.head.transform.position,
                    x.currentDirection.Serialize(),
                    linksJson,
                    x.GetComponent<NetworkIdentity>().netId,
                    x.isDead,
                    GlobalTick.I.currentTick
                );
            }
        );
    }

    [TargetRpc]
    public void TargetReceiveSnakePosition(NetworkConnection connection, Vector3 position, short direction, string linksJson, NetworkInstanceId netId, bool isDead, int tick) {
        if (netId == this.netId) return;

        GlobalTick.I.SetTickForSnakeStuff(tick);

        var snakeToModify = Snake.all.First(x => x.GetComponent<NetworkIdentity>().netId == netId);

        snakeToModify.SetSnakeData(new SnakeState() {
            linkPositions = JsonConvert.DeserializeObject<Vector3[]>(linksJson),
                headPosition = position,
                direction = Direction.Deserialize(direction),
                isDead = isDead
        });

        gotSnakes = true;
    }

    void Update() {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            Toolbox.Log(snake.snakeEvents.ToString());
        }

        if (!ready && gotApples && gotSnakes) {
            Init();
        }

        if (!initialized || snake.isDead) return;

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

    [Client]
    void Init() {
        CmdTellGlobalTickToSendTickToClient();
        ready = true;
    }

    [Command]
    private void CmdTellGlobalTickToSendTickToClient() {
        GlobalTick.I.InitTickForNewClient(connectionToClient);
    }

    void SendNewDirection(Direction direction) {
        snake.DoEventAtNextTick(new SnakeChangeDirectionEvent(direction));
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

    void OnDestroy() {
        GlobalTick.OnInitialized -= OnGlobalTickInitialized;
    }
}
