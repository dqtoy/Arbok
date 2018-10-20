using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbox : MonoBehaviour {

    public static void Log(string text) {
        Debug.Log(text);
        DebugScreen.I.Log(text);
    }
}
