using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour {
    public GameObject snakeLinkPrefab;
    public List<GameObject> links = new List<GameObject>();

    public float movesPerSecond = 5;

    GameObject head;
    GameObject tail;
    float elapsedTime = 0;
    bool growOnNextMove = false;
    Direction direction = Up.I;
    Direction nextDirection = Up.I;

    void Awake () {
        head = GameObject.Instantiate(snakeLinkPrefab);
        tail = head;
        head.transform.parent = transform;
		links.Insert(0, head);
	}
	
	void Update () {
        if (direction != Down.I && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            nextDirection = Up.I;
        }
        if (direction != Left.I && (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)))
        {
            nextDirection = Right.I;
        }
        if (direction != Up.I && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            nextDirection = Down.I;
        }
        if (direction != Right.I && (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)))
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
                head = newLink;
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

    private bool AboutToCollideWithSelf(Vector3 newPosition)
    {
        return links.Any(x => x.transform.position == newPosition);
    }

    void Die() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Grow() {
        Debug.Log("GROW");
        growOnNextMove = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.HasComponent<Wall>() || other.gameObject.HasComponent<SnakeLink>()) {
            Die();
        }
        if (other.gameObject.HasComponent<Apple>())
        {
            Grow();
        }
    }
}
