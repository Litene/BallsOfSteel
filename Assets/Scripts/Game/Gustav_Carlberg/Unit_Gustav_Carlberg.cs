using AI;
using Game;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustav_Carlberg
{
    public class Unit_Gustav_Carlberg : Unit
    {
        #region Properties

        public new Team_Gustav_Carlberg Team => base.Team as Team_Gustav_Carlberg;

        #endregion

        protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            return enemiesInRange[Random.Range(0, enemiesInRange.Count)];
        }
    }
}