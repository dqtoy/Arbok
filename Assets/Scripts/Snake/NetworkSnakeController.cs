using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSnakeController : NetworkBehaviour, ISnakeController
{

    Direction nextDirection = Up.I;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(!isLocalPlayer) {
            return;
        }	

        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            CmdKeyDownUp();
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            CmdKeyDownRight();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            CmdKeyDownDown();
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CmdKeyDownLeft();
        }
    }

    // ==============================
    // Network
    // ==============================
    [Command]
    private void CmdKeyDownUp()
    {
        RpcKeyDownUp();
    }

    [ClientRpc]
    public void RpcKeyDownUp()
    {
        nextDirection = Up.I;
    }

    [Command]
    private void CmdKeyDownDown()
    {
        RpcKeyDownDown();
    }

    [ClientRpc]
    public void RpcKeyDownDown()
    {
        nextDirection = Down.I;
    }

    [Command]
    private void CmdKeyDownLeft()
    {
        RpcKeyDownLeft();
    }

    [ClientRpc]
    public void RpcKeyDownLeft()
    {
        nextDirection = Left.I;
    }

    [Command]
    private void CmdKeyDownRight()
    {
        RpcKeyDownRight();
    }

    [ClientRpc]
    public void RpcKeyDownRight()
    {
        nextDirection = Right.I;
    }


    // ==============================
    // ISnakeController
    // ==============================
    public bool IsDownButtonPressed()
    {
        return nextDirection == Down.I;
    }

    public bool IsLeftButtonPressed()
    {
        return nextDirection == Left.I;
    }

    public bool IsRightButtonPressed()
    {
        return nextDirection == Right.I;
    }

    public bool IsUpButtonPressed()
    {
        return nextDirection == Up.I;
    }
}
