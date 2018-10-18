using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {
	public static MainCamera I;

	public Transform target;

	public float moveSpeed = 1;

	// Use this for initialization
	void Awake() {
		I = this;
		target = this.transform;
	}

	// Update is called once per frame
	void Update() {
		if (target == null) target = this.transform;

		var vectorToTarget = target.position - transform.position;

		vectorToTarget.y = 0;

		transform.position += vectorToTarget * Time.deltaTime * moveSpeed;
	}
}
