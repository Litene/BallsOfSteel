using System;
using Game;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gustav_Carlberg {
	public class Team_Gustav_Carlberg : Team {
		[SerializeField] private Color m_myFancyColor;

		private Dictionary<string, Unit_Gustav_Carlberg>
			m_myTeamLookup = new();

		public float TooFarApartThreshold = 12f;

		public List<Unit_Gustav_Carlberg> m_myUnits;

		public float fleeDistance = 5.0f;

		public Vector3 AverageEnemyTeamPos => EnemyTeam.Units.Aggregate(new Vector3(0, 0, 0),
			(s, v) => s + v.transform.position / (float)EnemyTeam.Units.Count());

		public Vector3 AverageTeamPos => m_myUnits.Aggregate(new Vector3(0, 0, 0),
			(s, v) => s + v.transform.position / (float)m_myUnits.Count());

		public Vector3 FleeVector => (AverageEnemyTeamPos - AverageTeamPos);

		protected override void Start() {
			base.Start();
			m_myUnits = GetComponentsInChildren<Unit_Gustav_Carlberg>().ToList();
			foreach (var unit in m_myUnits) {
				m_myTeamLookup.Add(unit.name, unit);
			}
		}

		bool TooFarApart() {
			float combinedMagnitude = 0;
			foreach (var unit in m_myUnits) {
				Vector3 targetVector = AverageTeamPos - unit.transform.position;
				combinedMagnitude += Vector3.Magnitude(targetVector);
			}

			if (combinedMagnitude > TooFarApartThreshold) {
				return true;
			}

			return false;
		}

		private void Update() {
			foreach (var unit in m_myUnits) {
				if (unit == null) continue;

				
				if (TooFarApart() && !unit.fleeing) {
					unit.TargetNode = GraphUtils.GetClosestNode<Battlefield.Node>(Battlefield.Instance, AverageTeamPos);
				}
				else if (unit.EnemiesInRange.Any() && !unit.fleeing) {
					var sortedList = unit.EnemiesInRange
						.OrderBy(x => Vector3.Distance(x.transform.position, unit.transform.position)).ToList();
					unit.TargetNode =
						GraphUtils.GetClosestNode<Battlefield.Node>(Battlefield.Instance,
							sortedList[0].transform.position);
				}
				else if (!unit.fleeing) {
					unit.TargetNode =
						GraphUtils.GetClosestNode<Battlefield.Node>(Battlefield.Instance, AverageEnemyTeamPos);
				}
			}
		}

		private void OnDrawGizmos() {
			Gizmos.color = Color.red;
			Gizmos.DrawCube(AverageEnemyTeamPos, new Vector3(1, 1, 1));

			Gizmos.color = Color.magenta;
			foreach (var unit in m_myUnits) {
				if (unit == null) continue;
				if (unit.TargetNode != null) {
					Vector3 inversedir = -(AverageEnemyTeamPos - unit.transform.position).normalized * 5;
					Gizmos.DrawRay(unit.transform.position, inversedir);
				}
			}
		}

		#region Properties

		public override Color Color => m_myFancyColor;

		#endregion

	}
}