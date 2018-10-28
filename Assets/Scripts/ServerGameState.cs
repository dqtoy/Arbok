using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class ServerGameState : NetworkBehaviour {
	public abstract ServerGameState GetNextState();

	public virtual void Enter() { }

	public virtual void Exit() { }
}
