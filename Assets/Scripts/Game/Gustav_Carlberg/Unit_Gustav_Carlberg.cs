using System;
using AI;
using Game;
using Graphs;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gustav_Carlberg {
	[System.Serializable] public class Unit_Gustav_Carlberg : Unit {


		#region Properties

		public Team_Gustav_Carlberg Team => base.Team as Team_Gustav_Carlberg;

		#endregion

		private Unit_Gustav_Carlberg m_teamCaptain = null;

		private Brain m_brain;

		public bool fleeing;
		
		public Unit_Gustav_Carlberg Captain;

		protected override void Start() {
			base.Start();
			m_brain = GetComponent<Brain>();
			List<Unit_Gustav_Carlberg> tempTeammates = new List<Unit_Gustav_Carlberg>();
			foreach (var unit in Team.m_myUnits) {
				if (unit != this) {
					tempTeammates.Add(unit);
				}
			}

			m_brain.Tree.StartTree(m_brain);
			m_brain.Tree.Blackboard.SetValue("TeamMates", tempTeammates);
			
			if (this.name != "Capn Crunch") {
				foreach (var unit in m_brain.Tree.Blackboard.GetValue("TeamMates", new List<Unit_Gustav_Carlberg>())) {
					if (unit.name == "Capn Crunch") {
						Captain = unit;
						m_brain.Tree.Blackboard.SetValue("Captain", unit);
					}
				}
			}
		}

		protected override Unit SelectTarget(List<Unit> enemiesInRange) {
			// focus fire
			Unit lowestHealthUnit = null;
			int lowestHealth = int.MaxValue;
			//var tempList = enemiesInRange.Sort(unit => unit.Health);
			foreach (var enemy in enemiesInRange) {
				if (enemy.Health < lowestHealth) {
					lowestHealth = enemy.Health;
					lowestHealthUnit = enemy;
				}
			}
			
			return lowestHealthUnit;
		}
	}
}