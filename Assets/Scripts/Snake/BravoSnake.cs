using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BravoSnake : NetworkBehaviour {
    public static List<BravoSnake> all = new List<BravoSnake>();

    public GameObject snakeTailPrefab;
    public SnakeHead snakeHead;
    public GameObject networkHead;
    public Transform cameraTarget;
    public Text tickUI;
    public float movesPerSecond = 5;

    public Direction currentDirection = Up.I;

    public List<GameObject> links { get; private set; }
    public int currentTick { get; private set; }

    // public event Action AfterTick;

    float elapsedTime = 0;

    void Awake() {
        links = new List<GameObject>();
        currentTick = 0;
        all.Add(this);
    }

    void Update() {

        if (isLocalPlayer) {
            DoChangeDirection();
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime > (1 / movesPerSecond)) {
            elapsedTime -= 1 / movesPerSecond;
            DoTick();
        }

        cameraTarget.position += Vector3.up * (links.Count + 1);
    }

    void DoChangeDirection() {
        var newDirection = currentDirection;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) { newDirection = Up.I; }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { newDirection = Right.I; }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) { newDirection = Down.I; }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) { newDirection = Left.I; }

        if (!invalidTurn(currentDirection, newDirection)) {
            currentDirection = newDirection;
        }

        snakeHead.SetRotationOfVisual(currentDirection.GetHeadRotation());
    }

    private static bool invalidTurn(Direction currentDir, Direction newDir) {
        if ((newDir == Down.I && currentDir == Up.I) ||
            (newDir == Left.I && currentDir == Right.I) ||
            (newDir == Up.I && currentDir == Down.I) ||
            (newDir == Right.I && currentDir == Left.I)) {

            return true;
        } else {
            return false;
        }
    }

    void UpdateTickText() {
        tickUI.text = currentTick.ToString();
    }

    void DoTick() {
        if (isLocalPlayer) {
            DoAppleEatCheck();
        }

        currentTick++;
        UpdateTickText();

        Move();

        // AfterTick?.Invoke();
    }

    void Move() {
        Vector3 newPosition = Vector3.zero;
        if (isLocalPlayer) {
            newPosition = networkHead.transform.position + currentDirection.GetMoveVector();

            if (AboutToCollideWithSelf(this, newPosition)) {
                Die();
            }

            if (links.Count > 0) {
                var oldTail = links.Last();
                links.RemoveAt(links.Count - 1);
                links.Insert(0, oldTail);
                oldTail.transform.position = snakeHead.transform.position;
            }
        }

        if (isLocalPlayer) {
            networkHead.transform.position = newPosition;
        }

        snakeHead.transform.position = networkHead.transform.position;
    }

    private bool AboutToCollideWithSelf(BravoSnake snake, Vector3 newPosition) {
        return snake.links.Any(x => x.transform.position == newPosition);
    }

    void DoAppleEatCheck() {
        AppleManager.all.ForEach(x => {
            if (x.transform.position == snakeHead.transform.position) {
                EatApple(x.gameObject);
            }
        });
    }

    void EatApple(GameObject apple) {
        KillAppleNewTail(apple);
        String appleId = apple.name;
        CmdEatApple(appleId);
    }

    [Command]
    private void CmdEatApple(String appleId) {
        RpcEatApple(appleId);
    }

    [ClientRpc]
    public void RpcEatApple(String appleId) {
        if (!isLocalPlayer) {
            GameObject apple = GameObject.Find(appleId);
            KillAppleNewTail(apple);
        }
    }

    void KillAppleNewTail(GameObject apple) {
        apple.SetActive(false);
        NewTail();
    }

    readonly Vector3 zeroVector = Vector3.zero;

    GameObject NewTail() {
        return NewTail(Vector3.zero);
    }

    GameObject NewTail(Vector3 position) {
        var newTail = Instantiate(snakeTailPrefab, position, Quaternion.identity, transform);
        links.Add(newTail);
        gameObject.SetActive(false);
        var netTransChild = gameObject.AddComponent<NetworkTransformChild>();
        netTransChild.target = newTail.transform;
        netTransChild.interpolateMovement = 1;
        netTransChild.interpolateRotation = 1;
        // netTransChild.sendInterval = 29;
        gameObject.SetActive(true);
        return newTail;
    }

    // public void SetSnakeData(SnakeState state) {
    //     this.head.transform.position = state.headPosition;
    //     this.currentTick = state.tick;
    //     this.elapsedTime = state.elapsedTime;
    //     this.currentDirection = state.direction;
    //     this.links = state.linkPositions.Select(x => Instantiate(snakeTailPrefab, x, Quaternion.identity, this.transform)).ToList();
    // }

    public void SetSnakeData(SnakeState state) {
        this.links = state.linkPositions.Select(x => NewTail(x)).ToList();
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
        if (other.gameObject.HasComponent<Wall>() || (other.gameObject.HasComponent<SnakeTailContainer>())) {
            Die();
        }
    }
}
