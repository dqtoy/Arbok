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
    Direction direction = Up.I;
    Direction nextDirection = Up.I;
    ISnakeController controller;

    void Awake() {
        all.Add(this);
        links.Add(head);
        nextDirection = direction;

        controller = GetComponent<NetworkSnakeController>();
    }

    void Update() {
        nextDirection = controller.GetDirection();

        if (nextDirection == Down.I && direction == Up.I) {
            nextDirection = direction;
        }
        if (nextDirection == Left.I && direction == Right.I) {
            nextDirection = direction;
        }
        if (nextDirection == Up.I && direction == Down.I) {
            nextDirection = direction;
        }
        if (nextDirection == Right.I && direction == Left.I) {
            nextDirection = direction;
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime > (1 / movesPerSecond)) {
            elapsedTime -= 1 / movesPerSecond;

            float speed = 1.0f;

            direction = nextDirection;

            Vector3 newPosition = head.transform.position + speed * direction.GetMoveVector();

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

            head.transform.rotation = direction.GetHeadRotation();
            head.transform.position = newPosition;
        }
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
