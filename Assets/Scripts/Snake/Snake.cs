using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour {

    public GameObject head;


    public float movesPerSecond = 5;
    float elapsedTime = 0;

    Direction direction = Up.I;

    void Start () {
		
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
            transform.Translate(speed * direction.GetMoveVector());
            // Move here
        }
    }

    void Grow() {

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
