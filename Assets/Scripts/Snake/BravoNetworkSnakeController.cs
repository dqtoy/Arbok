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
        CmdRequestSnakePositions();
        CmdRequestDeadAppleNames();
        netIdUI.text = netId.ToString() + "*";
        MainCamera.I.target = snake.cameraTarget;
    }

    [Command]
    public void CmdRequestDeadAppleNames() {
        Toolbox.Log("CmdRequestApplePositions");
        TargetReceiveDeadAppleNames(
            connectionToClient,
            JsonConvert.SerializeObject(AppleManager.all.Where(x => x.gameObject.activeSelf == false).Select(x => x.gameObject.name).ToArray())
        );
    }

    [TargetRpc]
    public void TargetReceiveDeadAppleNames(NetworkConnection connection, string deadAppleNamesJson) {
        if (!isServer) {
            JsonConvert.DeserializeObject<string[]>(deadAppleNamesJson).ToList().ForEach(deadAppleName => {
                AppleManager.all.Find(x => x.gameObject.name == deadAppleName).gameObject.SetActive(false);
            });
        } else {
            // AppleManager.EnableAll();
        }
    }

    [Command]
    public void CmdRequestSnakePositions() {
        BravoSnake.all.ForEach(
            x => {
                var linksJson = JsonConvert.SerializeObject(x.links.Select(y => y.transform.position).ToArray());
                TargetReceiveSnakePosition(
                    connectionToClient,
                    linksJson,
                    x.GetComponent<NetworkIdentity>().netId
                );
            }
        );
    }

    [TargetRpc]
    public void TargetReceiveSnakePosition(NetworkConnection connection, string linksJson, NetworkInstanceId netId) {
        if (netId == this.netId) return;

        var snakeToModify = BravoSnake.all.First(x => x.GetComponent<NetworkIdentity>().netId == netId);

        snakeToModify.SetSnakeData(new SnakeState() {
            linkPositions = JsonConvert.DeserializeObject<Vector3[]>(linksJson),
        });
    }

    void Update() { }
}
