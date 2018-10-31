using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class GameState : NetworkBehaviour {
	[Server]
	public abstract GameState GetNextState();

	[Server]
	public virtual void Enter() { }

	[Server]
	public virtual void Exit() { }
}
