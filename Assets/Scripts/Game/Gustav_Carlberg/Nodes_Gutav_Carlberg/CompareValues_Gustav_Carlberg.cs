using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes {
	public class CompareValues_Gustav_Carlberg : DecoratorNode {
		public enum Operator {
			Equals,
			NotEquals,
			GreaterThan,
			LessThan,
		};

		public string m_key1 = "VariableName1";
		public Operator m_operator = Operator.LessThan;
		public string m_key2 = "VariableName2";

		static string[] sm_opCodes = new string[] { " == ", " != ", " > ", " < " };

		#region Properties

		//public override string Description => m_key1 + sm_opCodes[(int)m_operator] + m_iValue.ToString();

		#endregion

		protected override State OnUpdate() {
			m_state = State.Failure;
			if (Tree != null && Tree.Blackboard != null) {
				int iValue1 = Tree.Blackboard.GetValue<int>(m_key1, 10);
				int iValue2 = Tree.Blackboard.GetValue<int>(m_key2, new int());

				switch (m_operator) {
					case Operator.Equals:
						m_state = iValue1 == iValue2 ? State.Running : State.Failure;
						break;

					case Operator.NotEquals:
						m_state = iValue1 != iValue2 ? State.Running : State.Failure;
						break;

					case Operator.GreaterThan:
						m_state = iValue1 > iValue2 ? State.Running : State.Failure;
						break;

					case Operator.LessThan:
						m_state = iValue1 < iValue2 ? State.Running : State.Failure;
						break;
				}
			}

			// update child?
			if (m_state == State.Running) {
				m_state = m_child.Update();
			}

			return m_state;
		}
	}
}