using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BravoNetworkSnakeController : NetworkBehaviour {
    public Text netIdUI;

    public BravoSnake snake;

    public override void OnStartLocalPlayer() {
        // CmdRequestSnakePositions();
        netIdUI.text = netId.ToString() + "*";
        MainCamera.I.target = snake.cameraTarget;
        // CmdRequestApplePositions();
    }

    void Update() { }
}
