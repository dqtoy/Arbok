using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class Snake : NetworkBehaviour {

    public static List<Snake> all = new List<Snake>();

    public GameObject snakeTailPrefab;
    public List<GameObject> links = new List<GameObject>();

    public float movesPerSecond = 5;

    public GameObject head;
    float elapsedTime = 0;
    bool growOnNextMove = false;
    public Direction currentDirection = Up.I;
    NetworkSnakeController controller;
    public int currentTick { get; private set; }
    public SnakeEvents snakeEvents = new SnakeEvents();
    public Text tickUI;

    public void ChangeDirectionAtNextTick(Direction newDirection) {
        snakeEvents.AddOrReplaceAtTick(currentTick + 1, new SnakeChangeDirectionEvent(newDirection));
    }

    public void ChangeDirectionAtTick(Direction newDirection, int tick) {
        var missedTick = tick <= this.currentTick;

        if (missedTick) {
            var rolledBackCount = RollbackToTick(tick - 1);
            snakeEvents.AddOrReplaceAtTick(tick, new SnakeChangeDirectionEvent(newDirection));
            for (int i = 0; i < rolledBackCount; i++) {
                DoTick();
            }
        } else {
            snakeEvents.AddOrReplaceAtTick(tick, new SnakeChangeDirectionEvent(newDirection));
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
        links.Add(head);

        controller = GetComponent<NetworkSnakeController>();
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

        float speed = 1.0f;

        Vector3 newPosition = head.transform.position + speed * currentDirection.GetMoveVector();

        if (AboutToCollideWithSelf(newPosition)) {
            Die();
        }

        if (growOnNextMove) {
            growOnNextMove = false;
            //CmdSpawnTail();
        } else if (links.Count > 1) {
            var oldTail = links[links.Count - 1];
            links.RemoveAt(links.Count - 1);
            links.Insert(1, oldTail);
            oldTail.transform.position = head.transform.position;
        }

        head.transform.rotation = currentDirection.GetHeadRotation();
        head.transform.position = newPosition;
    }

    [Command]
    private void CmdSpawnTail() {
        GameObject newTail = Instantiate(snakeTailPrefab, head.transform.position, Quaternion.identity);
        NetworkServer.SpawnWithClientAuthority(newTail, connectionToClient);
        RpcSpawnTail(newTail);
    }

    [ClientRpc]
    public void RpcSpawnTail(GameObject newTail) {
        newTail.transform.parent = transform;
        links.Insert(1, newTail);
    }

    public override void OnStartLocalPlayer() {
        CmdRequestSnakePositions();
    }

    [Command]
    public void CmdRequestSnakePositions() {
        Debug.Log("CmdRequestSnakePositions: connectionToClient " + connectionToClient.connectionId);
        Snake.all.ForEach(x => TargetReceiveSnakePosition(connectionToClient, x.head.transform.position, x.currentTick, x.currentDirection.Serialize(), x.netId));
    }

    [TargetRpc]
    public void TargetReceiveSnakePosition(NetworkConnection connection, Vector3 position, int tick, short direction, NetworkInstanceId netId) {
        if (netId == this.netId) return;
        var snakeToModify = Snake.all.Where(x => x.netId == netId).First();
        snakeToModify.head.transform.position = position;
        snakeToModify.currentTick = tick;
        snakeToModify.elapsedTime = 0;
        snakeToModify.currentDirection = Direction.Deserialize(direction);
    }

    private bool AboutToCollideWithSelf(Vector3 newPosition) {
        return links.Any(x => x.transform.position == newPosition);
    }

    private bool AboutToCollideWithAnySnake(Vector3 newPosition) {
        return all.Any(x => AreWeGoingToCollideWithOtherSnake(x, newPosition));
    }

    private bool AreWeGoingToCollideWithOtherSnake(Snake otherSnake, Vector3 newPosition) {
        return otherSnake.links.Any(x => x.transform.position == newPosition);
    }

    void Die() {
        Debug.Log("DIE");
        all.Remove(this);
        Destroy(gameObject);
    }

    void Grow() {
        growOnNextMove = true;
        // snakeEvents[currentTick + 1] = ;
    }

    void OnTriggerEnter(Collider other) {
        //if (other.gameObject.HasComponent<Wall>() || (other.gameObject.HasComponent<SnakeTail>())) {
        //    Die();
        //}
        if (other.gameObject.HasComponent<Apple>()) {
            Destroy(other.gameObject);
            Grow();
        }
    }
}
