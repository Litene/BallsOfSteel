using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Nodes;
using Game;
using UnityEngine;

public class Flee_Gustav_Carlberg : ActionNode {
	// lacking information
	// need to access Unit
	protected override void OnStart() {
		
	}

	protected override void OnStop() {
		Tree.Blackboard.SetValue("HasFled", true);
	}

	protected override State OnUpdate() {
		return State.Success;
	}
}