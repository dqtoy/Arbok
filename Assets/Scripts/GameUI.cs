using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
	public static GameUI I;

	public Text mainGameText;
	public Text alivePlayersText;
	public string alivePlayersTemplate = "";

	void Awake() {
		I = this;
	}

	void Update() {

	}

	public void SetMainGameText(string text) {
		mainGameText.text = text;
	}

	public void SetAlivePlayersText(int alivePlayerCount) {
		alivePlayersText.text = alivePlayersTemplate + alivePlayerCount;
	}
}
