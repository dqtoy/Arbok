using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeParity : NetworkBehaviour {
	public Snake snake;
	[Range(1, 60)]
	public int checkRateSeconds = 5;

	void Start() {
		if (isLocalPlayer) {
			StartCoroutine(Check());
		}
	}

	IEnumerator Check() {
		yield return new WaitForEndOfFrame();

		while (true) {
			yield return new WaitForSeconds(checkRateSeconds);

			int tickToCheck = snake.currentTick - ((int) Mathf.Ceil(snake.movesPerSecond) * checkRateSeconds);
			var expectedHeadPosition = snake.GetHeadPositionAtTick(tickToCheck);
			CmdCheckHeadParity(tickToCheck, expectedHeadPosition);
		}
	}

	[Command]
	void CmdCheckHeadParity(int tick, Vector3 expectedHeadPosition) {
		RpcCheckHeadParity(tick, expectedHeadPosition);

		DoHeadCheck(tick, expectedHeadPosition);
	}

	[ClientRpc]
	void RpcCheckHeadParity(int tick, Vector3 expectedHeadPosition) {
		if (!isLocalPlayer) {
			DoHeadCheck(tick, expectedHeadPosition);
		}
	}

	void DoHeadCheck(int tick, Vector3 expectedHeadPosition) {

		var actualHeadPosition = snake.GetHeadPositionAtTick(tick);

		if (actualHeadPosition != expectedHeadPosition) {
			throw new UnityException("CmdCheckHeadParity check failed! Expected head position: " + expectedHeadPosition + " | Actual: " + actualHeadPosition + " | at tick: " + tick + " | netId: " + netId);
		}

        Toolbox.Log("DoHeadCheck PASS");
	}

	void Update() {

	}
}
