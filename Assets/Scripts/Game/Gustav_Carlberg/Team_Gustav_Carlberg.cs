using System;
using Game;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Gustav_Carlberg;
using UnityEngine;

namespace Gustav_Carlberg {
	public class Team_Gustav_Carlberg : Team {

		#region Serialized Members

		[SerializeField] private Color m_myFancyColor;

		#endregion

		private Data_Gustav_Carlberg m_data = new();
		private Unit m_huntedUnit = null;

		private float SquareMagnitude = 0;

		#region Constants

		private const float TooFarApartThreshold = 12f;
		private const int MaxSearchDistance = 6;

		#endregion

		#region Data Structures

		private float[] m_firePowerSteps = new[] { -0.2f, -0.4f, -0.6f, -0.8f, -1.0f };
		private List<Battlefield.Node> m_covers = new();
		private List<Battlefield.Node> m_sortedNodes = new();
		private List<Battlefield.Node> m_closestNodes = new();
		public List<Unit_Gustav_Carlberg> m_myUnits { get; private set; }
		public Dictionary<Vector2Int, Battlefield.Node> m_nodeLookup = new Dictionary<Vector2Int, Battlefield.Node>();

		private readonly Vector2Int[] m_directions = new Vector2Int[] {
			new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
			new Vector2Int(-1, -1), new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1)
		};

		#endregion

		#region Properties

		public override Color Color => m_myFancyColor;

		private Vector3 AverageEnemyTeamPos => EnemyTeam.Units.Aggregate(new Vector3(0, 0, 0),
			(compareVector, unit) => compareVector + unit.transform.position / this.EnemyTeam.Units.Count());

		#endregion

		// find average team pos
		public Vector3 AverageTeamPos() {
			//initialize vector to zero
			Vector3 calculatedVector = Vector3.zero;
			//count units
			int count = 0;
			foreach (var unit in m_myUnits) {
				//gaurd clause if m_myUnits contains a null value
				if (unit == null) continue;

				//count units in list
				count++;
				//add to collectedVector
				calculatedVector += unit.transform.position;
			}

			//divide by the count to get the average
			return calculatedVector / count;
		}

		IEnumerator UpdateData() {
			//infinite loop to update the data within m_data
			while (true) {
				SquareMagnitude = (AverageEnemyTeamPos - AverageTeamPos()).sqrMagnitude;
				yield return null;
				m_data.CalculateInfluence(m_myUnits.Cast<Unit>().ToList(), EnemyTeam.Units.ToList(), Battlefield.Instance.Nodes)
					.CalculateFirePower(EnemyTeam.Units.ToList(), Battlefield.Instance.Nodes);
				yield return null;
				//add separate list for covers
				m_covers = m_data.CalculateCover(EnemyTeam.Units.ToList(), Battlefield.Instance.Nodes);
				yield return null;
			}
		}

		protected override void Start() {
			base.Start();
			//Find all my units
			Time.timeScale = 30.0f;
			m_myUnits = GetComponentsInChildren<Unit_Gustav_Carlberg>().ToList();

			//create vector2Int lookup table
			foreach (var node in Battlefield.Instance.Nodes) {
				if (node is Battlefield.Node currNode) {
					m_nodeLookup.Add(currNode.Position, currNode);
				}
			}

			//start coroutines
			StartCoroutine(UpdateData());
			//StartCoroutine(UpdateBestNode());
		}


		private Battlefield.Node FindOptimalNode() {
			//find all nodes with low enemy firepower

			m_sortedNodes = new List<Battlefield.Node>();
			for (int i = 0; i < m_firePowerSteps.Length; i++) {
				foreach (var value in m_nodeLookup.Values) {
					if (m_data.GetNodeValue(value, out DataWrapper wrapper)) {
						if (m_firePowerSteps != null && Equals(wrapper.EnemyFirePower, m_firePowerSteps[i])) {
							if (!m_sortedNodes.Contains(value) && (value is not Game.Node_Mud)) {
								m_sortedNodes.Add(value);
							}
						}
					}
				}

				//break if we find more than 50 nodes
				if (m_sortedNodes.Count() > 50) break;
			}

			// sort by distance to average team position
			List<Battlefield.Node> sortedByDistanceNodes =
				m_sortedNodes.OrderBy(x => Vector3.Distance(x.WorldPosition, AverageTeamPos())).ToList();
			m_closestNodes = new List<Battlefield.Node>();
			//add the 10 closest
			for (int i = 0; i < sortedByDistanceNodes.Count; i++) {
				m_closestNodes.Add(sortedByDistanceNodes[i]);
				if (m_closestNodes.Count >= 10) break;
			}


			//find node with highest Influence value
			Battlefield.Node bestNode = null;
			float highestValue = float.MinValue;
			foreach (var closestNode in m_closestNodes) {
				if (m_data.GetEntry(closestNode).Influence is float currValue) {
					if (bestNode == null || currValue > highestValue) {
						bestNode = closestNode;
						highestValue = currValue;
					}
				}
			}

			//return best node found
			return bestNode;
		}

		private Battlefield.Node FindOptimalNodeWithinDistance(Battlefield.Node node, int distance = MaxSearchDistance) {
			HashSet<Battlefield.Node> almostNeighbors =
				GraphUtils.GetNodesWithinDistance<Battlefield.Node>(Battlefield.Instance,
					node,
					distance == MaxSearchDistance ? MaxSearchDistance : distance);

			m_sortedNodes = new List<Battlefield.Node>();
			for (int i = 0; i < m_firePowerSteps.Length; i++) {
				foreach (var value in almostNeighbors) {
					if (m_data.GetNodeValue(value, out DataWrapper wrapper)) {
						if (m_firePowerSteps != null && Equals(wrapper.EnemyFirePower, m_firePowerSteps[i])) {
							if (!m_sortedNodes.Contains(value) && (value is not Game.Node_Mud)) {
								m_sortedNodes.Add(value);
							}
						}
					}
				}
			}

			// sort by distance to average team position
			List<Battlefield.Node> sortedByDistanceNodes =
				m_sortedNodes.OrderBy(x => Vector3.Distance(x.WorldPosition, AverageTeamPos())).ToList();
			m_closestNodes = new List<Battlefield.Node>();
			//add the 10 closest
			for (int i = 0; i < sortedByDistanceNodes.Count; i++) {
				m_closestNodes.Add(sortedByDistanceNodes[i]);
				if (m_closestNodes.Count >= 10) break;
			}


			//find node with highest Influence value
			Battlefield.Node bestNode = null;
			float highestValue = float.MinValue;
			foreach (var closestNode in m_closestNodes) {
				if (m_data.GetEntry(closestNode).Influence is float currValue) {
					if (bestNode == null || currValue > highestValue) {
						bestNode = closestNode;
						highestValue = currValue;
					}
				}
			}

			//return best node found
			return bestNode;
		}

		public new GraphUtils.Path GetShortestPath(Battlefield.Node start, Battlefield.Node goal) {
			if (start == null ||
			    goal == null ||
			    start == goal ||
			    Battlefield.Instance == null) {
				return null;
			}

			// initialize pathfinding
			foreach (Battlefield.Node node in Battlefield.Instance.Nodes) {
				node?.ResetPathfinding();
			}

			// add start node
			start.m_fDistance = 0.0f;
			start.m_fRemainingDistance = Battlefield.Instance.Heuristic(goal, start);
			List<Battlefield.Node> open = new List<Battlefield.Node>();
			HashSet<Battlefield.Node> closed = new HashSet<Battlefield.Node>();
			open.Add(start);

			// search
			while (open.Count > 0) {
				// get next node (the one with the least remaining distance)
				Battlefield.Node current = open[0];
				for (int i = 1; i < open.Count; ++i) {
					if (open[i].m_fRemainingDistance < current.m_fRemainingDistance) {
						current = open[i];
					}
				}

				open.Remove(current);
				closed.Add(current);

				// found goal?
				if (current == goal) {
					// construct path
					GraphUtils.Path path = new GraphUtils.Path();
					while (current != null) {
						path.Add(current.m_parentLink);
						current = current != null && current.m_parentLink != null ? current.m_parentLink.Source : null;
					}

					path.RemoveAll(l => l == null); // HACK: check if path contains null links
					path.Reverse();
					return path;
				}
				else {
					foreach (Battlefield.Link link in current.Links) {
						if (link.Target is Battlefield.Node target) {
							if (!closed.Contains(target) &&
							    target.Unit == null) {
								if (m_data.GetEntry(target).Cover is { } coverValued) {
									float newDistance = current.m_fDistance +
									                    Vector3.Distance(current.WorldPosition, target.WorldPosition) +
									                    (target.AdditionalCost);
									float newRemainingDistance =
										newDistance + Battlefield.Instance.Heuristic(target, start);

									if (open.Contains(target)) {
										if (newRemainingDistance < target.m_fRemainingDistance) {
											// re-parent neighbor node
											target.m_fRemainingDistance = newRemainingDistance;
											target.m_fDistance = newDistance;
											target.m_parentLink = link;
										}
									}
									else {
										// add target to openlist
										target.m_fRemainingDistance = newRemainingDistance;
										target.m_fDistance = newDistance;
										target.m_parentLink = link;
										open.Add(target);
									}
								}
							}
						}
					}
				}
			}

			return null;
		}

		//find closest cover node within a distance of MaxSearchDistance(6)
		private Battlefield.Node FindNearbyCover(Battlefield.Node node) {
			var nearbyNodes = GraphUtils.GetNodesWithinDistance(Battlefield.Instance, node, MaxSearchDistance);
			Battlefield.Node bestNode = null;
			foreach (var nearbyNode in nearbyNodes) {
				if (bestNode == null || m_data.GetEntry(nearbyNode).Cover > m_data.GetEntry(bestNode).Cover) {
					bestNode = nearbyNode;
				}
			}

			return bestNode;
		}

		private void Update() {
			foreach (var unit in m_myUnits) {
				if (unit == null) continue;

				//AI logic, acts a replacement of a selector node
				//First check checks if any enemies are in range and if so, do they have less then 3 units less if so then rambo mode is activated,
				//which means we hunt the team down without mercy. 
				if (unit.EnemiesInRange.Any() && EnemyTeam.Units.Count() <= m_myUnits.Count - 3) {
					//we then sort by closest the list of enemies in range to find the closest one. 
					var sortedList = unit.EnemiesInRange // would want same unit
						.OrderBy(x => Vector3.Distance(x.transform.position, unit.transform.position)).ToList();

					//set unit to hunt for entire team
					if (m_huntedUnit is null) {
						m_huntedUnit = sortedList[0];
					}

					//find closest optimal node to that unit
					Battlefield.Node moveNode = FindOptimalNodeWithinDistance(
						GraphUtils.GetClosestNode<Battlefield.Node>(Battlefield.Instance,
							m_huntedUnit != null ? m_huntedUnit.transform.position : sortedList[0].transform.position));

					if (unit.TargetNode != moveNode) {
						unit.TargetNode = moveNode;
					}
				}
				//Checks if there are enmies nearby and if unit is in a good position (had good cover)
				else if (unit.EnemiesInRange.Any() &&
				         m_data.GetEntry(unit.CurrentNode).Cover > 0.8f &&
				         m_covers.Contains(unit.CurrentNode)) {
					//makes unit stand still if that's the case
					unit.TargetNode = null;
				}
				//Checks if enemies are in range and find the optimal cover
				else if (unit.EnemiesInRange.Any() && m_data.GetEntry(unit.CurrentNode).Cover < 0.6f) {
					Battlefield.Node moveNode = FindNearbyCover(FindOptimalNodeWithinDistance(unit.CurrentNode, 12));
					if (unit.TargetNode != moveNode) {
						unit.TargetNode = moveNode;
					}
				}
				else {
					//general targetLogic that uses optionalnode within distance and find the optimal node on the board
					Battlefield.Node moveNode = FindOptimalNodeWithinDistance(FindOptimalNode());
					if (unit.TargetNode != moveNode) {
						unit.TargetNode = moveNode;
					}
				}
			}
		}
	}
}