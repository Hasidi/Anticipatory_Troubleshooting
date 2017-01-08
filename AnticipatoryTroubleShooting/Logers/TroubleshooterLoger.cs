using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class TroubleshooterLoger
    {
        StringBuilder _SB;
        int _expNumbber;

        private string ORIGIN_PATH = @"../Debug/Files/Logers/";

        private string PATH = @"../Debug/Files/Logers/";

        private static TroubleshooterLoger Instance;
        private TroubleshooterLoger()
        {
            _SB = new StringBuilder();            

        }
        public static TroubleshooterLoger _instance
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new TroubleshooterLoger();

                }
                return Instance;
            }
        }

        //-----------------------------------------------------------------------------------------------------------
        public void setLogFileName(string fileName, string troubleshooterType)
        {
            PATH = ORIGIN_PATH + fileName + "_"+troubleshooterType +".txt";
            if (!File.Exists(PATH))
                File.Create(PATH).Close();
            //else
            //{
            //    File.Delete(PATH);
            //    File.Create(PATH).Close();
            //}
        }
        public void setLogFileName(string fileName)
        {
            PATH = ORIGIN_PATH + fileName + "_" +  ".txt";
            if (!File.Exists(PATH))
                File.Create(PATH).Close();
            else
            {
                File.Delete(PATH);
                File.Create(PATH).Close();
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public void setLogFileName(string fileName, double timeLimit)
        {
            PATH = ORIGIN_PATH + fileName + "_timeLimit_" + timeLimit + ".txt";
            if (!File.Exists(PATH))
                File.Create(PATH).Close();
            else
            {
                File.Delete(PATH);
                File.Create(PATH).Close();
            }
        }

        //-----------------------------------------------------------------------------------------------------------
        public void markPriorObservedValues(Dictionary<int, Component> components)
        {
            _SB.AppendLine("        Diagnostic Begin......");
            StringBuilder sbTests = new StringBuilder();
            sbTests.AppendLine("Test pre observations");
            StringBuilder sbCommand = new StringBuilder();
            sbCommand.AppendLine("Command pre observations");
            StringBuilder sbSensors = new StringBuilder();
            sbSensors.AppendLine("Sensors pre observations");
            foreach (var comp in components)
            {
                if (comp.Value._componentType == ComponentType.TEST)
                    sbTests.AppendLine("   V-" + comp.Key + " with pre Value: " + comp.Value._preTestObservation);
                else if (comp.Value._componentType == ComponentType.COMMAND)
                    sbCommand.AppendLine("   V-" + comp.Key + " with pre Value: " + comp.Value._preTestObservation);
                //else
                //    sbSensors.AppendLine("   V-" + comp.Key + " with pre Value: " + comp.Value._preTestObservation);
            }
            _SB.AppendLine(sbTests.ToString());
            _SB.AppendLine(sbCommand.ToString());
            _SB.AppendLine(sbSensors.ToString());
            _SB.AppendLine("---------------------------------------------------------------------------------------");
            writeCurrText();
        }
        //-----------------------------------------------------------------------------------------------------------
        public void markRevealedSensors(Dictionary<int, int> sensorsRevealedSet)
        {
            StringBuilder sbRevealed = new StringBuilder();
            foreach (var revealComp in sensorsRevealedSet)
            {
                sbRevealed.Append(revealComp.Key + ", ");
            }
            sbRevealed.AppendLine();
            //sbRevealed.AppendLine("---------------------------------------------------------------------------------------");
            _SB.Append(sbRevealed.ToString());
            writeCurrText();
        }
        //-----------------------------------------------------------------------------------------------------------
        public void printDiagnoserDecision(int iteration, int selectedVar, int observedValue, Dictionary<int, Component> components)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("----------------------------------iteration num." + iteration + "-----------------------------------------------------");
            foreach (var component in components)
            {
                double[] currProbs = component.Value._distribution;
                string s = "";
                for (int i = 0; i < currProbs.Length; i++)
                    s += currProbs[i].ToString("N5") + ",";
                s = s.Substring(0, s.Length - 1);
                sb.AppendLine("V-" + component.Key + " with distribution {" + s + "}, " + " , "
                    + component.Value._bsProb + " ," + component.Value._normailzePorb
                    + " , " + component.Value._svProb
                    + " ," + component.Value._faultProb
                    + " ," + component.Value._decisionRatio + ", " + component.Value._age);
            }
            sb.AppendLine("selected var: V-" + selectedVar + " with observed value: " + observedValue);
            //sb.AppendLine("------------------------------------------------------------------");
            _SB.Append(sb.ToString());
            writeCurrText();
        }
        //-----------------------------------------------------------------------------------------------------------
        public void markEndOfExperiment(double experimentTotalCost)
        {
            _SB.AppendLine("=================================================Experiment num." + _expNumbber + " TotalCost: " + experimentTotalCost + "========================================================");
            writeCurrText();
            _expNumbber++;
        }
        //-----------------------------------------------------------------------------------------------------------
        public void totalCostOfExperiments(double totalCost)//4 last one
        {
            _SB.AppendLine("---------------->>Total Cost in this Algorithm: " + totalCost);
            writeCurrText();
            _expNumbber = 0;
        }
        //-----------------------------------------------------------------------------------------------------------
        private void writeCurrText()
        {
            File.AppendAllText(PATH, _SB.ToString());
            _SB = new StringBuilder();
        }
        //-----------------------------------------------------------------------------------------------------------
        public void printFixPolicy(string policy, double price, string policyString)
        {
            _SB.AppendLine("");
            _SB.AppendLine("choosed to -" + policy + "- with price: " + price);
            _SB.AppendLine(policyString);
        }
        //-----------------------------------------------------------------------------------------------------------
        public void writeText(string toWrite)
        {
            _SB.AppendLine(toWrite);

            File.AppendAllText(PATH, _SB.ToString());
            _SB = new StringBuilder();
        }

    }
}
