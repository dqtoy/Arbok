using System;
using System.Collections;
using System.Collections.Generic;
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

    void Awake () {
        head = GameObject.Instantiate(snakeLinkPrefab);
        tail = head;
        head.transform.parent = transform;
		links.Insert(0, head);
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = Up.I;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Left.I;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Down.I;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            direction = Right.I;
        }

        elapsedTime += Time.deltaTime;
        if(elapsedTime > (1 / movesPerSecond)) {
            elapsedTime -= 1 / movesPerSecond;

            float speed = 1.0f;
            
            Vector3 newPosition = head.transform.position + speed * direction.GetMoveVector();

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

    void Grow() {
        Debug.Log("GROW");
        growOnNextMove = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.HasComponent<Wall>()) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (other.gameObject.HasComponent<Apple>())
        {
            Grow();
        }
    }
}
