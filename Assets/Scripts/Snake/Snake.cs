using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour {

    public static List<Snake> snakes = new List<Snake>();

    public GameObject snakeLinkPrefab;
    public List<GameObject> links = new List<GameObject>();

    public float movesPerSecond = 5;

    GameObject head;
    GameObject tail;
    float elapsedTime = 0;
    bool growOnNextMove = false;
    Direction direction = Up.I;
    Direction nextDirection = Up.I;
    public ISnakeController controller;

    void Awake () {
        snakes.Add(this);
        head = GameObject.Instantiate(snakeLinkPrefab);
        head.tag = "Head";
        tail = head;
        head.transform.position = transform.position;
        head.transform.parent = transform;
		links.Insert(0, head);
        nextDirection = direction;
	}
	
	void Update () {
        if (direction != Down.I && controller.IsUpButtonPressed())
        {
            nextDirection = Up.I;
        }
        if (direction != Left.I && controller.IsRightButtonPressed())
        {
            nextDirection = Right.I;
        }
        if (direction != Up.I && controller.IsDownButtonPressed())
        {
            nextDirection = Down.I;
        }
        if (direction != Right.I && controller.IsLeftButtonPressed())
        {
            nextDirection = Left.I;
        }

        elapsedTime += Time.deltaTime;

        if(elapsedTime > (1 / movesPerSecond)) {
            elapsedTime -= 1 / movesPerSecond;

            float speed = 1.0f;

            direction = nextDirection;
            
            Vector3 newPosition = head.transform.position + speed * direction.GetMoveVector();

            if (AboutToCollideWithSelf(newPosition)) {
                Die();
            }

            if (growOnNextMove) {
                growOnNextMove = false;
                var newLink = GameObject.Instantiate(snakeLinkPrefab);
                newLink.transform.parent = transform;
                newLink.transform.position = newPosition;
                head.tag = "Tail";
                head = newLink;
                head.tag = "Head";
                links.Insert(0, head);
            } else {
                var tail = links[links.Count - 1];
                head = tail;
                links.RemoveAt(links.Count - 1);
                links.Insert(0, tail);
                tail = links[links.Count - 1];
                head.transform.position = newPosition;
            }
        }
    }

	private bool AboutToCollideWithSelf(Vector3 newPosition) {
        return links.Any(x => x.transform.position == newPosition);
	}

    private bool AboutToCollideWithAnySnake(Vector3 newPosition)
    {
        return snakes.Any(x => AreWeGoingToCollideWithOtherSnake(x, newPosition));
    }

    private bool AreWeGoingToCollideWithOtherSnake(Snake otherSnake, Vector3 newPosition)
    {
        return otherSnake.links.Any(x => x.transform.position == newPosition);
    }

    void Die() {
        snakes.Remove(this);
        GameObject.Destroy(gameObject);
    }

    void Grow() {
        growOnNextMove = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.HasComponent<Wall>() || (other.gameObject.HasComponent<SnakeLink>() && other.tag == "Tail")) {
            Die();
        }
        if (other.gameObject.HasComponent<Apple>())
        {
            Destroy(other.gameObject);
            Grow();
        }
    }
}
