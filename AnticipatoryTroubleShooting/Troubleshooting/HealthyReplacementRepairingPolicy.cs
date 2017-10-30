using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.Troubleshooting
{
    class HealthyReplacementRepairingPolicy : ITroubleShooterRepairingPolicy
    {
        double _currTime;
        DFS_HybridRepairPolicy _repairPolicy;
        TroubleShooter _troubleshooter;
        int _lastHealthyReplaced = -1;
        public HashSet<int> _HealthComponentsReplaced;

        public HealthyReplacementRepairingPolicy(TroubleShooter troubleshooter, int nIntervalsLookAhead)
        {
            _troubleshooter = troubleshooter;
            _repairPolicy = new DFS_HybridRepairPolicy(troubleshooter, nIntervalsLookAhead);
            _HealthComponentsReplaced = new HashSet<int>();
        }

        //-----------------------------------------------------------------------------------------------------------
        public ReapirType RepairComponentPolicy(Model model, int compID, double timeLimit, double currTime, out string policyString, out double repairCost)
        {
            _currTime = currTime;
            double costAns;
            if (_HealthComponentsReplaced.Contains(compID))
                costAns = 0; // זה סתם לדיבאג, ל0 אין משמעות

            _HealthComponentsReplaced = new HashSet<int>();
            ReapirType repairType = _repairPolicy.RepairComponentPolicy(model, compID, timeLimit, currTime, out policyString, out costAns);
            _HealthComponentsReplaced.Remove(compID);
            repairCost = costAns;

            Dictionary<int, ReapirType> healthPolicy = RepairHealthComponents(model, compID, timeLimit);
            double healthCost = getHealthyCosts(healthPolicy);
            repairCost += healthCost;
            policyString = null;

            return repairType;
        }
        //-----------------------------------------------------------------------------------------------------------
        //public HashSet<int> GetHelathCompsPolicy(Model model, int faultComp, double timeLimit, double currTime)
        //{
        //    return _HealthComponentsReplaced;
        //}
        //-----------------------------------------------------------------------------------------------------------
        private double getHealthyCosts(Dictionary<int, ReapirType> healthPolicy)
        {
            double totalCost = 0;
            foreach (var healthComp in healthPolicy)
            {
                double currCost = _troubleshooter._model._components[healthComp.Key].getRepairCost(healthComp.Value);
                totalCost += currCost;
            }
            return totalCost;
        }
        //-----------------------------------------------------------------------------------------------------------

        private Dictionary<int, ReapirType> RepairHealthComponents(Model model, int faultComp, double timeLimit)
        {
            Dictionary<int, ReapirType> dicAns = new Dictionary<int, ReapirType>();
            foreach (var comp in model._testComponents)
            {
                if (faultComp == comp)
                    continue;
                if (ShouldReplaceComp(model, comp, timeLimit))
                {
                    dicAns.Add(comp, ReapirType.REPLACE);
                    _lastHealthyReplaced = comp;
                    _HealthComponentsReplaced.Add(comp);
                    //Console.WriteLine(1);

                }
                else
                {
                    dicAns.Add(comp, ReapirType.NoAction);
                }

            }
            return dicAns;
        }
        //-----------------------------------------------------------------------------------------------------------
        private bool ShouldReplaceComp(Model model, int healthComp, double timeLimit)
        {
            _repairPolicy = new DFS_HybridRepairPolicy(_troubleshooter, _repairPolicy._nIntervals, healthComp);
            _repairPolicy._Tlimit = timeLimit;
            List<Interval> compIntervals = _repairPolicy.createIntervals(_currTime, timeLimit, healthComp);
            double nextTime = compIntervals.First().Ur;
            List<Interval> intervalsCopy = new List<Interval>(compIntervals);
            State startState = new State(_currTime, 0, true, null, _troubleshooter);

            double repairCost = _troubleshooter.repairComponent(healthComp, ReapirType.REPLACE); //update only the specific repair comp
            _troubleshooter._model.updateCompsAges(nextTime - _currTime);
            compIntervals.RemoveAt(0);

            State ReplaceState = new State(nextTime, startState._currTime, false, startState, _troubleshooter);
            ReplaceState._repairType = ReapirType.REPLACE; //הכרחי לפונקציה בתוך DFS

            double ExpecReplaceCost = _repairPolicy.computeCost(ReplaceState, compIntervals);
            ExpecReplaceCost += model._components[healthComp]._replaceCost;



            _troubleshooter._model.updateComps(startState._comps);
            _troubleshooter._model.updateCompsAges(nextTime - _currTime);
            compIntervals = intervalsCopy;
            //startState = new State(_currTime, 0, false, null, _troubleshooter);

            State NoActionState = new State(nextTime, startState._currTime, false, startState, _troubleshooter);
            NoActionState._repairType = ReapirType.NoAction; //הכרחי לפונקציה בתוך DFS
            double ExpecNoActionCost = _repairPolicy.computeCost(NoActionState, compIntervals);

            _troubleshooter._model.updateComps(startState._comps);

            return (ExpecReplaceCost <= ExpecNoActionCost);
        }
        //-----------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "HealthReplaceLK_" + _repairPolicy._nIntervals;
        }
        //-----------------------------------------------------------------------------------------------------------


    }
}
