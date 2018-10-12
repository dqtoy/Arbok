using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSnakeController : ISnakeController {
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override bool IsUpButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
    }

    public override bool IsRightButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
    }

    public override bool IsDownButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
    }

    public override bool IsLeftButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    }
}
