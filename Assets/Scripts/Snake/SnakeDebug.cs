using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnakeDebug : MonoBehaviour, ISerializationCallbackReceiver {

	public Snake snake;
	public Text eventsText;

	// Use this for initialization
	void Start() {
		// snake.AfterTick += () => { };
	}

	// Update is called once per frame
	void Update() {
		eventsText.text = snake.snakeEvents.ToString();
	}

	// Debug info

	public List<string> keys = new List<string>();

	public void OnBeforeSerialize() {
		// keys.Clear();

		// foreach (var kvp in snake.snakeEvents) {
		// 	foreach (var kvp2 in snake.snakeEvents) {
		// 		keys.Add(kvp.Key + " " + kvp.Value.previousDirection.Serialize().ToString() + " " + kvp.Value.newDirection.Serialize().ToString());
		// 	}
		// }
	}

	public void OnAfterDeserialize() { }

	void OnGUI() {
		// foreach (var kvp in snake.snakeEvents) {
		// 	GUILayout.Label(kvp.Key + " " + kvp.Value.previousDirection.Serialize().ToString() + " " + kvp.Value.newDirection.Serialize().ToString());
		// }
	}
}
