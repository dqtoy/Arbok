using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeDebug : MonoBehaviour, ISerializationCallbackReceiver {

	public Snake snake;

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

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
