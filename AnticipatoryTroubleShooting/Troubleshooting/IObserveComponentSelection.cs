using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    interface IObserveComponentSelection
    {
        int chooseComponent(Dictionary<int, Component> components);

    }
    //-----------------------------------------------------------------------------------------------------------
    class RandomSelector : IObserveComponentSelection
    {
        public int chooseComponent(Dictionary<int, Component> components)
        {
            List<int> componentsNumbers = components.Keys.ToList();
            int currChoosen = UsefulFunctions.randFromGivenSet(componentsNumbers);
            return currChoosen;
        }
        public override string ToString()
        {
            return "RandomAlgo";
        }
    }
    //-----------------------------------------------------------------------------------------------------------
    class CostProbSelector : IObserveComponentSelection
    {
        public int chooseComponent(Dictionary<int, Component> components)
        {
            double minStatistic = double.MaxValue;
            int minVarStatistic = -1;
            Dictionary<int, double> bla = new Dictionary<int, double>();
            foreach (var currVar in components)
            {
                double currStatistic = components[currVar.Key]._repairCost / components[currVar.Key]._faultProb;
                components[currVar.Key]._decisionRatio = currStatistic; // update it only for using it in the log file
                if (currStatistic < minStatistic)
                {
                    minStatistic = currStatistic;
                    minVarStatistic = currVar.Key;
                }
                bla.Add(currVar.Key, currStatistic);
            }
            return minVarStatistic;
        }
        public override string ToString()
        {
            return "CostProbAlgo";
        }
    }
    //-----------------------------------------------------------------------------------------------------------
    class ConstCostProbSelector : IObserveComponentSelection
    {
        public int chooseComponent(Dictionary<int, Component> components)
        {
            double minStatistic = double.MaxValue;
            int minVarStatistic = -1;
            Dictionary<int, double> bla = new Dictionary<int, double>();

            foreach (var currVar in components)
            {
                double currStatistic = 1 / components[currVar.Key]._faultProb;
                components[currVar.Key]._decisionRatio = currStatistic; // update it only for using it in the log file
                if (currStatistic < minStatistic)
                {
                    minStatistic = currStatistic;
                    minVarStatistic = currVar.Key;
                }
                bla.Add(currVar.Key, currStatistic);
            }
            return minVarStatistic;
        }
        public override string ToString()
        {
            return "ConstCostProbAlgo";
        }
    }
    //-----------------------------------------------------------------------------------------------------------

}
