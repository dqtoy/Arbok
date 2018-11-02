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

    public bool isDead;

    public static int GetAlivePlayerCount() {
        return Snake.all.Count(x => x.isDead == false);
    }

    void Awake() {
        Toolbox.Log("Snake Awake: " + GetInstanceID());
        Toolbox.Log("Snake Awake Frame: " + Time.frameCount);
        snakeEvents = new GameEvents<SnakeCompoundEvent, Snake>();
        links = new List<SnakeTail>();
        all.Add(this);

        SnakeDieEvent.DoExecute(this);

        StartTicking();
    }

    public void SpawnOnNextTick() {
        Toolbox.Log("Snake SpawnOnNextTick: " + GetInstanceID());
        var ticksAhead = 1;
        DoEventAtNextTick(new SnakeSpawnEvent(), ticksAhead);
        CmdSpawn(GlobalTick.I.currentTick + ticksAhead);
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
            snakeEvents.CorrectEventAtTick(new SnakeSpawnEvent(), tick);
        }
    }

    void Start() {
        Toolbox.Log("Snake Start " + Time.frameCount);
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

    public void DoTick(int tick) {
        if (!isDead) {
            DoDeathCheck(tick);
            DoAppleEatCheck(tick);
            snakeEvents.AddOrReplaceAtTick(tick, new SnakeMoveEvent());
        }

        snakeEvents.ExecuteEventsAtTickIfAny(tick, this);
    }

    public SnakeState ToState() {
        return new SnakeState() {
            linkPositions = links.Select(Position),
                headPosition = head.transform.position,
                direction = currentDirection,
                isDead = isDead,
                netId = netId
        };
    }

    Vector3 Position(MonoBehaviour x) => x.transform.position;

    void DoDeathCheck(int tick) {
        if (DidWeCollideWithSelf() || DidWeCollideWithWall() || DidWeCollideWithOtherSnake()) {
            Toolbox.Log("DIE");
            snakeEvents.AddOrReplaceAtTick(tick, new SnakeDieEvent());
        }
    }

    bool DidWeCollideWithSelf() => links.Any(doesPositionMatchHeadPosition);

    bool DidWeCollideWithWall() => Wall.all.Any(doesPositionMatchHeadPosition);

    bool DidWeCollideWithOtherSnake() => all.Where(SnakeIsAlive).Any(x => x.links.Any(doesPositionMatchHeadPosition));

    bool SnakeIsAlive(Snake s) => s.isDead == false;

    bool doesPositionMatchHeadPosition(MonoBehaviour x) => x.transform.position == head.transform.position;

    void DoAppleEatCheck(int tick) {
        // - check if there is a possible apple at head position
        // - if yes, has it spawned?
        // - if yes, has it been eaten?
        // - if no, eat it
        //   - mark apple as eaten in AllApplesState, and add snake tail

        AppleState appleState;

        if (AppleManager.I.TryGetAppleAtPosition(head.transform.position, out appleState) == false) return;
        Toolbox.Log("DoAppleEatCheck 2");

        if (appleState.spawnTick > tick) return;
        Toolbox.Log("DoAppleEatCheck 3");

        if (appleState.eatenTick < tick) return;
        Toolbox.Log("DoAppleEatCheck 4");

        AppleManager.I.EatApple(tick, appleState);

        var newTail = GameObject.Instantiate(snakeTailPrefab, transform).GetComponent<SnakeTail>();
        links.Add(newTail);
    }

    void ReverseAppleEatCheck(int tick) {
        // - check if there is a possible apple at head position
        // - if yes, has it spawned?
        // - if yes, was it eaten on this tick?
        // - if yes, reverse eating it
        //   - mark apple as not eaten in AllApplesState, and remove snake tail

        AppleState appleState;

        if (AppleManager.I.TryGetAppleAtPosition(head.transform.position, out appleState) == false) return;

        if (appleState.spawnTick > tick) return;

        if (appleState.eatenTick != tick) return;

        AppleManager.I.ReverseEatApple(tick, appleState);

        var firstTailLink = links.Last();
        links.Remove(firstTailLink);
        GameObject.Destroy(firstTailLink.gameObject);
    }

    public void RollbackTick(int tick) {
        snakeEvents.ReverseEventsAtTickIfAny(tick, this);

        ReverseAppleEatCheck(tick);
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
