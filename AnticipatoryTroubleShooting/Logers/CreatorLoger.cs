using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.Models;
using System.IO;

namespace AnticipatoryTroubleShooting.Logers
{
    class CreatorLoger
    {
         StringBuilder _SB;
        int _expNumbber;
        //public string PATH { get; set; }
        private static string ROOT_PATH = @"../Debug/Files/CreatorLoger/creatorLogFile_AgeDiff_";
        public string PATH = "";
        #region singleton Implementation

        private static CreatorLoger Instance;
        public CreatorLoger()
        {
            _SB = new StringBuilder();
            _SB.AppendLine("----------------------------------Creating Experiemt is now begining----------------------------------");
        }

        public void setPath(string path, int fileNumberInit)
        {
            PATH = ROOT_PATH + path + ".txt";
            _expNumbber = fileNumberInit;
            //if (File.Exists(PATH))
            //    File.Delete(PATH);
        }
        #endregion



        public void addControlCompObservation(int compNum, int observation)
        {
            
            
            
            _SB.AppendLine("control comp num." + compNum + "  with observed Val of " + observation);
            
        }
        
        public void addTestCompDist(Component comp)
        {
            _SB.AppendLine( "V-" + comp._number.ToString() + " with dist {" + getDistString(comp._distribution) + "}"
                + ", " + comp._bsProb.ToString() + ", " + comp._svProb.ToString() + ", " + comp._faultProb.ToString() );
        }

        public void addChosenComp(Component comp)
        {
            _SB.AppendLine("selected var:" + " V-" + comp._number.ToString() + " with observed value: " + comp._observedValue);
        }


        public void addSensorCompDist(Component comp)
        {
            _SB.AppendLine("V-" + comp._number.ToString() + " with dist {" + getDistString(comp._distribution) + "}"
                + ", " + comp._observedValue.ToString());
        }
        private string getDistString(double[] probs)
        {
            string s = "";
            for (int i = 0; i < probs.Length; i++)
                s += probs[i].ToString("N4") + ",";
            s = s.Substring(0, s.Length - 1);
            return s;
        }

        public void writeToLogFile()
        {
            if (_expNumbber <10)
                _SB.AppendLine("*************------------------------------------------------Experiment num.0" + _expNumbber.ToString() + " creation was ended------------------------------------------***********************");
            else
                _SB.AppendLine("*************------------------------------------------------Experiment num." + _expNumbber.ToString() + " creation was ended------------------------------------------***********************");

            _expNumbber++;
            File.AppendAllText(PATH, _SB.ToString());
            _SB = new StringBuilder();
        }

    }
}
