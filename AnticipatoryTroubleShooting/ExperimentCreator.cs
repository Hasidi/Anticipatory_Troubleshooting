using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.Logers;
using AnticipatoryTroubleShooting.Models;
using System.IO;
using AnticipatoryTroubleShooting.SurvivalFunctions;

namespace AnticipatoryTroubleShooting
{
    class ExperimentCreator
    {
        public static string FILES_PATH = "../Debug/Files";
        public static string EXPERIMENT_FILE_PATH = "../Debug/Files/Experiments";
        Survival_IN_BayesModel _surBayesModel;
        public StringBuilder SB;
        protected static int CURR_FILE_NUMBER = 0;
        protected CreatorLoger CREATOR_LOGER;
        public static double MAX_AGE_DIFF = 8;
        public static double MIN_AGE = 0.1;
        protected string _directoryPath;
        static Random RANDOM = new Random();
        public ExperimentCreator() { }

        public ExperimentCreator(Survival_IN_BayesModel Csvm)
        {
            _surBayesModel = Csvm;
            string[] files = Directory.GetFiles(EXPERIMENT_FILE_PATH);
            CURR_FILE_NUMBER = files.Length;
            _directoryPath = string.Empty;

            CREATOR_LOGER = new CreatorLoger();
        }


        //-----------------------------------------------------------------------------------------------------------
        public void createExperiments(int nIterations, double maxAgeDiff, double minAge)
        {
            MAX_AGE_DIFF = maxAgeDiff;
            _directoryPath = EXPERIMENT_FILE_PATH + @"/MaxAgeDiff_" + MAX_AGE_DIFF.ToString();
            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
            CURR_FILE_NUMBER = Directory.GetFiles(_directoryPath).Length;
            CREATOR_LOGER.setPath(MAX_AGE_DIFF.ToString(), CURR_FILE_NUMBER);
            for (int i = 0; i < nIterations; i++)
            {
                string[] costs = getComponentsValues(ExperimentCreator.FILES_PATH + @"/costsFile.txt");
                string[] survivals = getComponentsValues(ExperimentCreator.FILES_PATH + @"/survivalFile.txt");
                createSingleExperiment(costs, survivals, maxAgeDiff, minAge);
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public virtual void createSingleExperiment(string[] costs, string[] survivals, double maxAGeDiff, double minAge)
        {

            insertTestComponentValues(costs, survivals, maxAGeDiff, minAge);
            Dictionary<int, int> testComponentObservations = new Dictionary<int, int>();
            Dictionary<int, int> controlObservations = new Dictionary<int, int>();
            Dictionary<int, int> sensorComponentsObservations = new Dictionary<int, int>();

            int faultComp = prepareExperiment(testComponentObservations, controlObservations, sensorComponentsObservations);
            writeExperiment(controlObservations, testComponentObservations, sensorComponentsObservations, faultComp);
            _surBayesModel.clearObservedValues(); //clear observations for the next experiment

        }

        //-----------------------------------------------------------------------------------------------------------
        protected virtual void insertTestComponentValues(string[] costs, string[] survivals, double maxAGeDiff, double minAge)
        {
            for (int i = 0; i < _surBayesModel._testComponents.Count; i++)
            {
                Component component = _surBayesModel._components[_surBayesModel._testComponents[i]];
                component._repairCost = int.Parse(costs[i]);
                component._age = minAge + UsefulFunctions.randFromSpecificRange(0, maxAGeDiff);

                component._survivalFactor = double.Parse(survivals[i]);
                _surBayesModel._survivalCurves[component._number] = new ExponentialDecayCurve(1, component._survivalFactor);
            }

            _surBayesModel.initPriorProbs();

        }
        //-----------------------------------------------------------------------------------------------------------
        protected int prepareExperiment(Dictionary<int, int> testComps, Dictionary<int, int> controlObservations, Dictionary<int, int> sensorComponents)
        {
            try
            {
                //CreatorLoger.markBegin();
                //part I : set control nodes
                sampleCommands(controlObservations);

                //part II : choose fault component
                int faultComp = chooseFaultComponent();
                int cValue = sampleTestComponentValues(testComps, faultComp);
                //set observations to all test comps
                foreach (var compN in _surBayesModel._testComponents)
                {
                    if (compN != faultComp)
                        _surBayesModel.updateObservedValue(compN, 0);  //means healthy
                    CREATOR_LOGER.addTestCompDist(_surBayesModel._components[compN]);
                }
                _surBayesModel.updateObservedValue(faultComp, cValue + 1); // Ask Roni if to set this value in this state
                CREATOR_LOGER.addChosenComp(_surBayesModel._components[faultComp]);

                //part III : set observed values to the rest comps
                sampleSensors(sensorComponents);
                int check = controlObservations.Count + testComps.Count + sensorComponents.Count;
                if (check != _surBayesModel._nComponents)
                    throw new Exception();
                //CreatorLoger.writeSensorsComps(sensorsForLog);
                CREATOR_LOGER.writeToLogFile();
                return faultComp;

            }
            catch (Exception ex)
            {
                Console.WriteLine();
            }
            return 0;
        }
        //-----------------------------------------------------------------------------------------------------------
        protected virtual int chooseFaultComponent() //not consider cost
        {
            Dictionary<int, double> testComponentsProbs = new Dictionary<int, double>();
            foreach (var testCompNum in _surBayesModel._testComponents)
            {
                Component testComponent = _surBayesModel._components[testCompNum];
                _surBayesModel.getComponentDistribution(testCompNum);
                double faultProb = _surBayesModel.getFaultProb(testCompNum);
                testComponentsProbs.Add(testCompNum, faultProb);
            }
            int selectedComp = UsefulFunctions.samplingFromDic(testComponentsProbs);
            return selectedComp;
        }
        //-----------------------------------------------------------------------------------------------------------
        private int sampleTestComponentValues(Dictionary<int, int> testComps, int faultComp)
        {
            double[] dis = _surBayesModel._components[faultComp]._distribution;
            double[] cdis = UsefulFunctions.excludeFirstFromDistribution(dis);
            int cvalue = UsefulFunctions.createSample(cdis);
            if ((cvalue + 1) >= dis.Length)
                Console.WriteLine("Exception in creator");
            foreach (var compNum in _surBayesModel._testComponents)
            {
                if (compNum == faultComp)
                    testComps.Add(compNum, cvalue + 1);
                else
                    testComps.Add(compNum, 0);
            }
            return cvalue;
        }
        //-----------------------------------------------------------------------------------------------------------
        private void sampleSensors(Dictionary<int, int> sensorComponents)
        {
            Dictionary<int, Component> sensorsForLog = new Dictionary<int, Component>();
            foreach (var compNum in _surBayesModel._sensorComponents)
            {
                Component component = _surBayesModel._components[compNum];
                double[] distribution = _surBayesModel.getComponentDistribution(component._number);
                component._distribution = distribution;
                if (distribution == null)
                    Console.WriteLine("null dis");
                int value = UsefulFunctions.createSample(distribution);
                sensorComponents.Add(component._number, value);
                component._preTestObservation = value;
                _surBayesModel.updateObservedValue(component._number, value);
                sensorsForLog.Add(component._number, component);
                CREATOR_LOGER.addSensorCompDist(_surBayesModel._components[compNum]);
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        private void sampleCommands(Dictionary<int, int> controlObservations)
        {
            foreach (var compNum in _surBayesModel._controlComponents)
            {
                if (compNum == _surBayesModel._components.Count - 1)
                    continue;
                Component comp = _surBayesModel._components[compNum];
                double[] distribution = _surBayesModel.getComponentDistribution(comp._number);
                int value = UsefulFunctions.createSample(distribution);
                //int value = 1;
                controlObservations.Add(comp._number, value);
            }
            controlObservations.Add(_surBayesModel._components.Count - 1, 1);
            //controlObservations[_surBayesModel._components.Count - 1] = 1;  //setting to "fault node indicator" to be fault
            foreach (var comp in controlObservations)
            {
                _surBayesModel.updateObservedValue(comp.Key, comp.Value);
                CREATOR_LOGER.addControlCompObservation(comp.Key, comp.Value);
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        protected string writeExperiment(Dictionary<int, int> controlObservations,
       Dictionary<int, int> testComponents,
       Dictionary<int, int> otherComponentsObservations, int faultComp)
        {
            string fileNumber = "";
            if (CURR_FILE_NUMBER < 10)
                fileNumber = @"/experimentFile0" + CURR_FILE_NUMBER + ".txt";
            else
                fileNumber = @"/experimentFile" + CURR_FILE_NUMBER + ".txt";
            string filePath = _directoryPath + fileNumber;
            CURR_FILE_NUMBER++;
            File.Create(filePath).Close();

            //StreamWriter sw = new StreamWriter(filePath);

            StringBuilder sb = new StringBuilder();
            StringBuilder toWrite = new StringBuilder();

            foreach (var comp in testComponents)
            {
                sb.Append(_surBayesModel._components[comp.Key]._repairCost + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());
            sb = new StringBuilder();

            foreach (var comp in testComponents)
            {
                sb.Append(_surBayesModel._components[comp.Key]._age + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());
            sb = new StringBuilder();

            foreach (var comp in testComponents)
            {
                sb.Append(_surBayesModel._components[comp.Key]._survivalFactor + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());

            sb = new StringBuilder();

            foreach (var comp in testComponents)
            {
                sb.Append(comp.Key + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());
            sb = new StringBuilder();

            foreach (var comp in testComponents)
            {
                sb.Append(comp.Value + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());
            sb = new StringBuilder();

            foreach (var comp in controlObservations)
            {
                sb.Append(comp.Key + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());
            sb = new StringBuilder();
            foreach (var comp in controlObservations)
            {
                sb.Append(comp.Value + " ");
            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());
            sb = new StringBuilder();


            foreach (var comp in otherComponentsObservations)
            {
                sb.Append(comp.Key + " ");

            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());
            sb = new StringBuilder();
            foreach (var comp in otherComponentsObservations)
            {
                sb.Append(comp.Value + " ");

            }
            sb.Remove(sb.Length - 1, 1);
            toWrite.AppendLine(sb.ToString());

            File.AppendAllText(filePath, toWrite.ToString());

            return filePath;
        }
        //----------------------------------------------------------------------------------------------------------------------
        private string[] getComponentsValues(string filePath)
        {
            List<string> experiments = new List<string>();
            StreamReader sr = new StreamReader(filePath);
            int i = 0;
            while (sr.Peek() > 0)
            {
                experiments.Add(sr.ReadLine());
                i++;
            }
            sr.Close();

            int rand = RANDOM.Next(0, experiments.Count);
            //int rand = 0;
            return experiments.ElementAt(rand).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        }
        //----------------------------------------------------------------------------------------------------------------------

    }
}
