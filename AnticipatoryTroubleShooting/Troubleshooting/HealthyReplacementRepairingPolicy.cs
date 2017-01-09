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

        public HealthyReplacementRepairingPolicy(TroubleShooter troubleshooter, int nIntervalsLookAhead)
        {
            _troubleshooter = troubleshooter;
            _repairPolicy = new DFS_HybridRepairPolicy(troubleshooter, nIntervalsLookAhead);
        }

        //-----------------------------------------------------------------------------------------------------------
        public ReapirType RepairComponentPolicy(Model model, int compID, double timeLimit, double currTime, out string policyString, out double repairCost)
        {
            _currTime = currTime;
            double costAns;
            ReapirType repairType = _repairPolicy.RepairComponentPolicy(model, compID, timeLimit, currTime, out policyString, out costAns);
            repairCost = costAns;
            Dictionary<int, ReapirType> healthPolicy = RepairHealthComponents(model, compID, timeLimit);
            policyString = null;
            return repairType;
        }
        //-----------------------------------------------------------------------------------------------------------
        public Dictionary<int, ReapirType> GetHelathCompsPolicy(Model model, int faultComp, double timeLimit, double currTime)
        {
            _currTime = currTime;
            Dictionary<int, ReapirType> healthPolicy = RepairHealthComponents(model, faultComp, timeLimit);
            return healthPolicy;
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
            List<Interval> compIntervals = _repairPolicy.createIntervals(_currTime, timeLimit, healthComp);
            double nextTime = compIntervals.First().Ur;
            State startState = new State(_currTime, 0, true, null, _troubleshooter);
            List<Interval> intervalsCopy = new List<Interval>(compIntervals);

            double repairCost = _troubleshooter.repairComponent(healthComp, ReapirType.REPLACE); //update only the specific repair comp
            _troubleshooter._model.updateCompsAges(nextTime - _currTime);
            compIntervals.RemoveAt(0);
            State ReplaceState = new State(nextTime, 0, false, startState, _troubleshooter);
            double ExpecReplaceCost = _repairPolicy.computeCost(startState, compIntervals) + model._components[healthComp]._replaceCost;



            _troubleshooter._model.updateComps(startState._comps);
            _troubleshooter._model.updateCompsAges(nextTime - _currTime);
            compIntervals = intervalsCopy;
            State NoActionState = new State(nextTime, 0, false, startState, _troubleshooter);
            double ExpecNoActionCost = _repairPolicy.computeCost(startState, compIntervals);

            _troubleshooter._model.updateComps(startState._comps);

            return (ExpecReplaceCost <= ExpecNoActionCost);
        }
        //-----------------------------------------------------------------------------------------------------------

    }
}
