using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {
	public static IList<Wall> all = new List<Wall>();

	// Use this for initialization
	void Awake() {
		all.Add(this);
	}

	// Update is called once per frame
	void Update() {

	}

	void OnDestroy() {
		all.Remove(this);
	}
}
