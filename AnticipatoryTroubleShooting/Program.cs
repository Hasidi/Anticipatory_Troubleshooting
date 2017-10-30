using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.Models;
using AnticipatoryTroubleShooting.SurvivalFunctions;

namespace AnticipatoryTroubleShooting
{
    class Program
    {
        static void Main(string[] args)
        {
            //Dictionary<int, double> faultsQueue = new Dictionary<int, double>() { { 1, 0.5 }, { 2, 0.2 }, { 3, 0.1 }, { 4, 0.8 } };

            //var minTime = faultsQueue.Select(item => Convert.ToDouble(item.Value)).Min();
            //var element = faultsQueue.First(kvp => kvp.Value == Convert.ToDouble(minTime));

            //faultsQueue.Remove(element.Key);

            //UnitTest unitTest = new UnitTest();
            //SurvivalFunction survFunc = new ExponentialDecayCurve(1, 0.12);
            //Interval interval = new Interval(8,12);
            //double diff = unitTest.intervalSurviveTime(survFunc, 0.209216, interval, 3, 6);

            Model model = new BayesianNetworkModel("PaperExample.txt", "PaperExample_Types.txt");
            //model.updateObservedValue(4, 1);
            model.updateObservedValue(3, 1);
            double[] dis0 = model.getComponentDistribution(0);
            double[] dis1 = model.getComponentDistribution(1);
            double[] dis2 = model.getComponentDistribution(2);

            //Model model2 = new Survival_IN_BayesModel("AlarmNetwork_.txt", "AlarmNetwork_Types.txt");
            //Model model2 = new Survival_IN_BayesModel("myDetails2.txt", "networkFileTypes2.txt");
            //Model model = new Survival_IN_BayesModel("myDetails.txt", "networkFileTypes.txt");

            //Diagnoser diagnoser = new Diagnoser(model, null, new ConstCostProbSelector());
            //TroubleShooter troubleshooter = new TroubleShooter(model, diagnoser, new HybridFixPolicy());
            //diagnoser._troubleShooter = troubleshooter;

            //ExperimentCreator expCreator = new ExperimentCreator((Survival_IN_BayesModel)model);
            //createExperiments(expCreator);


            //runExperiments("myDetails.txt", "networkFileTypes.txt");

            runExperimetnsOverTime3("myDetails2.txt", "networkFileTypes2.txt");
            //runExperimetnsOverTimeDiffCost("myDetails2.txt", "networkFileTypes2.txt");

            //runExperimetnsOverTimeDiffCost_EdgeCost("myDetails2.txt", "networkFileTypes2.txt");
            //System.Diagnostics.Process.GetCurrentProcess().Kill();

        }

        //-----------------------------------------------------------------------------------------------------------
        public static void createExperiments(ExperimentCreator expCreator)
        {
            List<double> agesDiff = new List<double>() { 0.01, 0.05, 0.1, 0.3, 0.5, 1, 1.5, 2, 2.5, 5, 10, 15, 20, 25 };
            //List<double> agesDiff = new List<double>() { 0.01, 0.05, 1, 3, 7, 15, 20 };

            foreach (var ageDiff in agesDiff)
            {
                expCreator.createExperiments(100, ageDiff, 0.3);
            }
        }

        //-----------------------------------------------------------------------------------------------------------
        public static void runExperiments(string filePath, string compsTypesPath)
        {
            List<TroubleShooter> troubleshooters = setTroubleshooters(filePath, compsTypesPath);
            Model model = troubleshooters[0]._model;
            int revealedHop = model._sensorComponents.Count / 5;
            Dictionary<int, List<List<int>>> nodesToReveal = new Dictionary<int, List<List<int>>>();

            ExperimentRunner experimentRunner = new ExperimentRunner(troubleshooters[0]);
            experimentRunner.readExperiments();
            //int nExperiments = experimentRunner._experiments.Count;
            //for (int i=0; i<model._sensorComponents.Count; i+= revealedHop)
            //{
            //    List<List<int>> currNodesToReveal = randSensorsToReveal(model, i, 1);
            //    nodesToReveal.Add(currNodesToReveal[0]);
            //}

            for (int k = 0; k < experimentRunner._experiments.Count; k++)
            {
                List<List<int>> experimentNodesToReveal = new List<List<int>>();
                nodesToReveal.Add(k, experimentNodesToReveal);
                for (int i = 0; i < model._sensorComponents.Count; i += revealedHop)
                {
                    List<List<int>> currNodesToReveal = randSensorsToReveal(model, i, 1);
                    experimentNodesToReveal.Add(currNodesToReveal[0]);
                }
            }

            //for (int k = 0; k < experimentRunner._experiments.Count; k++)
            //{
            //    List<List<int>> experimentNodesToReveal = new List<List<int>>();
            //    nodesToReveal.Add(k, experimentNodesToReveal);
            //    List<List<int>> currNodesToReveal = randSensorsToReveal(model, 7, 1);
            //    experimentNodesToReveal.Add(currNodesToReveal[0]);
            //}

            foreach (var troubleshooter in troubleshooters)
            {
                experimentRunner = new ExperimentRunner(troubleshooter);
                experimentRunner.readExperiments();
                experimentRunner.runExperimentsOverAgesDiff(nodesToReveal);
            }

        }
        //-----------------------------------------------------------------------------------------------------------
        public static List<TroubleShooter> setTroubleshooters(string filePath, string compsTypesPath)
        {

            Model model1 = new Survival_IN_BayesModel(filePath, compsTypesPath);
            Model model2 = new BayesianNetworkModel(filePath, compsTypesPath);
            Model model3 = new SurvivalModel(new BayesianNetworkModel(filePath, compsTypesPath));
            Model model4 = new RandomModel(new BayesianNetworkModel(filePath, compsTypesPath));
            //Model model4 = new SurvivalBayesModel(filePath, compsTypesPath);
            List<Model> models = new List<Model>();
            models.Add(model3);
            models.Add(model1); models.Add(model2); 
            //models.Add(model4);
            List<TroubleShooter> troubleshooters = new List<TroubleShooter>();
            foreach (var model in models)
            {
                Diagnoser diagnoser = new Diagnoser(model, null, new ConstCostProbSelector());
                TroubleShooter troubleshooter = new TroubleShooter(model, diagnoser, new FixingRepairPolicyDecreasing());
                diagnoser._troubleShooter = troubleshooter;
                troubleshooters.Add(troubleshooter);
            }
            Diagnoser diagnoser4 = new Diagnoser(model4, null, new RandomSelector());
            TroubleShooter troubleshooter4 = new TroubleShooter(model4, diagnoser4, new FixingRepairPolicyDecreasing());
            diagnoser4._troubleShooter = troubleshooter4;
            troubleshooters.Add(troubleshooter4);
            return troubleshooters;
        }
        //-----------------------------------------------------------------------------------------------------------

        public static void runExperimetnsOverTime(string filePath, string compsTypesPath)
        {
            Model model = new Survival_IN_BayesModel(filePath, compsTypesPath);
            Diagnoser diagnoser = new Diagnoser(model, null, new ConstCostProbSelector());
            TroubleShooter troubleshooter = new TroubleShooter(model, diagnoser, new FixingRepairPolicyDecreasing());
            diagnoser._troubleShooter = troubleshooter;
            ExperimentRunner experimentRunner = new ExperimentRunner(troubleshooter);

            //List<double> fixRatios = new List<double>() { 0.01, 0.04, 0.07, 0.1, 0.15, 0.2, 0.4, 0.6, 0.8, 0.9, 0.92, 0.94, 0.96, 1 };
            //List<double> punishFactor = new List<double>() { 1, 1.01, 1.05, 1.1, 1.3, 1.4, 1.5, 1.6, 1.8, 2, 2.4, 3 };

            //List<double> fixRatios = new List<double>() { 0.6, 0.7, 0.8, 0.9, 1 };
            //List<double> punishFactor = new List<double>() { 1, 1.6, 1.7, 1.8 };

            List<double> fixRatios = new List<double>() { 0.7 };
            List<double> punishFactor = new List<double>() { 1.8 };
            experimentRunner.readFilesOverTime();
            for (int i = 0; i < punishFactor.Count; i++)
            {
                ExperimentRunner.SURVIVAL_FACTOR_REDUCE = punishFactor[i];
                for (int j = 0; j < fixRatios.Count; j++)
                {
                    ExperimentRunner.FIX_RATIO = fixRatios[j];

                    experimentRunner.runExperimentsOverTime(28, new DecresingSurival(), new FixingRepairPolicyDecreasing(), new List<int>());
                    experimentRunner.runExperimentsOverTime(28, new DecresingSurival(), new HybridRepairPolicyDecreasing(), new List<int>());
                    experimentRunner.runExperimentsOverTime(28, new DecresingSurival(), new ReplacingRepairPolicy(), new List<int>());
                    //experimentRunner.runExperimentsOverTime(28, new DecresingSurival(), new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter), new List<int>());

                    //experimentRunner.runExperimentsOverTime(28, new ConstSurvial(), new FixingRepairPolicy(), new List<int>());
                    //experimentRunner.runExperimentsOverTime(28, new ConstSurvial(), new HybridRepairPolicy(), new List<int>());
                    //experimentRunner.runExperimentsOverTime(28, new ConstSurvial(), new ReplacingRepairPolicy(), new List<int>());
                }
            }
        }
        // BE Careful, ConstSurvival can be work only with no decreasing repair policy
        //public static void runExperimetnsOverTime2(string filePath, string compsTypesPath)
        //{
        //    Model model = new Survival_IN_BayesModel(filePath, compsTypesPath);
        //    Diagnoser diagnoser = new Diagnoser(model, null, new ConstCostProbSelector());
        //    TroubleShooter troubleshooter = new TroubleShooter(model, diagnoser, new FixingRepairPolicyDecreasing());
        //    diagnoser._troubleShooter = troubleshooter;
        //    ExperimentRunner experimentRunner = new ExperimentRunner(troubleshooter);

        //    List<double> fixRatios = new List<double>() { 0.01, 0.04, 0.07, 0.1, 0.15, 0.2, 0.4, 0.6, 0.8, 0.9, 0.92, 0.94, 0.96, 1 };

        //    //List<double> fixRatios = new List<double>() {0.4 };
        //    experimentRunner.readFilesOverTime();

        //    for (int j = 0; j < fixRatios.Count; j++)
        //    {
        //        ExperimentRunner.FIX_RATIO = fixRatios[j];
        //        experimentRunner.runExperimentsOverTime(28, new ConstSurvial(), new FixingRepairPolicy(), new List<int>());
        //        experimentRunner.runExperimentsOverTime(28, new ConstSurvial(), new HybridRepairPolicy(), new List<int>());
        //        experimentRunner.runExperimentsOverTime(28, new ConstSurvial(), new ReplacingRepairPolicy(), new List<int>());
        //    }
            
        //}

        public static void runExperimetnsOverTime3(string filePath, string compsTypesPath)
        {
            Model model = new Survival_IN_BayesModel(filePath, compsTypesPath);
            Diagnoser diagnoser = new Diagnoser(model, null, new ConstCostProbSelector());
            TroubleShooter troubleshooter = new TroubleShooter(model, diagnoser, new FixingRepairPolicyDecreasing());
            diagnoser._troubleShooter = troubleshooter;
            ExperimentRunner experimentRunner = new ExperimentRunner(troubleshooter);

            //List<double> fixRatios = new List<double>() { 0.01, 0.05, 0.1, 0.15, 0.2, 0.4, 0.6, 0.8, 0.85, 0.9, 0.95, 1 };
            //List<double> punishFactor = new List<double>() { 1, 1.2, 1.4, 1.6, 1.8, 2, 2.2, 2.4};

            //List<double> fixRatios = new List<double>() { 0.6 };
            //List<double> punishFactor = new List<double>() { 1.8 };

            List<double> fixRatios = new List<double>() { 0.1, 0.4, 0.6, 0.7, 0.8, 0.85, 0.9 };
            List<double> punishFactor = new List<double>() { 1.2, 1.4, 1.6, 1.8, 2, 2.2 };

            Dictionary<ITroubleShooterRepairingPolicy, TimeSpan> algorithmsList = new Dictionary<ITroubleShooterRepairingPolicy, TimeSpan>();

            //algorithmsList.Add(new ReplacingRepairPolicy(), new TimeSpan());
            //algorithmsList.Add(new FixingRepairPolicyDecreasing(), new TimeSpan());
            //algorithmsList.Add(new HybridRepairPolicyDecreasing(), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 1), new TimeSpan());

            ////algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 2), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 3), new TimeSpan());

            algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 4), new TimeSpan());
            ////algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 6), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 8), new TimeSpan());

            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 10), new TimeSpan());




            Stopwatch timer = new Stopwatch();

            experimentRunner.readFilesOverTime();
            for (int a=0; a<algorithmsList.Count; a++)
            {
                ITroubleShooterRepairingPolicy algorithm = algorithmsList.ElementAt(a).Key;
                timer.Restart();
                for (int i = 0; i < punishFactor.Count; i++)
                {
                    ExperimentRunner.SURVIVAL_FACTOR_REDUCE = punishFactor[i];

                    for (int j = 0; j < fixRatios.Count; j++)
                    {
                        UsefulFunctions.RANDOM = new Random(10);

                        ExperimentRunner.FIX_RATIO = fixRatios[j];
                        experimentRunner.runExperimentsOverTime(30, new DecresingSurival(), algorithm, new List<int>());
                    }
                }
                timer.Stop();
                algorithmsList[algorithm] = timer.Elapsed;
                Console.WriteLine("done");
                //UsefulFunctions.RANDOM = new Random(10);

            }
            Console.WriteLine("All Done");

        }
        //-----------------------------------------------------------------------------------------------------------
        public static void runExperimetnsOverTimeDiffCost(string filePath, string compsTypesPath)
        {
            Model model = new Survival_IN_BayesModel(filePath, compsTypesPath);
            Diagnoser diagnoser = new Diagnoser(model, null, new ConstCostProbSelector());
            TroubleShooter troubleshooter = new TroubleShooter(model, diagnoser, new FixingRepairPolicyDecreasing());
            diagnoser._troubleShooter = troubleshooter;
            ExperimentRunner experimentRunner = new ExperimentRunner(troubleshooter);

            //List<double> fixRatios = new List<double>() { 0.01, 0.05, 0.1, 0.15, 0.2, 0.4, 0.6, 0.8, 0.85, 0.9, 0.95, 1 };
            //List<double> punishFactor = new List<double>() { 1, 1.2, 1.4, 1.6, 1.8, 2, 2.2, 2.4};

            //List<double> fixRatios = new List<double>() { 0.6 };
            List<double> punishFactor = new List<double>() { 1.8};

            //List<double> punishFactor = new List<double>() {  1.8,2,2.1, 2.2,2.4 };

            Dictionary<ITroubleShooterRepairingPolicy, TimeSpan> algorithmsList = new Dictionary<ITroubleShooterRepairingPolicy, TimeSpan>();

            algorithmsList.Add(new ReplacingRepairPolicy(), new TimeSpan());
            algorithmsList.Add(new FixingRepairPolicyDecreasing(), new TimeSpan());
            //algorithmsList.Add(new HybridRepairPolicyDecreasing(), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 1), new TimeSpan());

            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 2), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 3), new TimeSpan());

            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 4), new TimeSpan());
            algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 6), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 8), new TimeSpan());

            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 10), new TimeSpan());

            algorithmsList.Add(new Troubleshooting.HealthyReplacementRepairingPolicy(troubleshooter, 6), new TimeSpan());


            experimentRunner.readFilesOverTime();
            Random randomSeed = new Random(50);
            List<Interval> intervalsCost = initCostIntervals();
            //List<int> randomSeeds = new List<int>() { 1, 2, 3, 4 };

            for (int p = 0; p < punishFactor.Count; p++)
            {
                ExperimentRunner.SURVIVAL_FACTOR_REDUCE = punishFactor[p];
                foreach (var interval in intervalsCost)
                { 
                    for (int j = 0; j < 1; j++)
                    {
                        //UsefulFunctions.RANDOM = new Random();
                        List <double> costs = randCosts(interval, model._testComponents.Count);
                        int seed = randomSeed.Next();
                        for (int a = 0; a < algorithmsList.Count; a++)
                        {
                            ITroubleShooterRepairingPolicy algorithm = algorithmsList.ElementAt(a).Key;

                            UsefulFunctions.RANDOM = new Random(seed);

                            experimentRunner.runSingleExperimentOverTime(40, new DecresingSurival(), algorithm, new List<int>(), costs, interval.Ur);
                        }
                    }
                }
           
                Console.WriteLine("done");
                //UsefulFunctions.RANDOM = new Random(10);

            }
            Console.WriteLine("All Done");

        }

        //-----------------------------------------------------------------------------------------------------------
        public static List<List<int>> randSensorsToReveal(Model model, int nReveal, int nPermutations)
        {
            List<List<int>> ans = new List<List<int>>();
            for (int i=0; i< nPermutations; i++)
            {
                List<int> sensors = new List<int>(model._sensorComponents);
                if (nReveal > sensors.Count)
                    throw new InvalidOperationException();
                List<int> sensorsToReveal = new List<int>();
                for (int j=0; j<nReveal; j++)
                {
                    int sensorRand = UsefulFunctions.randFromGivenSet(sensors);
                    sensors.Remove(sensorRand);
                    sensorsToReveal.Add(sensorRand);
                }
                ans.Add(sensorsToReveal);
            }
            return ans;
        }

        //-----------------------------------------------------------------------------------------------------------

        public static List<double> randCosts(Interval interval, int nTestComps )
        {
            List<double> ans = new List<double>();
            for (int i=0; i< nTestComps; i++)
            {
                double d = UsefulFunctions.randFromSpecificRange(interval.Lr, interval.Ur);
                ans.Add(d);
            }
            return ans;
        }


        //-----------------------------------------------------------------------------------------------------------

        public static List<Interval> initCostIntervals()
        {
            List<Interval> ans = new List<Interval>();
            //ans.Add(new Interval(0.1, 0.4));
            //ans.Add(new Interval(0.4, 0.65));
            //ans.Add(new Interval(0.3, 0.9));
            //ans.Add(new Interval(0.85, 0.95));


            ans.Add(new Interval(0.2, 0.6));
            //ans.Add(new Interval(0.75, 0.9));

            return ans;

        }





        public static void runExperimetnsOverTimeDiffCost_EdgeCost(string filePath, string compsTypesPath)
        {
            Model model = new Survival_IN_BayesModel(filePath, compsTypesPath);
            Diagnoser diagnoser = new Diagnoser(model, null, new ConstCostProbSelector());
            TroubleShooter troubleshooter = new TroubleShooter(model, diagnoser, new FixingRepairPolicyDecreasing());
            diagnoser._troubleShooter = troubleshooter;
            ExperimentRunner experimentRunner = new ExperimentRunner(troubleshooter);

            //List<double> fixRatios = new List<double>() { 0.01, 0.05, 0.1, 0.15, 0.2, 0.4, 0.6, 0.8, 0.85, 0.9, 0.95, 1 };
            //List<double> punishFactor = new List<double>() { 1, 1.2, 1.4, 1.6, 1.8, 2, 2.2, 2.4};

            //List<double> fixRatios = new List<double>() { 0.6 };
            List<double> punishFactor = new List<double>() { 1.8 };

            //List<double> punishFactor = new List<double>() {  1.8,2,2.1, 2.2,2.4 };

            Dictionary<ITroubleShooterRepairingPolicy, TimeSpan> algorithmsList = new Dictionary<ITroubleShooterRepairingPolicy, TimeSpan>();

            algorithmsList.Add(new ReplacingRepairPolicy(), new TimeSpan());
            algorithmsList.Add(new FixingRepairPolicyDecreasing(), new TimeSpan());
            algorithmsList.Add(new HybridRepairPolicyDecreasing(), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 1), new TimeSpan());

            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 2), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 3), new TimeSpan());

            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 4), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 6), new TimeSpan());
            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 8), new TimeSpan());

            //algorithmsList.Add(new Troubleshooting.DFS_HybridRepairPolicy(troubleshooter, 10), new TimeSpan());

            //algorithmsList.Add(new Troubleshooting.HealthyReplacementRepairingPolicy(troubleshooter, 8), new TimeSpan());


            experimentRunner.readFilesOverTime();
            Random randomSeed = new Random(2);
            List<Interval> intervalsCost = initCostIntervals();
            //List<int> randomSeeds = new List<int>() { 1, 2, 3, 4 };

            for (int p = 0; p < punishFactor.Count; p++)
            {
                ExperimentRunner.SURVIVAL_FACTOR_REDUCE = punishFactor[p];

                for (int j = 0; j < 100; j++)
                {
                    //UsefulFunctions.RANDOM = new Random();
                    List<double> costs = randCosts(model._testComponents.Count);
                    int seed = randomSeed.Next();
                    for (int a = 0; a < algorithmsList.Count; a++)
                    {
                        ITroubleShooterRepairingPolicy algorithm = algorithmsList.ElementAt(a).Key;

                        UsefulFunctions.RANDOM = new Random(seed);

                        experimentRunner.runSingleExperimentOverTime(40, new DecresingSurival(), algorithm, new List<int>(), costs, -1);
                    }
                }
                

                Console.WriteLine("done");
                //UsefulFunctions.RANDOM = new Random(10);

            }
            Console.WriteLine("All Done");

        }



        public static List<double> randCosts(int nComps)
        {
            List<double> costRatiosToSampleFrom = new List<double>() { 0.4, 0.9 };
            List<double> ans = new List<double>();
            for (int i = 0; i < nComps; i++)
            {
                double d = UsefulFunctions.randFromGivenSet(costRatiosToSampleFrom);
                ans.Add(d);
            }
            return ans;
        }










    }
}
