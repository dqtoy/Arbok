using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Snake : NetworkBehaviour, ITickable {
    public static List<Snake> all = new List<Snake>();

    public GameObject snakeTailPrefab;
    public SnakeHead head;
    public Transform cameraTarget;
    public GameObject headVisual;

    public Direction currentDirection = Up.I;

    public GameEvents<SnakeCompoundEvent, Snake> snakeEvents { get; private set; }
    public List<SnakeTail> links { get; private set; }

    public event Action AfterRollbackTick;

    public bool isDead;

    public static int GetAlivePlayerCount() {
        return Snake.all.Count(x => x.isDead == false);
    }

    void Awake() {
        Debug.Log("Snake Awake: " + GetInstanceID());
        Debug.Log("Snake Awake Frame: " + Time.frameCount);
        snakeEvents = new GameEvents<SnakeCompoundEvent, Snake>();
        links = new List<SnakeTail>();
        all.Add(this);

        SnakeDieEvent.DoExecute(this);

        StartTicking();
    }

    public void SpawnOnNextTick() {
        Debug.Log("Snake SpawnOnNextTick: " + GetInstanceID());
        DoEventAtNextTick(new SnakeSpawnEvent(), 10);
        CmdSpawn(GlobalTick.I.currentTick + 10);
    }

    // ==============================
    // Network
    // ==============================
    [Command]
    private void CmdSpawn(int tick) {
        RpcSpawn(tick);
    }

    [ClientRpc]
    public void RpcSpawn(int tick) {
        if (!isLocalPlayer) {
            CorrectEventAtTick(new SnakeSpawnEvent(), tick);
        }
    }

    void Start() {
        Debug.Log("Snake Start " + Time.frameCount);
    }

    public void StartTicking() {
        GlobalTick.OnDoTick += DoTick;
        GlobalTick.OnRollbackTick += RollbackTick;
    }

    void Update() {
        cameraTarget.position = new Vector3(cameraTarget.position.x, links.Count + 1, cameraTarget.position.z);
    }

    public void DoEventAtNextTick(GameEvent<Snake> snakeEvent, int ticksInFuture = 1) {
        snakeEvents.PurgeTicksAfterTick(GlobalTick.I.currentTick + ticksInFuture - 1);
        snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick + ticksInFuture, snakeEvent);
    }

    public void CorrectEventAtTick(GameEvent<Snake> snakeEvent, int tick) {
        var missedTick = tick <= GlobalTick.I.currentTick;

        if (missedTick) {
            var realCurrentTick = GlobalTick.I.currentTick;
            GlobalTick.I.RollbackToTick(tick - 1);
            snakeEvents.PurgeTicksAfterTick(GlobalTick.I.currentTick);
            snakeEvents.AddOrReplaceAtTick(tick, snakeEvent);
            GlobalTick.I.RollForwardToTick(realCurrentTick);
        } else {
            snakeEvents.AddOrReplaceAtTick(tick, snakeEvent);
            // TODO If this happens, then we are behind and need to fastforward
        }
    }

    public void DoTick() {
        if (!isDead) {
            DoDeathCheck();
            DoAppleEatCheck();
            snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick, new SnakeMoveEvent());
        }

        snakeEvents.ExecuteEventsAtTickIfAny(GlobalTick.I.currentTick, this);
    }

    void DoDeathCheck() {
        if (DidWeCollideWithSelf() || DidWeCollideWithWall() || DidWeCollideWithOtherSnake()) {
            Toolbox.Log("DIE");
            snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick, new SnakeDieEvent());
        }
    }

    bool DidWeCollideWithSelf() => links.Any(doesPositionMatchHeadPosition);

    bool DidWeCollideWithWall() => Wall.all.Any(doesPositionMatchHeadPosition);

    bool DidWeCollideWithOtherSnake() => all.Where(SnakeIsAlive).Any(x => x.links.Any(doesPositionMatchHeadPosition));

    bool SnakeIsAlive(Snake s) => s.isDead == false;

    bool doesPositionMatchHeadPosition(MonoBehaviour x) => x.transform.position == head.transform.position;

    void DoAppleEatCheck() {
        var apple = AppleManager.I.all.FirstOrDefault(x => (x.gameObject.activeSelf && x.transform.position == head.transform.position));

        if (apple) EatApple(apple);
    }

    void EatApple(Apple apple) {
        snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick, new SnakeEatAppleEvent(apple));
    }

    public void RollbackTick() {
        //Toolbox.Log("RollbackTick");
        snakeEvents.ReverseEventsAtTickIfAny(GlobalTick.I.currentTick, this);
        AfterRollbackTick?.Invoke();
    }

    public void SetSnakeData(SnakeState state) {
        this.head.transform.position = state.headPosition;
        this.currentDirection = state.direction;
        this.links = state.linkPositions.Select(x => Instantiate(snakeTailPrefab, x, Quaternion.identity, this.transform).GetComponent<SnakeTail>()).ToList();

        if (state.isDead == false) {
            SnakeDieEvent.DoReverse(this);
        } else {
            SnakeDieEvent.DoExecute(this);
        }
    }

    void OnDestroy() {
        all.Remove(this);
        GlobalTick.OnDoTick -= DoTick;
        GlobalTick.OnRollbackTick -= RollbackTick;
    }
}
