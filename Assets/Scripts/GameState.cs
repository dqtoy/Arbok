using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class GameState : NetworkBehaviour {
	public abstract GameState GetNextState();

	public virtual void Enter() { }

	public virtual void Exit() { }
}
