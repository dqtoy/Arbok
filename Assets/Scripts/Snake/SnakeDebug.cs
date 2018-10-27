using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnakeDebug : MonoBehaviour {

	public Snake snake;
	public Text eventsText;

	void Start() { }

	void Update() {
		eventsText.text = snake.snakeEvents.ToString().Truncate(300);

		if (Input.GetKeyDown(KeyCode.L)) {
			eventsText.enabled = !eventsText.enabled;
		}
	}
}
