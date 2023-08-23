using Game;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustav_Carlberg {
	public class Team_Gustav_Carlberg : Team {
		[SerializeField] private Color m_myFancyColor;

		#region Properties

		public override Color Color => m_myFancyColor;

		#endregion

	}
}