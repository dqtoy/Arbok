using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {

    public static DebugScreen I;

    Text text;

	void Awake () {
        I = this;

        text = transform.Find("Panel/Text").GetComponent<Text>();
    }

    public void Log(string log) {
        text.text = log + "\n" + text.text;
    }

    public void ToggleVisibility() {
        text.gameObject.SetActive(!text.gameObject.activeSelf);
    }
}
