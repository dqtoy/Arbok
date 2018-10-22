using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {
	public static MainCamera I;

	public Transform target;
	new Camera camera;

	public float moveSpeed = 1;
	float startSize;

	// Use this for initialization
	void Awake() {
		I = this;
		target = this.transform;
		camera = GetComponent<Camera>();
		startSize = camera.orthographicSize;
	}

	// Update is called once per frame
	void Update() {
		if (target == null) target = this.transform;

		var vectorToTarget = target.position - transform.position;

		vectorToTarget.y = 0;

		transform.position += vectorToTarget * Time.deltaTime * moveSpeed;

		var targetSize = startSize + (target.localScale.x - 1) / 4;

		var amountToTargetSize = targetSize - camera.orthographicSize;

		camera.orthographicSize += amountToTargetSize / 2 * Time.deltaTime;
	}
}
