using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SnakeEvents {
    Dictionary<int, SnakeCompoundEvent> dict = new Dictionary<int, SnakeCompoundEvent>();

    public void AddOrReplaceAtTick(int tick, SnakeEvent snakeEvent) {
        if (dict.ContainsKey(tick) == false) {
            dict[tick] = new SnakeCompoundEvent();
        }
        dict[tick].AddOrReplaceEvent(snakeEvent);
    }

    public bool HasEventAtTick(int tick) {
        return dict.ContainsKey(tick);
    }

    public void ExecuteEventsAtTickIfAny(int tick, Snake snake) {
        if (dict.ContainsKey(tick)) {
            dict[tick].Execute(snake);
        }
    }

    public void ReverseEventsAtTickIfAny(int tick, Snake snake) {
        if (dict.ContainsKey(tick)) {
            dict[tick].Reverse(snake);
        }
    }
}

public class SnakeCompoundEvent : Dictionary<Type, SnakeEvent> {
    Dictionary<Type, SnakeEvent> events = new Dictionary<Type, SnakeEvent>();

    public void AddOrReplaceEvent(SnakeEvent snakeEvent) {
        events[snakeEvent.GetType()] = snakeEvent;
    }

    public void Execute(Snake snake) {
        foreach (var kvp in events) {
            kvp.Value.Execute(snake);
        }
    }

    public void Reverse(Snake snake) {
        foreach (var kvp in events) {
            kvp.Value.Reverse(snake);
        }
    }
}

public interface SnakeEvent {
    void Execute(Snake snake);
    void Reverse(Snake snake);
}

public class SnakeChangeDirectionEvent : SnakeEvent {
    public Direction previousDirection = DummyDirection.I;
    public Direction newDirection;

    public SnakeChangeDirectionEvent(Direction newDirection) {
        this.newDirection = newDirection;
    }

    public void Execute(Snake snake) {
        previousDirection = snake.currentDirection;

        if ((newDirection == Down.I && snake.currentDirection == Up.I) ||
            (newDirection == Left.I && snake.currentDirection == Right.I) ||
            (newDirection == Up.I && snake.currentDirection == Down.I) ||
            (newDirection == Right.I && snake.currentDirection == Left.I)) {

            return;
        }

        snake.currentDirection = newDirection;
    }

    public void Reverse(Snake snake) {
        snake.currentDirection = previousDirection;
    }
}

public class SnakeEatAppleEvent : SnakeEvent {
    GameObject apple;

    public SnakeEatAppleEvent(GameObject apple) {
        this.apple = apple;
    }

    public void Execute(Snake snake) {
        apple.SetActive(false);
        var newTail = GameObject.Instantiate(snake.snakeTailPrefab, snake.head.transform.position, Quaternion.identity);
        newTail.transform.parent = snake.transform;
        snake.links.Insert(0, newTail);
    }

    public void Reverse(Snake snake) {
        var firstTailLink = snake.links[0];
        snake.links.Remove(firstTailLink);
        GameObject.Destroy(firstTailLink);
        apple.SetActive(true);
    }
}

public class Snake : NetworkBehaviour {

    public static List<Snake> all = new List<Snake>();

    public GameObject snakeTailPrefab;
    public List<GameObject> links = new List<GameObject>();

    public float movesPerSecond = 5;

    public GameObject head;
    public Direction currentDirection = Up.I;
    public int currentTick { get; private set; }
    public SnakeEvents snakeEvents = new SnakeEvents();
    public Text tickUI;
    public NetworkSnakeController controller;

    float elapsedTime = 0;

    public void ChangeDirectionAtNextTick(Direction newDirection) {
        snakeEvents.AddOrReplaceAtTick(currentTick + 1, new SnakeChangeDirectionEvent(newDirection));
    }

    public void ChangeDirectionAtTick(Direction newDirection, int tick) {
        var missedTick = tick <= this.currentTick;
        var changeDirectionEvent = new SnakeChangeDirectionEvent(newDirection);

        if (missedTick) {
            var rolledBackCount = RollbackToTick(tick - 1);
            snakeEvents.AddOrReplaceAtTick(tick, changeDirectionEvent);
            for (int i = 0; i < rolledBackCount; i++) {
                DoTick();
            }
        } else {
            snakeEvents.AddOrReplaceAtTick(tick, changeDirectionEvent);
        }

        UpdateTickText();
    }

    int RollbackToTick(int tickToRollbackTo) {
        var rolledBackCount = 0;

        while (this.currentTick != tickToRollbackTo) {
            RollbackTick();
            rolledBackCount++;
        }

        return rolledBackCount;
    }

    void RollbackTick() {
        MoveBack(currentDirection);
        snakeEvents.ReverseEventsAtTickIfAny(currentTick, this);
        currentTick--;
    }

    void MoveBack(Direction direction) {
        head.transform.position += direction.GetMoveVector() * -1;
    }

    void Awake() {
        currentTick = 0;
        all.Add(this);
    }

    void Update() {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > (1 / movesPerSecond)) {
            elapsedTime -= 1 / movesPerSecond;
            DoTick();
        }
    }

    void UpdateTickText() {
        tickUI.text = currentTick.ToString();
    }

    void DoTick() {
        currentTick++;
        UpdateTickText();

        snakeEvents.ExecuteEventsAtTickIfAny(currentTick, this);

        Vector3 newPosition = head.transform.position + currentDirection.GetMoveVector();

        if (AboutToCollideWithSelf(newPosition)) {
            Die();
        }

        if (links.Count > 0) {
            var oldTail = links[links.Count - 1];
            links.RemoveAt(links.Count - 1);
            links.Insert(0, oldTail);
            oldTail.transform.position = head.transform.position;
        }

        head.transform.rotation = currentDirection.GetHeadRotation();
        head.transform.position = newPosition;
    }

    private bool AboutToCollideWithSelf(Vector3 newPosition) {
        return links.Any(x => x.transform.position == newPosition);
    }

    public override void OnStartLocalPlayer() {
        CmdRequestSnakePositions();
    }

    [Command]
    public void CmdRequestSnakePositions() {
        Debug.Log("CmdRequestSnakePositions: connectionToClient " + connectionToClient.connectionId);
        Snake.all.ForEach(
            x => {
                var linksJson = JsonConvert.SerializeObject(x.links.Select(y => y.transform.position).ToArray());
                Debug.Log("TargetReceiveSnakePosition: linksJson: " + linksJson);
                TargetReceiveSnakePosition(
                    connectionToClient,
                    x.head.transform.position,
                    x.currentTick,
                    x.currentDirection.Serialize(),
                    linksJson,
                    x.netId
                );
            }
        );
    }

    [TargetRpc]
    public void TargetReceiveSnakePosition(NetworkConnection connection, Vector3 position, int tick, short direction, string linksJson, NetworkInstanceId netId) {
        if (netId == this.netId) return;
        var snakeToModify = Snake.all.Where(x => x.netId == netId).First();
        snakeToModify.head.transform.position = position;
        snakeToModify.currentTick = tick;
        snakeToModify.elapsedTime = 0;
        snakeToModify.currentDirection = Direction.Deserialize(direction);

        Debug.Log("TargetReceiveSnakePosition: linksJson: " + linksJson);

        var z = JsonConvert.DeserializeObject<Vector3[]>(linksJson);
        snakeToModify.links = z.Select(y => Instantiate(snakeTailPrefab, y, Quaternion.identity)).ToList();
        snakeToModify.links.ForEach(x => x.transform.parent = snakeToModify.transform);

    }

    // Not used at the moment
    // private bool AboutToCollideWithAnySnake(Vector3 newPosition) {
    //     return all.Any(x => AreWeGoingToCollideWithOtherSnake(x, newPosition));
    // }

    // private bool AreWeGoingToCollideWithOtherSnake(Snake otherSnake, Vector3 newPosition) {
    //     return otherSnake.links.Any(x => x.transform.position == newPosition);
    // }

    void Die() {
        Debug.Log("DIE");
        all.Remove(this);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other) {
        //if (other.gameObject.HasComponent<Wall>() || (other.gameObject.HasComponent<SnakeTail>())) {
        //    Die();
        //}
        if (other.gameObject.HasComponent<Apple>()) {
            EatApple(other.gameObject);
        }
    }

    void EatApple(GameObject apple) {
        snakeEvents.AddOrReplaceAtTick(currentTick + 1, new SnakeEatAppleEvent(apple));
        // CmdSnakeGrow(currentTick + 1);
    }

    // Not sure if we need to send message when eating an apple
    // [Command]
    // private void CmdSnakeGrow(int tick) {
    //     RpcSnakeGrow(tick);
    // }

    // [ClientRpc]
    // public void RpcSnakeGrow(int tick) {
    //     // snakeEvents.AddOrReplaceAtTick(tick, new SnakeGrowEvent());
    // }
}
