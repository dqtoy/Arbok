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
    public Text tickUI;
    public float movesPerSecond = 5;

    public Direction currentDirection = Up.I;

    public SnakeEvents snakeEvents { get; private set; }
    public List<GameObject> links { get; private set; }
    public int currentTick { get; private set; }

    public event Action AfterTick;
    public event Action AfterRollbackTick;

    float elapsedTime = 0;

    void Awake() {
        snakeEvents = new SnakeEvents();
        links = new List<GameObject>();
        currentTick = 0;
        all.Add(this);
    }

    void Update() {
        elapsedTime += Time.deltaTime;

        // if (elapsedTime > (1 / movesPerSecond)) {
        //     elapsedTime -= 1 / movesPerSecond;
        //     DoTick();
        // }

        if (Input.GetKeyDown(KeyCode.N)) {
            DoTick();
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            RollbackTick();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && netId.Value == 1) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                RollbackTick();
            } else {
                DoTick();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && netId.Value == 2) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                RollbackTick();
            } else {
                DoTick();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && netId.Value == 3) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                RollbackTick();
            } else {
                DoTick();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && netId.Value == 4) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                RollbackTick();
            } else {
                DoTick();
            }
        }
        cameraTarget.position += Vector3.up * (links.Count + 1);
    }

    void UpdateTickText() {
        tickUI.text = currentTick.ToString();
    }

    public void ChangeDirectionAtNextTick(Direction newDirection) {
        snakeEvents.AddOrReplaceAtTick(currentTick + 1, new SnakeChangeDirectionEvent(newDirection));
    }

    public void ChangeDirectionAtTick(Direction newDirection, int tick) {
        var missedTick = tick <= this.currentTick;
        var changeDirectionEvent = new SnakeChangeDirectionEvent(newDirection);

        if (missedTick) {
            var realCurrentTick = currentTick;
            var rolledBackCount = RollbackToTick(tick - 1);
            snakeEvents.AddOrReplaceAtTick(tick, changeDirectionEvent);
            RollForwardToTick(realCurrentTick);
        } else {
            snakeEvents.AddOrReplaceAtTick(tick, changeDirectionEvent);
        }

        UpdateTickText();
    }

    public Vector3 GetHeadPositionAtTick(int tick) {
        Toolbox.Log("GetHeadPositionAtTick A " + currentTick + " | " + JsonConvert.SerializeObject(head.transform.position));
        var realCurrentTick = currentTick;
        RollbackToTick(tick);
        Toolbox.Log("GetHeadPositionAtTick B " + currentTick + " | " + JsonConvert.SerializeObject(head.transform.position));
        var x = head.transform.position;
        RollForwardToTick(realCurrentTick);
        Toolbox.Log("GetHeadPositionAtTick C " + currentTick + " | " + JsonConvert.SerializeObject(head.transform.position));

        return x;
    }

    void RollForwardToTick(int tick) {
        while (currentTick < tick) {
            DoTick();
        }
    }

    int RollbackToTick(int tickToRollbackTo) {
        Toolbox.Log("RollbackToTick " + tickToRollbackTo);
        var rolledBackCount = 0;

        while (this.currentTick != tickToRollbackTo) {
            RollbackTick();
            rolledBackCount++;
        }

        return rolledBackCount;
    }

    void DoTick() {
        currentTick++;
        UpdateTickText();

        snakeEvents.AddOrReplaceAtTick(currentTick, new SnakeMoveEvent());

        snakeEvents.ExecuteEventsAtTickIfAny(currentTick, this);

        AfterTick?.Invoke();
    }

    void RollbackTick() {
        Toolbox.Log("RollbackTick");
        snakeEvents.ReverseEventsAtTickIfAny(currentTick, this);
        currentTick--;
        AfterRollbackTick?.Invoke();
    }

    public void SetSnakeData(SnakeState state) {
        this.head.transform.position = state.headPosition;
        this.currentTick = state.tick;
        this.elapsedTime = state.elapsedTime;
        this.currentDirection = state.direction;
        this.links = state.linkPositions.Select(x => Instantiate(snakeTailPrefab, x, Quaternion.identity, this.transform)).ToList();
    }

    public void Die() {
        Toolbox.Log("DIE");
        Destroy(gameObject);
        // Hide visible parts
        // Stop moving/ticking
    }

    void OnDestroy() {
        all.Remove(this);
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.HasComponent<Wall>() || (other.gameObject.HasComponent<SnakeTail>())) {
            Die();
        }
        if (other.gameObject.HasComponent<Apple>()) {
            EatApple(other.gameObject);
        }
    }

    void EatApple(GameObject apple) {
        snakeEvents.AddOrReplaceAtTick(currentTick + 1, new SnakeEatAppleEvent(apple));
    }
}
