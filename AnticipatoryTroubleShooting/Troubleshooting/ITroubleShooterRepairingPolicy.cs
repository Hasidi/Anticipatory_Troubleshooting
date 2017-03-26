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
        ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString, out double repairCost);
    }
    //-----------------------------------------------------------------------------------------------------------
    //class HybridRepairPolicy : ITroubleShooterRepairingPolicy
    //{
    //    // does not do what should do - is like the decreasing one
    //    public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString, out double repairCost)
    //    {
    //        Component comp = model._components[compID];
    //        SurvivalBayesModel svModel = (SurvivalBayesModel)model;
    //        //double futureFixSurvive = svModel._survivalCurves[compID].survive(maxAge, currTime, comp._age);
    //        double futureFixSurvive = 1 - svModel._survivalCurves[compID].FaultBetween(maxAge, currTime, currTime);

    //        double futureFixCostEstimated = comp._repairCost + comp._replaceCost * (1 - futureFixSurvive);

    //        //double futureReplaceSurvive = svModel._survivalCurves[compID].survive(maxAge, currTime, 0);
    //        double futureReplaceSurvive = 1 - svModel._survivalCurves[compID].FaultBetween(maxAge, currTime, currTime);

    //        double futureReplaceCostEstimated = comp._replaceCost + comp._replaceCost * (1 - futureReplaceSurvive);
    //        policyString = "comp.age = " + comp._age + ", currTime = " + currTime + Environment.NewLine + "future-Fix-survive = " + futureFixSurvive + Environment.NewLine + "future-Replace-survive = " + futureReplaceSurvive;
    //        ReapirType fixAns;
    //        if (futureReplaceCostEstimated <= futureFixCostEstimated)
    //        {
    //            fixAns = ReapirType.REPLACE;
    //            comp._age = 0;
    //            repairCost = futureReplaceCostEstimated;

    //        }
    //        else
    //        {
    //            fixAns = ReapirType.FIX;
    //            repairCost = futureFixCostEstimated;
    //        }
    //        return fixAns;
    //    }

    //    public override string ToString()
    //    {
    //        return "Hybrid-Fix";
    //    }

    //}
    ////-----------------------------------------------------------------------------------------------------------
    //class FixingRepairPolicy : ITroubleShooterRepairingPolicy
    //{
    //    public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString, out double repairCost)
    //    {
    //        SurvivalBayesModel svModel = (SurvivalBayesModel)model;

    //        Component comp = model._components[compID];
    //        policyString = string.Empty;
    //        repairCost = model._components[compID]._repairCost;
    //        return ReapirType.FIX;

    //    }

    //    public override string ToString()
    //    {
    //        return "Always-Fix";
    //    }

    //}
    //-----------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------
    class HybridRepairPolicyDecreasing : ITroubleShooterRepairingPolicy
    {
        public ReapirType RepairComponentPolicy(Model model, int compID, double Tlimit, double currTime, out string policyString, out double repairCost)
        {
            
            Component comp = model._components[compID];
            //if (Math.Abs(comp._age - currTime) > 0.001)
            //    throw new Exception();
            SurvivalBayesModel svModel = (SurvivalBayesModel)model;
            svModel._survivalCurves[compID].setParameter(ExperimentRunner.getNewFixCurve(svModel._components[compID]._survivalFactor));
            double futureFixFault = svModel._survivalCurves[compID].FaultBetween(Tlimit, currTime, currTime);

            double futureFixCostEstimated = comp._repairCost + comp._replaceCost * futureFixFault;

            svModel._survivalCurves[compID].setParameter(ExperimentRunner.getNewCurve());
            double futureReplaceFault = svModel._survivalCurves[compID].FaultBetween(Tlimit, currTime, currTime);

            double futureReplaceCostEstimated = comp._replaceCost + comp._replaceCost * futureReplaceFault;

            //policyString = "comp.age = " + comp._age + ", currTime = " + currTime + Environment.NewLine + "future-Fix-survive = " + futureFixSurvive + Environment.NewLine + "future-Replace-survive = " + futureReplaceSurvive;
            policyString = "comp.age = " + comp._age + ", currTime = " + currTime + Environment.NewLine + "future-Fix-fault = " + futureFixFault + Environment.NewLine + "future-Replace-fault = " + futureReplaceFault;

            ReapirType fixAns;
            if (futureReplaceCostEstimated <= futureFixCostEstimated)
            {
                fixAns = ReapirType.REPLACE;
                repairCost = futureReplaceCostEstimated;

                //comp._age = 0;
                //svModel.updateSurvivalCurve(compID, ExperimentRunner.SURVIVAL_FACTOR_NEW);

            }
            else
            {
                fixAns = ReapirType.FIX;
                repairCost = futureFixCostEstimated;

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
        public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString, out double repairCost)
        {
            SurvivalBayesModel svModel = (SurvivalBayesModel)model;

            Component comp = model._components[compID];
            policyString = string.Empty;

            //comp._age = 0;
            //svModel.updateSurvivalCurve(compID, ExperimentRunner.SURVIVAL_FACTOR_NEW * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
            //svModel.updateSurvivalCurve(compID, comp._survivalFactor * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
            repairCost = model._components[compID]._repairCost;
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
        public ReapirType RepairComponentPolicy(Model model, int compID, double maxAge, double currTime, out string policyString, out double repairCost)
        {
            Component comp = model._components[compID];
            policyString = string.Empty;
            //comp._age = 0;
            repairCost = model._components[compID]._replaceCost;
 
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
    public enum ReapirType { FIX, REPLACE, NoAction };


}
