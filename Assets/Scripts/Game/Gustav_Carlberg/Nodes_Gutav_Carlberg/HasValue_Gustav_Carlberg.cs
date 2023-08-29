using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes {
	public class HasValue_Gustav_Carlberg : DecoratorNode {
		public string m_key = "VariableName";
		public bool HasValue;

		protected override State OnUpdate() {
			m_state = State.Failure;
			if (Tree != null && Tree.Blackboard != null) {
				object o = new object();
				var iValue = Tree.Blackboard.GetValue(m_key, o);
				if (iValue != o && HasValue || iValue == o && !HasValue) m_state = State.Running;
				else if (iValue == o && HasValue || iValue != o && !HasValue) m_state = State.Failure;
			}

			// update child?
			if (m_state == State.Running) {
				m_state = m_child.Update();
			}

			return m_state;
		}
	}
}