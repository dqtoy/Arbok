using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkApple : NetworkBehaviour {

    [ServerCallback]
    public void DestroyOnServer() {
        Destroy(gameObject);
    }

    public void SetVisible(bool isVisible) {
        gameObject.SetActive(isVisible);
    }
}
