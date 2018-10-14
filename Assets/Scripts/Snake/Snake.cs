using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Snake : NetworkBehaviour {

    public static List<Snake> all = new List<Snake>();

    public GameObject snakeTailPrefab;
    public List<GameObject> links = new List<GameObject>();

    public float movesPerSecond = 5;

    public GameObject head;
    float elapsedTime = 0;
    bool growOnNextMove = false;
    Direction currentDirection = Up.I;
    NetworkSnakeController controller;
    public int currentTick { get; private set; }
    Dictionary<int, Direction> snakeEvents = new Dictionary<int, Direction>();

    public void ChangeDirectionAtNextTick(Direction newDirection) {
        snakeEvents[currentTick + 1] = newDirection;
    }

    public void ChangeDirectionAtTick(Direction newDirection, int tick) {
        var missedTick = tick <= this.currentTick;

        if (missedTick) {
            var rolledBackCount = RollbackToTick(tick - 1);
            snakeEvents[tick] = newDirection;
            for (int i = 0; i < rolledBackCount; i++) {
                DoTick();
            }
        } else {
            snakeEvents[tick] = newDirection;
        }
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
        if (snakeEvents.ContainsKey(currentTick)) {
            ReverseDirection(snakeEvents[currentTick]);
        } else {
            ReverseDirection(currentDirection);
        }
        currentTick--;
    }

    void ReverseDirection(Direction direction) {
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

    void DoTick() {
        currentTick++;

        currentDirection = GetNewDirection();

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

    Direction GetNewDirection() {
        if (!snakeEvents.ContainsKey(currentTick)) {
            return currentDirection;
        }

        var nextDirection = snakeEvents[currentTick];

        if (nextDirection == Down.I && currentDirection == Up.I) {
            return currentDirection;
        }
        if (nextDirection == Left.I && currentDirection == Right.I) {
            return currentDirection;
        }
        if (nextDirection == Up.I && currentDirection == Down.I) {
            return currentDirection;
        }
        if (nextDirection == Right.I && currentDirection == Left.I) {
            return currentDirection;
        }

        return nextDirection;
    }

    [Command] // runs only on server
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
        Snake.all.ForEach(x => TargetReceiveSnakePosition(connectionToClient, x.head.transform.position, x.netId));
    }

    [TargetRpc]
    public void TargetReceiveSnakePosition(NetworkConnection connection, Vector3 position, NetworkInstanceId netId) {
        if (netId == this.netId) return;
        Snake.all.Where(x => x.netId == netId).First().head.transform.position = position;
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
