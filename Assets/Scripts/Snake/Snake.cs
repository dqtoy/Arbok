using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Snake : NetworkBehaviour {

    public static List<Snake> snakes = new List<Snake>();

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
        snakes.Add(this);
        links.Add(head);
        nextDirection = direction;

        controller = GetComponent<NetworkSnakeController>();
    }

    void Update() {
        if (!isLocalPlayer) {
            return;
        }

        if (direction != Down.I && controller.IsUpButtonPressed()) {
            nextDirection = Up.I;
        }
        if (direction != Left.I && controller.IsRightButtonPressed()) {
            nextDirection = Right.I;
        }
        if (direction != Up.I && controller.IsDownButtonPressed()) {
            nextDirection = Down.I;
        }
        if (direction != Right.I && controller.IsLeftButtonPressed()) {
            nextDirection = Left.I;
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
                //var newLink = GameObject.Instantiate(snakeTailPrefab);

                //SpawnTail();
                CmdSpawnTail();

                //newTail.transform.parent = transform;
                //newTail.transform.position = head.transform.position;
                //links.Insert(1, newTail);

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
    private void CmdSpawnTail()
    {
        GameObject newTail = Instantiate(snakeTailPrefab, head.transform.position, Quaternion.identity);
        NetworkServer.SpawnWithClientAuthority(newTail, connectionToClient);
        RpcSpawnTail(newTail);
    }

    [ClientRpc]
    public void RpcSpawnTail(GameObject newTail)
    {
        newTail.transform.parent = transform;
        links.Insert(1, newTail);
    }

    private bool AboutToCollideWithSelf(Vector3 newPosition) {
        return links.Any(x => x.transform.position == newPosition);
    }

    private bool AboutToCollideWithAnySnake(Vector3 newPosition) {
        return snakes.Any(x => AreWeGoingToCollideWithOtherSnake(x, newPosition));
    }

    private bool AreWeGoingToCollideWithOtherSnake(Snake otherSnake, Vector3 newPosition) {
        return otherSnake.links.Any(x => x.transform.position == newPosition);
    }

    void Die() {
        Debug.Log("DIE");
        snakes.Remove(this);
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
