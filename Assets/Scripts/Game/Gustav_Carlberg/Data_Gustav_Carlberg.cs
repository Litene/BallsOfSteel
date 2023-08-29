using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Gustav_Carlberg;
using UnityEngine;

namespace Game.Gustav_Carlberg {
	
	// data class for storing data and performing actions on said data
	public class Data_Gustav_Carlberg {
		//main data structure
		public static Dictionary<Battlefield.Node, DataWrapper> Dictionary = new();

		//directions 
		private readonly Vector2Int[] s_coverDirections = new Vector2Int[]
			{ new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

		// method for updating and adding to the main data structure
		public void AddEntry(Battlefield.Node node, DataWrapper data) {
			if (Dictionary.TryGetValue(node, out DataWrapper value)) {
				value.EnemyFirePower = data.EnemyFirePower ?? value.EnemyFirePower;
				value.Influence = data.Influence ?? value.Influence;
				value.Cover = data.Cover ?? value.Cover;
				return;
			}
			Dictionary.Add(node, data);
		}

		// method for getting a entry, returns null if its invalid
		public DataWrapper GetEntry(Battlefield.Node node) {
			DataWrapper val;
			if (Dictionary.TryGetValue(node, out val)) {
				return val;
			}

			return null;
		} 

		//calculating influence and adds it to the datastructure, heavily influenced by Carl Granbergs model
		public Data_Gustav_Carlberg CalculateInfluence(List<Game.Unit> friendlyTeam, List<Game.Unit> enemyTeam,
			IEnumerable<Graphs.INode> nodes) {
			if (friendlyTeam.Any() && enemyTeam.Any()) {
				List<Game.Unit>[] teams = new[] { friendlyTeam, enemyTeam };
				foreach (var node in nodes) {
					if (node is Battlefield.Node currNode) {
						float currentScore = 0.0f;
						for (int i = 0; i < teams.Length; i++) {
							foreach (var unit in teams[i]) {
								if (unit == null || currNode == null) {
									continue;
								}

								float dist = Vector3.Distance(unit.transform.position, currNode.WorldPosition);
								if (dist < Unit.FIRE_RANGE) {
									currentScore += (1.0f - (dist / Unit.FIRE_RANGE)) * (i == 0 ? 1.0f : -1.0f);
								}
							}
						}

						AddEntry(currNode,
							new DataWrapper(null, currentScore / (friendlyTeam.Count() + enemyTeam.Count()), null));
					}
				}
			}

			return this;
		}

		//calculating Firepower and adds it to the datastructure, heavily influenced by Carl Granbergs model
		public Data_Gustav_Carlberg CalculateFirePower(List<Game.Unit> enemyTeam, IEnumerable<Graphs.INode> nodes) {
			if (enemyTeam.Any()) {
				foreach (var node in nodes) {
					if (node is Battlefield.Node currNode) {
						float currentScore = 0;
						foreach (var unit in enemyTeam) {
							float dist = Vector3.Distance(unit.transform.position, currNode.WorldPosition);
							if (dist < Unit.FIRE_RANGE) {
								currentScore -= 1.0f;
							}
						}

						AddEntry(currNode, new DataWrapper(currentScore / enemyTeam.Count, null, null));
					}
				}
			}

			return this;
		}

		//calculating Cover and adds it to the datastructure, heavily influenced by Carl Granbergs model
		public List<Battlefield.Node> CalculateCover(List<Game.Unit> enemyTeam,
			IEnumerable<Graphs.INode> nodes) {
			List<Battlefield.Node> CoverNodes = new List<Battlefield.Node>();
			foreach (var node in nodes) {
				if (node is Battlefield.Node currNode) {
					float currentScore = 0;
					if (enemyTeam.Any()) {
						foreach (var unit in enemyTeam) {
							Vector3 dirToUnit = Vector3.Normalize(unit.transform.position - currNode.WorldPosition);
							foreach (var dir in s_coverDirections) {
								if (Battlefield.Instance[currNode.Position + dir] is Node_Rock rock) {
									Vector3 dirToCover = Vector3.Normalize(rock.WorldPosition - currNode.WorldPosition);
									currentScore += Vector3.Dot(dirToUnit, dirToCover);
								}
							}
						}
					}

					if (currentScore > 0) {
						CoverNodes.Add(Battlefield.Instance[currNode.Position]);
					}

					float correctedScore = currentScore / enemyTeam.Count;
					AddEntry(currNode, new DataWrapper(null, null, correctedScore > 0 ? correctedScore : 0));
				}
			}

			return CoverNodes;
		}

		//returns the current node value
		public bool GetNodeValue(Battlefield.Node node, out DataWrapper outVar) {
			if (Dictionary.ContainsKey(node)) {
				float value = float.MinValue;
				if (Dictionary[node].EnemyFirePower is { } currFirePower &&
				    Dictionary[node].Influence is { } currInfluence &&
				    Dictionary[node].Cover is { } currCover) {
					outVar = Dictionary[node];
					return true;
				}
			}

			outVar = null;
			return false;
		}
	}
	//data wrapper
	public class DataWrapper {
		public float? EnemyFirePower;
		public float? Influence;
		public float? Cover;

		public DataWrapper(float? enemyFirePower = null, float? influence = null, float? cover = null) {
			EnemyFirePower = enemyFirePower;
			Influence = influence;
			Cover = cover;
		}
	}
}