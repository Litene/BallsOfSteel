using System;
using AI;
using Game;
using Graphs;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Gustav_Carlberg {
	[System.Serializable] public class Unit_Gustav_Carlberg : Unit {


		#region Properties

		public Team_Gustav_Carlberg Team => base.Team as Team_Gustav_Carlberg;

		#endregion
		

		// selects the target with least health
		protected override Unit SelectTarget(List<Unit> enemiesInRange) {
			Unit lowestHealthUnit = null;
			float lowestHealth = float.MaxValue;
			foreach (var enemy in enemiesInRange) {
				if (enemy.Health < lowestHealth) {
					lowestHealth = enemy.Health;
					lowestHealthUnit = enemy;
				}
			}
			return lowestHealthUnit;
		}

		protected override GraphUtils.Path GetPathToTarget() {
			return Team.GetShortestPath(CurrentNode, TargetNode);
		}
	}
}