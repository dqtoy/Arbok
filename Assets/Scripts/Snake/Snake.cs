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
    public List<GameObject> links { get; private set; }

    public event Action AfterTick;
    public event Action AfterRollbackTick;

    public float cameraScalingMod = 1;
    public bool isDead;

    void Awake() {
        snakeEvents = new SnakeEvents();
        links = new List<GameObject>();
        all.Add(this);
    }

    void Start() {
        GlobalTick.OnDoTick += DoTick;
        GlobalTick.OnRollbackTick += RollbackTick;
    }

    void Update() {
        // cameraTarget.position += Vector3.up * (links.Count + 1);
        cameraTarget.localScale = new Vector3(links.Count * cameraScalingMod, 1, 1);
    }

    public void ChangeDirectionAtNextTick(Direction newDirection) {
        snakeEvents.PurgeTicksAfterTick(GlobalTick.I.currentTick);
        snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick + 1, new SnakeChangeDirectionEvent(newDirection));
    }

    public void CorrectEventAtTick(SnakeEvent snakeEvent, int tick) {
        var missedTick = tick <= GlobalTick.I.currentTick;

        if (missedTick) {
            var realCurrentTick = GlobalTick.I.currentTick;
            var rolledBackCount = GlobalTick.I.RollbackToTick(tick - 1);
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
            snakeEvents.AddOrReplaceAtTick(GlobalTick.I.currentTick, new SnakeDieEvent());
        }
    }

    bool DidWeCollideWithSelf() {
        return links.Any(x => x.transform.position == head.transform.position);
    }

    bool DidWeCollideWithWall() {
        return Wall.all.Any(x => x.transform.position == head.transform.position);
    }

    bool DidWeCollideWithOtherSnake() {
        return false;
    }

    void DoAppleEatCheck() {
        var apple = AppleManager.all.FirstOrDefault(x => (x.transform.position == head.transform.position));

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
        this.links = state.linkPositions.Select(x => Instantiate(snakeTailPrefab, x, Quaternion.identity, this.transform)).ToList();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.HasComponent<Wall>() || (other.gameObject.HasComponent<SnakeTail>())) {
            Die();
        }
    }

    public void Die() {
        Toolbox.Log("DIE");
        Destroy(gameObject);
        // Hide visible parts
        // Stop moving/ticking
    }

    void OnDestroy() {
        all.Remove(this);
        GlobalTick.OnDoTick -= DoTick;
        GlobalTick.OnRollbackTick -= RollbackTick;
    }
}
