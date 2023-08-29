using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes {
	public class CompareBool_Gustav_Carlberg : DecoratorNode {

		public string m_key = "VariableName";
		public bool m_iValue;

		static string[] sm_opCodes = new string[] { " == ", " != ", " > ", " < " };

		protected override State OnUpdate() {
			m_state = State.Failure;
			if (Tree != null && Tree.Blackboard != null) {
				bool iValue = Tree.Blackboard.GetValue<bool>(m_key, new bool());

				if (iValue == m_iValue) m_state = State.Running;
				else m_state = State.Failure;
			}

			// update child?
			if (m_state == State.Running) {
				m_state = m_child.Update();
			}

			return m_state;
		}
	}
}