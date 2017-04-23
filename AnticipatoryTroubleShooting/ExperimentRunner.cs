using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.Logers;
using AnticipatoryTroubleShooting.Models;

namespace AnticipatoryTroubleShooting
{
    class ExperimentRunner
    {
        #region params
        public TroubleShooter _troubleshooter;

        public static string EXPERIMENTS_FILE_PATH = "../Debug/Files/Experiments";
        string[] _files;
        int _nFiles;
        public static string CSV_FILE_Defualt = "../Debug/Files/csvFile.csv";
        public static string CSV_FILE_overTime = "../Debug/Files/csvFileOverTime.csv";
        
        public static string CSV_FILE_TimeDis = "../Debug/Files/csvFileTimeDis.csv";
        public static string CSV_FILE_TimeDis_norm = "../Debug/Files/csvFileTimeDisNorm.csv";
        public static double EPSILON = 0.00000000001;
        public List<Experiment> _experiments;
        Dictionary<double, Dictionary<int, Component>> _experimentsAgeDiff;
        IWorldAfterRepair _worldAfterRepair;
        string _folderName;
        string _maxAgeDiff;
        int _nFaultsDuringTime = 0;
        public static int N_INTERVALS = 50;
        
        public static double REPLACE_COST = 10;
        private static double SURVIVAL_FACTOR_NEW = 0.04;
        public static double OVERHEADCOST = 15;
        public static double FIX_RATIO = 0;
        public static double SURVIVAL_FACTOR_REDUCE;
        public static double OVERHEAD_RATIO = 0.5;

        #endregion

        public ExperimentRunner(TroubleShooter troubleshooter)
        {
            _troubleshooter = troubleshooter;
            _experiments = new List<Experiment>();
            //readFilesOverTime();

            if (File.Exists(CSV_FILE_TimeDis) && File.Exists(CSV_FILE_TimeDis_norm))
            {
                File.Delete(CSV_FILE_TimeDis);
                File.Delete(CSV_FILE_TimeDis_norm);

            }
            else
            {
                //File.Create(CSV_FILE_TimeDis).Close();
                //File.Create(CSV_FILE_TimeDis_norm).Close();

            }
        }

        //----------------------------------------------------------------------------------------------------------------------
        //for every experiment run differnet sets of revealedNodes
        //public void runExperiments(List<List<int>> nodesToReveal)
        //{
        //    double totalCost = 0;
        //    int cExp = 0;
        //    TroubleshooterLoger._instance.setLogFileName(_folderName);
        //    foreach (var experiment in _experiments)
        //    {
        //        foreach (var revealedNodes in nodesToReveal)
        //        {
        //            Dictionary<int, Component> components = experiment.Value;
        //            TroubleshooterLoger._instance.markPriorObservedValues(components);
        //            Dictionary<int, int> sensorsRevealedSet = getSensorsPreTestValues(revealedNodes, components);
        //            _troubleshooter._model.initModel(components);
        //            double currFixCost = _troubleshooter.fixSystem(sensorsRevealedSet);
        //            totalCost += currFixCost;
        //            AddCSVLine("smallNetwork", _troubleshooter.ToString(), revealedNodes.Count, currFixCost);
        //            cExp++;
        //        }
        //    }
        //    TroubleshooterLoger._instance.totalCostOfExperiments(totalCost);
        //}
        //----------------------------------------------------------------------------------------------------------------------
        public void runExperimentsOverAgesDiff(Dictionary<int, List<List<int>>> nodesToReveal)
        {
            double totalCost = 0;
            int cExp = 0;
            int k = 0;
            foreach (var experiment in _experiments)
            {
                TroubleshooterLoger._instance.setLogFileName("ageDiff_"+experiment._ageDiff, _troubleshooter.ToString());
                foreach (var currRevealed in nodesToReveal[k]) 
                {
                    totalCost += runSingleExperiment(experiment._components, currRevealed, experiment._ageDiff);
                    cExp++;
                }
                k++;
            }
            TroubleshooterLoger._instance.totalCostOfExperiments(totalCost);
        }
        //----------------------------------------------------------------------------------------------------------------------
        private double runSingleExperiment(Dictionary<int, Component> components, List<int> nodesToReveal, double ageDiff)
        {
            TroubleshooterLoger._instance.markPriorObservedValues(components);
            Dictionary<int, int> sensorsRevealedSet = getSensorsPreTestValues(nodesToReveal, components);
            _troubleshooter._model.initModel(components);
            double totalCost = _troubleshooter.fixSystem(sensorsRevealedSet);
            AddCSVLine("smallNetwork", _troubleshooter.ToString(), nodesToReveal.Count, ageDiff, totalCost);
            return totalCost;
        }
        //----------------------------------------------------------------------------------------------------------------------
        public void runExperimentsOverTime(double Tlimit, IWorldAfterRepair worldAfterRepair, ITroubleShooterRepairingPolicy fixPolicy, List<int> nodesToReveal)
        {
            _troubleshooter._worldAfterRepair = worldAfterRepair;
            Dictionary<int, Component> components = _experiments[0]._components;
            _troubleshooter._model.initModel(components);
            initRepairCosts();
            //initRepairCostsDiffRatios();
            initSurvivals();
            int nFaults; int nFix; int nReplace;
            TroubleshooterLoger._instance.setLogFileName("troubleshooter_" + fixPolicy.ToString() + "_" + Tlimit);
            CreatorOverTimeLoger._instance.setLogFileName("Creator_" + fixPolicy.ToString() + "_" + Tlimit);
            
            for (int i=0; i< 30; i++)
            {
                //troubleshootingOverTime(fixPolicy, Tlimit, N_INTERVALS, new Dictionary<int, int>(), out nFaults);
               double totalCost = _troubleshooter.troubleshootingOverTime(fixPolicy, Tlimit, N_INTERVALS, new Dictionary<int, int>(), out nFaults, out nFix, out nReplace);

               AddCSVLine(_troubleshooter._repairPolicy, FIX_RATIO, Tlimit, nFaults, nFix, nReplace, _troubleshooter.nhealthyReplaced, totalCost);

               resetAges(); initSurvivals();
                _troubleshooter.initTroubleshooter();
            }
            TroubleshooterLoger._instance.writeText("=======================================================================Finished Params Iteration========================================================================");
            CreatorOverTimeLoger._instance.writeText("=======================================================================Finished Params Iteration========================================================================");

        }

        public void runSingleExperimentOverTime(double Tlimit, IWorldAfterRepair worldAfterRepair, ITroubleShooterRepairingPolicy fixPolicy, List<int> nodesToReveal, List<double> costsRatios, double maxRatio)
        {
            _troubleshooter._worldAfterRepair = worldAfterRepair;
            Dictionary<int, Component> components = _experiments[0]._components;
            _troubleshooter._model.initModel(components);

            initRepairCosts(costsRatios);
            initSurvivals();
            int nFaults; int nFix; int nReplace;
            TroubleshooterLoger._instance.setLogFileName("troubleshooter_" + fixPolicy.ToString() + "_" + Tlimit);
            CreatorOverTimeLoger._instance.setLogFileName("Creator_" + fixPolicy.ToString() + "_" + Tlimit);


            //troubleshootingOverTime(fixPolicy, Tlimit, N_INTERVALS, new Dictionary<int, int>(), out nFaults);
            double totalCost = _troubleshooter.troubleshootingOverTime(fixPolicy, Tlimit, N_INTERVALS, new Dictionary<int, int>(), out nFaults, out nFix, out nReplace);

            AddCSVLine(_troubleshooter._repairPolicy, maxRatio, Tlimit, nFaults, nFix, nReplace, _troubleshooter.nhealthyReplaced, totalCost);

            resetAges(); initSurvivals();
            _troubleshooter.initTroubleshooter();
            
            //TroubleshooterLoger._instance.writeText("=======================================================================Finished Params Iteration========================================================================");
            //CreatorOverTimeLoger._instance.writeText("=======================================================================Finished Params Iteration========================================================================");

        }
        //----------------------------------------------------------------------------------------------------------------------

        private Dictionary<int, int> getSensorsPreTestValues(List<int> sensors, Dictionary<int, Component> components)
        {
            Dictionary<int, int> sensorsValues = new Dictionary<int, int>();
            foreach (var sensor in sensors)
            {
                sensorsValues.Add(sensor, components[sensor]._preTestObservation);
            }
            return sensorsValues;
        }
        //----------------------------------------------------------------------------------------------------------------------
        private void initRepairCosts()
        {
            foreach (var comp in  _troubleshooter._model._testComponents)
            {
                _troubleshooter._model._components[comp]._replaceCost = REPLACE_COST;
                _troubleshooter._model._components[comp]._repairCost = FIX_RATIO * REPLACE_COST;
            }
        }
        private void initRepairCosts(List<double> costRatios)
        {
            int i = 0;
            foreach (var comp in _troubleshooter._model._testComponents)
            {
                _troubleshooter._model._components[comp]._replaceCost = REPLACE_COST;
                _troubleshooter._model._components[comp]._repairCost = costRatios.ElementAt(i) * REPLACE_COST;
                i++;
            }
        }
        //private void initRepairCostsDiffRatios()
        //{
        //    Random rand = new Random(5);
        //    double[] ratios = { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
        //    foreach (var comp in _troubleshooter._model._testComponents)
        //    {
        //        _troubleshooter._model._components[comp]._replaceCost = REPLACE_COST;
        //        int place = rand.Next(0, ratios.Length);
        //        _troubleshooter._model._components[comp]._repairCost = ratios[(int)place] * REPLACE_COST;
        //    }
        //}
        //----------------------------------------------------------------------------------------------------------------------
        private void initSurvivals()
        {
            SurvivalBayesModel model = (SurvivalBayesModel)_troubleshooter._model;
            foreach (var comp in _troubleshooter._model._testComponents)
            {
                model.updateSurvivalCurve(comp, ExperimentRunner.SURVIVAL_FACTOR_NEW);
                //model.updateSurvivalCurve(comp, ExperimentRunner.SURVIVAL_FACTOR_NEW * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
                
                //model.updateSurvivalCurve(comp, 0.0000001);

            }
            model.updateSurvivalCurve(7, ExperimentRunner.SURVIVAL_FACTOR_NEW);
        }
        //----------------------------------------------------------------------------------------------------------------------

        #region inputs&outputs
        public void readFilesOverTime()
        {
            _files = Directory.GetFiles(EXPERIMENTS_FILE_PATH);
            _nFiles = 0;
            foreach (var file in _files)
            {
                StreamReader sr = new StreamReader(file);
                string[] costs = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] ages = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] survivals = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] testComponentsString = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] testObservedValues = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                string[] controlComponentsString = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] controlObservedValues = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                string[] sensorsComponenetsString = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                string[] sensorsComponenetsObserevedValues = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                sr.Close();
            
                Dictionary<int, Component> components = new Dictionary<int,Component>();
                for (int i=0; i< testComponentsString.Length; i++)
                {
                
                    int compID = int.Parse(testComponentsString[i]);
                    Component component = new Component(compID);
                    component._number = compID;
                    component._preTestObservation = int.Parse(testObservedValues[i]);
                    component._componentType = ComponentType.TEST;
                    component._age = double.Parse(ages[i]);
                    component._repairCost = int.Parse(costs[i]);
                    component._survivalFactor = double.Parse(survivals[i]);
                    components.Add(compID, component);
                }

                for (int i=0; i< controlComponentsString.Length; i++)
                {
                    int compID = int.Parse(controlComponentsString[i]);
                    Component component = new Component(compID);             
                    component._number = compID;
                    component._preTestObservation = int.Parse(controlObservedValues[i]);
                    component._componentType = ComponentType.COMMAND;
                    components.Add(compID, component);
                }

                for (int i=0; i< sensorsComponenetsString.Length; i++)
                {
                    int compID = int.Parse(sensorsComponenetsString[i]);
                    Component component = new Component(compID);   
                    component._number = compID;
                    component._preTestObservation = int.Parse(sensorsComponenetsObserevedValues[i]);
                    component._componentType = ComponentType.SENSOR;
                    components.Add(compID, component);
                }
                Experiment exper = new Experiment(components);
                _experiments.Add(exper);
                _nFiles++;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------
        private void AddCSVLine(string name_of_DB, string Algorithm, int numberOfRevealed, double ageDiff, double totalCost)
        {

            //string Experiment_path_txt_ = CommonDefines.Experiment_path_txt;
            var sb = new StringBuilder();
            const string delimiter = ",";
            var length = 0; ;
            if (!File.Exists(CSV_FILE_Defualt))
            {
                File.Create(CSV_FILE_Defualt).Close();
                var Title = new[] { new[] { "Network :", "Algorithm :", "numberOfNodesRevealed :", "MaxAgeDiff", "TotalCost" } };
                length = Title.GetLength(0);
                for (int index = 0; index < length; index++)
                    sb.AppendLine(string.Join(delimiter, Title[index]));
            }

            var output = new[] { new[] { name_of_DB, Algorithm, numberOfRevealed.ToString(), ageDiff.ToString(), totalCost.ToString() } };
            length = output.GetLength(0);
            for (int index = 0; index < length; index++)
                sb.AppendLine(string.Join(delimiter, output[index]));
            File.AppendAllText(CSV_FILE_Defualt, sb.ToString());
        }
        //----------------------------------------------------------------------------------------------------------------------
        private void AddCSVLine(ITroubleShooterRepairingPolicy fixAlgo, double fixRatio, double timeLimit, int nFaults, double totalCost)
        {
            //string Experiment_path_txt_ = CommonDefines.Experiment_path_txt;
            var sb = new StringBuilder();
            const string delimiter = ",";
            var length = 0; ;
            if (!File.Exists(CSV_FILE_overTime))
            {
                File.Create(CSV_FILE_overTime).Close();
                var Title = new[] { new[] { "fixAlgo :", "FixRatio :", "PunishFactor: ", "Time-Limit :", "nFaults", "TotalCost" } };
                length = Title.GetLength(0);
                for (int index = 0; index < length; index++)
                    sb.AppendLine(string.Join(delimiter, Title[index]));
            }

            var output = new[] { new[] { fixAlgo.ToString(), fixRatio.ToString(), SURVIVAL_FACTOR_REDUCE.ToString(), timeLimit.ToString(), nFaults.ToString(), totalCost.ToString() } };
            length = output.GetLength(0);
            for (int index = 0; index < length; index++)
                sb.AppendLine(string.Join(delimiter, output[index]));
            File.AppendAllText(CSV_FILE_overTime, sb.ToString());
        }

        private void AddCSVLine(ITroubleShooterRepairingPolicy fixAlgo, double fixRatio, double timeLimit, int nFaults, int nFix, int nReplace, int nHelathyReplaced, double totalCost)
        {
            //string Experiment_path_txt_ = CommonDefines.Experiment_path_txt;
            var sb = new StringBuilder();
            const string delimiter = ",";
            var length = 0; ;
            if (!File.Exists(CSV_FILE_overTime))
            {
                File.Create(CSV_FILE_overTime).Close();
                var Title = new[] { new[] { "fixAlgo :", "FixRatio :", "PunishFactor: ", "Time-Limit :", "nFaults", "nFix", "nReplace", "nHealthyReplaced", "overHeadRatio", "TotalCost" } };
                length = Title.GetLength(0);
                for (int index = 0; index < length; index++)
                    sb.AppendLine(string.Join(delimiter, Title[index]));
            }

            var output = new[] { new[] { fixAlgo.ToString(), fixRatio.ToString(), SURVIVAL_FACTOR_REDUCE.ToString(), timeLimit.ToString(), nFaults.ToString(), nFix.ToString(), nReplace.ToString(), nHelathyReplaced.ToString(), OVERHEAD_RATIO.ToString(), totalCost.ToString() } };
            length = output.GetLength(0);
            for (int index = 0; index < length; index++)
                sb.AppendLine(string.Join(delimiter, output[index]));
            File.AppendAllText(CSV_FILE_overTime, sb.ToString());
        }
        //----------------------------------------------------------------------------------------------------------------------
        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        private void resetAges()
        {
            foreach (var comp in _troubleshooter._model._components)
            {
                comp.Value._age = 0;
            }
        }



        //----------------------------------------------------------------------------------------------------------------------

        #region Reading
        //----------------------------------------------------------------------------------------------------------------------
         private Dictionary<int, Component> readExperimentFile(string file, ref Dictionary<int, Component> sensorsComps)
        {
            StreamReader sr = new StreamReader(file);
            string[] costs = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] ages = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] survivals = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] testComponentsString = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] testObservedValues = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            string[] controlComponentsString = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] controlObservedValues = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            string[] sensorsComponenetsString = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] sensorsComponenetsObserevedValues = sr.ReadLine().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            sr.Close();
            
            Dictionary<int, Component> components = new Dictionary<int,Component>();
            for (int i=0; i< testComponentsString.Length; i++)
            {
                
                int compID = int.Parse(testComponentsString[i]);
                Component component = new Component(compID);
                component._number = compID;
                component._preTestObservation = int.Parse(testObservedValues[i]);
                component._componentType = ComponentType.TEST;
                component._age = double.Parse(ages[i]);
                component._repairCost = int.Parse(costs[i]);
                component._survivalFactor = double.Parse(survivals[i]);
                components.Add(compID, component);
            }

            for (int i=0; i< controlComponentsString.Length; i++)
            {
                int compID = int.Parse(controlComponentsString[i]);
                Component component = new Component(compID);             
                component._number = compID;
                component._preTestObservation = int.Parse(controlObservedValues[i]);
                component._componentType = ComponentType.COMMAND;
                components.Add(compID, component);
            }

            for (int i=0; i< sensorsComponenetsString.Length; i++)
            {
                int compID = int.Parse(sensorsComponenetsString[i]);
                Component component = new Component(compID);   
                component._number = compID;
                component._preTestObservation = int.Parse(sensorsComponenetsObserevedValues[i]);
                component._componentType = ComponentType.SENSOR;
                components.Add(compID, component);
                sensorsComps.Add(compID, component);
            }
            return components;

        }
        //----------------------------------------------------------------------------------------------------------------------
        private void readExperiments(string folderName)
         {
            string[] files = Directory.GetFiles(folderName);
            string folderNameString = folderName.Substring(folderName.LastIndexOf('\\')+1);
            double ageDiff = parseAgeDiff(folderName);
            int fileNumber = 0;
            foreach (string fileName in files)
            {
                Dictionary<int, Component> sensorsComponents = new Dictionary<int, Component>();
                Dictionary<int, Component> components = readExperimentFile(fileName, ref sensorsComponents);
                Experiment exper = new Experiment(components, ageDiff);
                _experiments.Add(exper);
                fileNumber++;
            }
            _nFiles += fileNumber;
         }
        //----------------------------------------------------------------------------------------------------------------------
        public void readExperiments()
        {
            _nFiles = 0;
            string[] directories = Directory.GetDirectories(EXPERIMENTS_FILE_PATH);
            foreach (var directory in directories)
                readExperiments(directory);
            Console.WriteLine();
        }

        //----------------------------------------------------------------------------------------------------------------------
        private double parseAgeDiff(string folderName)
        {
            string ageDiff = folderName.Substring(folderName.IndexOf('_')+1);
            return Convert.ToDouble(ageDiff);
        }
        //----------------------------------------------------------------------------------------------------------------------
        #endregion

        //----------------------------------------------------------------------------------------------------------------------

        public static double getNewFixCurve(double currCurve, double currTime)
        {
            SurvivalFunctions.SurvivalFunction survFunc = new SurvivalFunctions.WeibullCurve(SURVIVAL_FACTOR_NEW);
            double newCurvProb = survFunc.survive(currTime);
            survFunc.setParameter(SURVIVAL_FACTOR_NEW * SURVIVAL_FACTOR_REDUCE);
            double oldCurvProb = survFunc.survive(currTime);
            double ageFactor = 1-(oldCurvProb / newCurvProb);
            //double ageFactor = (newCurvProb - oldCurvProb);

            //if (ageFactor > 0)
            //    Console.WriteLine(ageFactor);

            //double ans = currCurve * SURVIVAL_FACTOR_REDUCE;
            //double ans = SURVIVAL_FACTOR_NEW * SURVIVAL_FACTOR_REDUCE;

            double ans = SURVIVAL_FACTOR_NEW * (ageFactor + 1.4);
            //Console.WriteLine(ans);

            return ans;
        }

        public static double getNewCurve()
        {
            return SURVIVAL_FACTOR_NEW;
        }

        //public static double getOverheadRatio() 
        //{
        //    return OVERHEAD_RATIO * REPLACE_COST;

        //}

    }
}
