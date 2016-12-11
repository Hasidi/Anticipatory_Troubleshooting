using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.Logers
{
    class CreatorOverTimeLoger
    {
        StringBuilder _SB;
        private string PATH = @"../Debug/Files/Logers/";

        private string ORIGINAL_PATH = @"../Debug/Files/Logers/";

        //Model _model;


        private static CreatorOverTimeLoger Instance;
        private CreatorOverTimeLoger()
        {
            _SB = new StringBuilder();            

        }
        public static CreatorOverTimeLoger _instance
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new CreatorOverTimeLoger();

                }
                return Instance;
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public void setLogFileName(string fileName)
        {
            PATH = ORIGINAL_PATH + fileName + ".txt";
            if (!File.Exists(PATH))
                File.Create(PATH).Close();
            else
            {
                File.Delete(PATH);
                File.Create(PATH).Close();
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public void markIntervalsProbs(Dictionary<int, KeyValuePair<Interval, List<Interval>>> intervalsProbs, Model model)
        {
            _SB.AppendLine("-----------------------------------------------------------------------------------");

            foreach (var testComp in model._testComponents)
            {
                KeyValuePair<Interval, List<Interval>> currInterval = intervalsProbs[testComp];
                Component comp = model._components[testComp];
                markCompInterval(comp._number, currInterval, comp);
            }
            _SB.AppendLine("===============================================================================================");
            writeCurrText();

        }

        public void markIntervalsProbs2(Dictionary<int, List<Interval>> intervalsProbs, Model model)
        {
            _SB.AppendLine("-----------------------------------------------------------------------------------");

            foreach (var testComp in model._testComponents)
            {
                List<Interval> currInterval = intervalsProbs[testComp];
                Component comp = model._components[testComp];
                markCompInterval2(comp._number, intervalsProbs[testComp], comp);
            }
            _SB.AppendLine("===============================================================================================");
            writeCurrText();

        }
        //-----------------------------------------------------------------------------------------------------------
        private void markCompInterval(int compId, KeyValuePair<Interval, List<Interval>> intervalsProbs, Component component)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append ( "Comp n." + compId + ", age: " + component._age.ToString("N3") + " ; intervals: ");
            Interval choosenInterval = intervalsProbs.Key;

            foreach (var interval in intervalsProbs.Value)
            {
                sb.Append( interval.ToString() + ", ");
            }
            sb.Append( "| " + "[" + choosenInterval.Lr + "," + choosenInterval.Ur + "]");
            _SB.AppendLine(sb.ToString());
            writeCurrText();

        }

        private void markCompInterval2(int compId, List<Interval> intervalsProbs, Component component)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Comp n." + compId + ", age: " + component._age.ToString("N3") + " ; intervals: ");
            double total = 0;
            foreach (var interval in intervalsProbs)
            {
                sb.Append(interval.ToString() + ", ");
                total += interval.faultProb;
            }
            sb.Append("|| " + total);
            _SB.AppendLine(sb.ToString());

            writeCurrText();

        }
        //-----------------------------------------------------------------------------------------------------------
        public void markCurrExamineInterval(Interval interval)
        {
            _SB.AppendLine("CurrInterval:  " + "[" + interval.Lr + "," + interval.Ur + "]");
            _SB.AppendLine("-----------------------------------------------------------------------------------");
            writeCurrText();

        }
        //-----------------------------------------------------------------------------------------------------------
        public void markFaultInInterval(double faultTime, Model model)
        {
            _SB.AppendLine("currTime: " + faultTime.ToString("#.0000"));
            foreach (var compID in model._testComponents)
            {
                Component comp = model._components[compID];
                _SB.AppendLine("Comp n." + comp._number + ", age: " + comp._age.ToString("#.0000") + ", " + "obVal = " + comp._observedValue);
            }
            _SB.AppendLine("");
            writeCurrText();

        }
        //-----------------------------------------------------------------------------------------------------------

        private void writeCurrText()
        {
            File.AppendAllText(PATH, _SB.ToString());
            _SB = new StringBuilder();
        }


        //-----------------------------------------------------------------------------------------------------------
        public void writeText(string toWrite)
        {
            File.AppendAllText(PATH, toWrite);
        }
    }
}
