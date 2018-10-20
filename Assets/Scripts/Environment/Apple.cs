using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apple : MonoBehaviour {

	// Use this for initialization
	void Awake() {
		AppleManager.all.Add(this);
	}

	// Update is called once per frame
	void Update() {

	}

	public AppleState ToState() {
		return new AppleState() {
			position = transform.position,
				isActive = gameObject.activeSelf
		};
	}
}
