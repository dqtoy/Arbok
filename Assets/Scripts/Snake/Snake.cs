using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Snake : NetworkBehaviour {
    public static List<Snake> all = new List<Snake>();

    public GameObject snakeTailPrefab;
    public SnakeHead head;
    public Transform cameraTarget;
    public GameObject headVisual;

    public Direction currentDirection = Up.I;

    public SnakeEvents snakeEvents { get; private set; }
    public List<SnakeTail> links { get; private set; }

    public event Action AfterTick;
    public event Action AfterRollbackTick;

    public float cameraScalingMod = 1;
    public bool isDead;

    public static int GetAlivePlayerCount() {
        return Snake.all.Count(x => x.isDead == false);
    }

    void Awake() {
        Debug.Log("Snake Awake Frame: " + Time.frameCount);
        snakeEvents = new SnakeEvents();
        links = new List<SnakeTail>();
        all.Add(this);
        GlobalTick.OnInitialized += (tick) => {
            Debug.Log("Snake GlobalTick.I.OnInitialized");
            StartTicking();
        };
    }

    void Start() {
        Debug.Log("Snake Start " + Time.frameCount);
    }

    void StartTicking() {
        GlobalTick.OnDoTick += DoTick;
        GlobalTick.OnRollbackTick += RollbackTick;
    }

    void Update() {
        cameraTarget.position = new Vector3(cameraTarget.position.x, links.Count + 1, cameraTarget.position.z);
        // cameraTarget.localScale = new Vector3(links.Count * cameraScalingMod, 1, 1);
    }

    public void ChangeDirectionAtNextTick(Direction newDirection) {
        snakeEvents.PurgeTicksAfterTick(GlobalTick.I.currentTick);
        snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick + 1, new SnakeChangeDirectionEvent(newDirection));
    }

    public void CorrectEventAtTick(SnakeEvent snakeEvent, int tick) {
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

    void DoTick() {
        if (!isDead) {
            DoDeathCheck();
            DoAppleEatCheck();
            snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick, new SnakeMoveEvent());
        }

        snakeEvents.ExecuteEventsAtTickIfAny(GlobalTick.I.currentTick, this);

        AfterTick?.Invoke();
    }

    void DoDeathCheck() {
        if (DidWeCollideWithSelf() || DidWeCollideWithWall() || DidWeCollideWithOtherSnake()) {
            Toolbox.Log("DIE");
            snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick, new SnakeDieEvent());
        }
    }

    bool DidWeCollideWithSelf() {
        return links.Any(doesPositionMatchHeadPosition);
    }

    bool DidWeCollideWithWall() {
        return Wall.all.Any(doesPositionMatchHeadPosition);
    }

    bool DidWeCollideWithOtherSnake() {
        return Snake.all.Where(SnakeIsAlive).Any(x => x.links.Any(doesPositionMatchHeadPosition));
    }

    bool SnakeIsAlive(Snake s) {
        return s.isDead == false;
    }

    bool doesPositionMatchHeadPosition(MonoBehaviour x) {
        return x.transform.position == head.transform.position;
    }

    void DoAppleEatCheck() {
        var apple = AppleManager.all.FirstOrDefault(x => (x.gameObject.activeSelf && x.transform.position == head.transform.position));

        if (apple) EatApple(apple);
    }

    void EatApple(Apple apple) {
        snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick, new SnakeEatAppleEvent(apple));
    }

    void RollbackTick() {
        //Toolbox.Log("RollbackTick");
        snakeEvents.ReverseEventsAtTickIfAny(GlobalTick.I.currentTick, this);
        AfterRollbackTick?.Invoke();
    }

    public void SetSnakeData(SnakeState state) {
        this.head.transform.position = state.headPosition;
        this.currentDirection = state.direction;
        this.links = state.linkPositions.Select(x => Instantiate(snakeTailPrefab, x, Quaternion.identity, this.transform).GetComponent<SnakeTail>()).ToList();
    }

    void OnDestroy() {
        all.Remove(this);
        GlobalTick.OnDoTick -= DoTick;
        GlobalTick.OnRollbackTick -= RollbackTick;
    }
}
