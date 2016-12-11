using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.Models;

namespace AnticipatoryTroubleShooting
{
    interface ITroubleShooterRepairingPolicy
    {
        ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString);
    }
    //-----------------------------------------------------------------------------------------------------------
    class HybridRepairPolicy : ITroubleShooterRepairingPolicy
    {
        public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString)
        {
            Component comp = model._components[compID];
            SurvivalBayesModel svModel = (SurvivalBayesModel)model;
            double futureFixSurvive = svModel._survivalCurves[compID].survive(maxAge, currTime, comp._age);
            double futureFixCostEstimated = comp._repairCost + comp._replaceCost * (1 - futureFixSurvive);
            double futureReplaceSurvive = svModel._survivalCurves[compID].survive(maxAge, currTime, 0);
            double futureReplaceCostEstimated = comp._replaceCost + comp._replaceCost * (1 - futureReplaceSurvive);
            policyString = "comp.age = " + comp._age + ", currTime = " + currTime + Environment.NewLine + "future-Fix-survive = " + futureFixSurvive + Environment.NewLine + "future-Replace-survive = " + futureReplaceSurvive;
            ReapirType fixAns;
            if (futureReplaceCostEstimated <= futureFixCostEstimated)
            {
                fixAns = ReapirType.REPLACE;
                comp._age = 0;
            }
            else
            {
                fixAns = ReapirType.FIX;
            }
            return fixAns;
        }

        public override string ToString()
        {
            return "Hybrid-Fix";
        }

    }
    //-----------------------------------------------------------------------------------------------------------
    class FixingRepairPolicy : ITroubleShooterRepairingPolicy
    {
        public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString)
        {
            SurvivalBayesModel svModel = (SurvivalBayesModel)model;

            Component comp = model._components[compID];
            policyString = string.Empty;
            return ReapirType.FIX;

        }

        public override string ToString()
        {
            return "Always-Fix";
        }

    }
    //-----------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------
    class HybridRepairPolicyDecreasing : ITroubleShooterRepairingPolicy
    {
        public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString)
        {
            Component comp = model._components[compID];
            SurvivalBayesModel svModel = (SurvivalBayesModel)model;
            double futureFixSurvive = svModel._survivalCurves[compID].survive(maxAge, currTime, comp._age);
            double futureFixCostEstimated = comp._repairCost + comp._replaceCost * (1 - futureFixSurvive);
            double futureReplaceSurvive = svModel._survivalCurves[compID].survive(maxAge, currTime, 0);
            double futureReplaceCostEstimated = comp._replaceCost + comp._replaceCost * (1 - futureReplaceSurvive);
            policyString = "comp.age = " + comp._age + ", currTime = " + currTime + Environment.NewLine + "future-Fix-survive = " + futureFixSurvive + Environment.NewLine + "future-Replace-survive = " + futureReplaceSurvive;
            ReapirType fixAns;
            if (futureReplaceCostEstimated <= futureFixCostEstimated)
            {
                fixAns = ReapirType.REPLACE;
                //comp._age = 0;
                //svModel.updateSurvivalCurve(compID, ExperimentRunner.SURVIVAL_FACTOR_NEW);

            }
            else
            {
                fixAns = ReapirType.FIX;
                //comp._age = 0;
                //svModel.updateSurvivalCurve(compID, ExperimentRunner.SURVIVAL_FACTOR_NEW * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
                //svModel.updateSurvivalCurve(compID, comp._survivalFactor * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
            }
            return fixAns;
        }

        public override string ToString()
        {
            return "Hybrid-Fix";
        }

    }
    //-----------------------------------------------------------------------------------------------------------
    class FixingRepairPolicyDecreasing : ITroubleShooterRepairingPolicy
    {
        public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString)
        {
            SurvivalBayesModel svModel = (SurvivalBayesModel)model;

            Component comp = model._components[compID];
            policyString = string.Empty;

            //comp._age = 0;
            //svModel.updateSurvivalCurve(compID, ExperimentRunner.SURVIVAL_FACTOR_NEW * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
            //svModel.updateSurvivalCurve(compID, comp._survivalFactor * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
            return ReapirType.FIX;

        }

        public override string ToString()
        {
            return "Always-Fix";
        }

    }





    //-----------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------
    class ReplacingRepairPolicy : ITroubleShooterRepairingPolicy
    {
        public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString)
        {
            Component comp = model._components[compID];
            policyString = string.Empty;
            //comp._age = 0;
            return ReapirType.REPLACE;

        }

        public override string ToString()
        {
            return "Always-Replace";
        }
    }


    //-----------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------

//DFS-HybridPolicy

   













    //-----------------------------------------------------------------------------------------------------------
    public enum ReapirType { FIX, REPLACE };


}
