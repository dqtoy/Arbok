using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {
	public static MainCamera I;

	public Transform target;
	new Camera camera;

	public float moveSpeed = 1;
	float startSize;
	float startYPosition;

	// Use this for initialization
	void Awake() {
		I = this;
		camera = GetComponent<Camera>();
		startSize = camera.orthographicSize;
		startYPosition = transform.position.y;
	}

	// Update is called once per frame
	void Update() {
		if (target == null) return;

		var targetYPosition = startYPosition + target.position.y;

		var targetActual = new Vector3(target.position.x, targetYPosition, target.position.z);

		var vectorToTarget = targetActual - transform.position;

		transform.position += vectorToTarget * Time.deltaTime * moveSpeed;

		if (camera.orthographic) {
			var targetSize = startSize + (target.localScale.x - 1) / 4;

			var amountToTargetSize = targetSize - camera.orthographicSize;

			camera.orthographicSize += amountToTargetSize / 2 * Time.deltaTime;
		} else {

		}
	}
}
