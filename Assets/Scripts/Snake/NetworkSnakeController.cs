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

    bool gotGameState = false;
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
            CmdRequestGameState();
        } else {
            initialized = true;
        }

        netIdUI.text = netId.ToString() + "*";
        MainCamera.I.target = snake.cameraTarget;
    }

    void OnGlobalTickInitialized(int tick) {
        Toolbox.Log("NetworkSnakeController OnGlobalTickInitialized");
        initialized = true;
        snake.SpawnOnNextTick();
    }

    [Command]
    public void CmdRequestGameState() {
        Toolbox.Log("CmdRequestGameState");

        var allSnakesJson = JsonConvert.SerializeObject(Snake.all.Select(x => x.ToState()));
        var applesJson = AppleManager.I.SerializeAllApplesState();

        TargetReceiveGameState(connectionToClient, GlobalTick.I.currentTick, applesJson, allSnakesJson);
    }

    [TargetRpc]
    public void TargetReceiveGameState(NetworkConnection connection, int tick, string appleStatesJson, string allSnakesJson) {
        Toolbox.Log("TargetReceiveGameState");

        AppleManager.I.DeserializeAndLoadAllApplesState(appleStatesJson);

        ReceiveAllSnakesState(allSnakesJson);

        GlobalTick.I.SetTickGorGameStateReceived(tick);

        gotGameState = true;
    }

    void ReceiveAllSnakesState(string allSnakesJson) {
        JsonConvert.DeserializeObject<IEnumerable<SnakeState>>(allSnakesJson)
            .Where(NotLocalSnake).ToList()
            .ForEach(LoadStateToSnake);
    }

    bool NotLocalSnake(SnakeState x) => x.netId != this.netId;

    void LoadStateToSnake(SnakeState x) => GetSnakeByNetId(x.netId).SetSnakeData(x);

    Snake GetSnakeByNetId(NetworkInstanceId netId) => Snake.all.First(x => x.GetComponent<NetworkIdentity>().netId == this.netId);

    void Update() {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            Toolbox.Log(snake.snakeEvents.ToString());
        }

        if (!ready && gotGameState) {
            Init();
        }

        if (!initialized || snake.isDead) return;

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && snake.currentDirection != Down.I) {
            SendNewDirection(Up.I);
        }
        if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && snake.currentDirection != Left.I) {
            SendNewDirection(Right.I);
        }
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && snake.currentDirection != Up.I) {
            SendNewDirection(Down.I);
        }
        if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && snake.currentDirection != Right.I) {
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
            snake.snakeEvents.CorrectEventAtTick(snakeEvent, tick);
        }
    }

    void OnDestroy() {
        GlobalTick.OnInitialized -= OnGlobalTickInitialized;
    }
}
