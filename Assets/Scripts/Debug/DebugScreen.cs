using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {

    public static DebugScreen I;

    public Text globalTickText;
    public Text globalTickNetID;
    public Material redMat;
    public Material blueMat;
    public GameObject plane;

    Text text;

    void Awake() {
        I = this;

        text = transform.Find("Panel/Text").GetComponent<Text>();
    }

    public void ToggleVisibility() {
        text.gameObject.SetActive(!text.gameObject.activeSelf);
    }

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        if (type == LogType.Error || type == LogType.Exception) {
            text.text = (stackTrace + "\n" + text.text).Truncate(1000);
        }

        text.text = (logString + "\n" + text.text).Truncate(1000);
    }

    public void SetPlaneRed() {
        plane.GetComponent<MeshRenderer>().material = redMat;
    }

    public void SetPlaneBlue() {
        plane.GetComponent<MeshRenderer>().material = blueMat;
    }
}
