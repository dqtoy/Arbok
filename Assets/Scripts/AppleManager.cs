using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleManager : MonoBehaviour {
	public static AppleManager I;
	public static List<Apple> all = new List<Apple>();
	public GameObject applePrefab;

	// Use this for initialization
	void Awake() {
		I = this;
	}

	// Update is called once per frame
	void Update() {

	}

	public static void DestroyAll() {
		all.ForEach(x => Destroy(x.gameObject));
	}

	public void SpawnApple(AppleState state) {
		var newApple = Instantiate(applePrefab, state.position, Quaternion.identity, transform);
		newApple.SetActive(state.isActive);
	}
}
