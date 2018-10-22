using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {

    public static DebugScreen I;

    public Text globalTickText;
    public Text globalTickNetID;

    Text text;

    void Awake() {
        I = this;

        text = transform.Find("Panel/Text").GetComponent<Text>();
    }

    public void Log(string log) {
        // text.text = log + "\n" + text.text;
    }

    public void ToggleVisibility() {
        text.gameObject.SetActive(!text.gameObject.activeSelf);
    }

    public string output = "";
    public string stack = "";

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        if (type == LogType.Error || type == LogType.Exception) {
            text.text = stackTrace + "\n" + text.text;
        }
        text.text = logString + "\n" + text.text;
        output = logString;
        stack = stackTrace;
    }
}
