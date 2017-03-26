using AnticipatoryTroubleShooting.Logers;
using AnticipatoryTroubleShooting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class TroubleShooter
    {
        public Model _model;
        Diagnoser _diagnoser;
        public ITroubleShooterRepairingPolicy _repairPolicy;
        Dictionary<int, int> _currObservedComponents;
        public List<int> _currSuspectedComponents;
        public double FIX_RATIO = 0.6;
        static double _Tlimit;
        double _nIntervals;
        public IWorldAfterRepair _worldAfterRepair;
        public static double EPSILON = 0.001;

        public int nhealthyReplaced = 0;
        public static double MIN_HOP;
        #region constants
        double OBSERVE_SYSTEM_COST = 1;
        #endregion


        public TroubleShooter(Model model, Diagnoser diagnoser, ITroubleShooterRepairingPolicy fixPolicy)
        {
            _model = model;
            _diagnoser = diagnoser;
            _repairPolicy = fixPolicy;
            initTroubleshooter();

        }

        public TroubleShooter(Model model, Diagnoser diagnoser)
        {
            _model = model;
            _diagnoser = diagnoser;
            _repairPolicy = new ReplacingRepairPolicy();
            initTroubleshooter();

        }

        public TroubleShooter(TroubleShooter troubleshooterCopy)
        {
            _model = troubleshooterCopy._model;
            _diagnoser = troubleshooterCopy._diagnoser;
            _repairPolicy = troubleshooterCopy._repairPolicy;
            initTroubleshooter();
        }




        //-----------------------------------------------------------------------------------------------------------
        public void initTroubleshooter()
        {
            //_model.clearObservedValues();
            //i changed it when writing the DFS , maybe it cause problems in the first code
            _model.initModel();
            _currObservedComponents = new Dictionary<int, int>();
            _currSuspectedComponents = new List<int>();
            _diagnoser._nCurrBrokenComponents = 1;
            foreach (var testComp in _model._testComponents)
            {
                _currSuspectedComponents.Add(testComp);
            }
        }


        //-----------------------------------------------------------------------------------------------------------
        public double fixSystem(Dictionary<int, int> revealedSensors)
        {
            insertSensorsValues(revealedSensors);
            TroubleshooterLoger._instance.markRevealedSensors(revealedSensors);
            double totalFixCost = 0;
            int iteration = 0;
            while (!_diagnoser.systemHelathy())
            {
                Dictionary<int, Component> testComponentsObservations = _diagnoser.diagnose(); //every diagnoser has its own startegey
                int currSelectedComp = _diagnoser.selectComponentToObserve(testComponentsObservations);
                int observedValue = _diagnoser.observeComponent(currSelectedComp);
                TroubleshooterLoger._instance.printDiagnoserDecision(iteration, currSelectedComp, observedValue, testComponentsObservations);
                _currObservedComponents.Add(currSelectedComp, observedValue);
                if (observedValue != 0)
                {
                    _diagnoser._nCurrBrokenComponents--;
                    double fixCost = repairComponent(currSelectedComp, ReapirType.FIX);
                    //totalFixCost += fixCost;
                }
                else
                {
                    _model.updateObservedValue(currSelectedComp, 0);
                }
                _currSuspectedComponents.Remove(currSelectedComp);
                iteration++;
            }
            totalFixCost += iteration * OBSERVE_SYSTEM_COST;
            TroubleshooterLoger._instance.markEndOfExperiment(totalFixCost);
            initTroubleshooter();
            return totalFixCost;
        }
        //-----------------------------------------------------------------------------------------------------------
        public double fixSystemConsiderTime(Dictionary<int, int> revealedSensors, double timeLimit, double currTime, List<Interval> compIntervals)
        {
            double repairCost;
            insertSensorsValues(revealedSensors);
            TroubleshooterLoger._instance.markPriorObservedValues(_model._components);
            TroubleshooterLoger._instance.markRevealedSensors(revealedSensors);
            double totalFixCost = 0;
            int iteration = 0;
            string fixPolicyString = "";
            while (!_diagnoser.systemHelathy())
            {
                Dictionary<int, Component> testComponentsObservations = _diagnoser.diagnose(); //every diagnoser has its own startegey
                int currSelectedComp = _diagnoser.selectComponentToObserve(testComponentsObservations);
                int observedValue = _diagnoser.observeComponent(currSelectedComp);
                TroubleshooterLoger._instance.printDiagnoserDecision(iteration, currSelectedComp, observedValue, testComponentsObservations);
                _currObservedComponents.Add(currSelectedComp, observedValue);
                if (observedValue != 0)
                {
                    _diagnoser._nCurrBrokenComponents--;
                    ReapirType repairType = _repairPolicy.RepairComponentPolicy(_model, currSelectedComp, timeLimit, currTime, out fixPolicyString, out repairCost);
                    repairComponent(currSelectedComp, repairType); //////
                    double currCost = _model._components[currSelectedComp].getRepairCost(repairType);
                    totalFixCost += currCost;
                    TroubleshooterLoger._instance.printFixPolicy(repairType.ToString(), currCost, fixPolicyString);
                }
                else
                {
                    _model.updateObservedValue(currSelectedComp, 0);
                }
                _currSuspectedComponents.Remove(currSelectedComp);
                iteration++;
            }
            //totalFixCost += iteration * OBSERVE_SYSTEM_COST;
            TroubleshooterLoger._instance.markEndOfExperiment(totalFixCost);
            initTroubleshooter();

            return totalFixCost;
        }
        //-----------------------------------------------------------------------------------------------------------
        public double repairComponent(int compID, ReapirType repairAction)
        {
            _model.updateObservedValue(compID, 0);
            Component comp = _model._components[compID];
            SurvivalBayesModel svModel = (SurvivalBayesModel)_model;
            double repairCost = _model._components[compID].getRepairCost(repairAction);
            if (repairAction == ReapirType.FIX)
            {
                comp._age = 0;
                svModel.updateSurvivalCurve(compID, ExperimentRunner.getNewFixCurve(comp._survivalFactor));
                //svModel.updateSurvivalCurve(compID, comp._survivalFactor * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
            }
            if (repairAction == ReapirType.REPLACE)
            {
                comp._age = 0;
                svModel.updateSurvivalCurve(compID, ExperimentRunner.getNewCurve());
            }

            return repairCost;
        }

        //public double repairComponent2(int compID, ReapirType repairAction, State state)
        //{
        //    _model.updateObservedValue(compID, 0);
        //    Component comp = _model._components[compID];
        //    SurvivalBayesModel svModel = (SurvivalBayesModel)_model;
        //    double repairCost = _model._components[compID].getRepairCost(repairAction);
        //    if (repairAction == ReapirType.FIX)
        //    {
        //        comp._age = 0;
        //        svModel.updateSurvivalCurve(compID, ExperimentRunner.SURVIVAL_FACTOR_NEW * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
        //        //svModel.updateSurvivalCurve(compID, comp._survivalFactor * ExperimentRunner.SURVIVAL_FACTOR_REDUCE);
        //    }
        //    else  /* if (repairAction == ReapirType.REPLACE)*/
        //    {
        //        comp._age = 0;
        //        svModel.updateSurvivalCurve(compID, ExperimentRunner.SURVIVAL_FACTOR_NEW);
        //    }

        //    return repairCost;
        //}
        ////-----------------------------------------------------------------------------------------------------------
        public void insertSensorsValues(Dictionary<int, int> revealedSensors)
        {
            foreach (var sensorComp in revealedSensors)
            {
                _model.updateObservedValue(sensorComp.Key, sensorComp.Value);
            }
            foreach (var compCommand in _model._controlComponents)
            {
                _model.updateObservedValue(compCommand, _model._components[compCommand]._preTestObservation);
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public double fixSystemConsiderTime(Dictionary<int, int> revealedSensors, double timeLimit, double currTime, int faultComp, List<Interval> compIntervals, out ReapirType repairAns)
        {
            double repairCost;
            insertSensorsValues(revealedSensors);
            //TroubleshooterLoger._instance.markPriorObservedValues(_model._components);
            //TroubleshooterLoger._instance.markRevealedSensors(revealedSensors);
            TroubleshooterLoger._instance.writeText("currTime: " + currTime);
            double totalFixCost = 0;
            int iteration = 0;
            string fixPolicyString = "";

            int observedValue = _diagnoser.observeComponent(faultComp);
            TroubleshooterLoger._instance.printDiagnoserDecision(iteration, faultComp, observedValue, new Dictionary<int, Component>());
            _currObservedComponents.Add(faultComp, observedValue);
            ReapirType repairType = 0;
            if (observedValue != 0)
            {
                _diagnoser._nCurrBrokenComponents--;
                repairType = _repairPolicy.RepairComponentPolicy(_model, faultComp, timeLimit, currTime, out fixPolicyString, out repairCost);
                
                double currCost = repairComponent(faultComp, repairType); //////

                if (_repairPolicy is Troubleshooting.HealthyReplacementRepairingPolicy)
                    currCost = repairCost;  // זמני
                totalFixCost += currCost;
                TroubleshooterLoger._instance.printFixPolicy(repairType.ToString(), currCost, fixPolicyString);
            }
            else
            {
                _model.updateObservedValue(faultComp, 0);
            }
            _currSuspectedComponents.Remove(faultComp);

            repairAns = repairType;
            //iteration++; // if we want to use observe system cost

            totalFixCost += iteration * OBSERVE_SYSTEM_COST;
            TroubleshooterLoger._instance.markEndOfExperiment(totalFixCost);
            initTroubleshooter();

            return totalFixCost;
        }
        //-----------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return _model.ToString() + "_troubleshooter";
        }


        //-----------------------------------------------------------------------------------------------------------
        ///// <summary>
        ///// Main method
        ///// </summary>
        ///// <param name="repairPolicy"></param>
        ///// <param name="Tlimit"></param>
        ///// <param name="nIntervals"></param>
        ///// <param name="revealedSensors"></param>
        ///// <param name="nFaults"></param>
        ///// <returns></returns>
        //public double troubleshootingOverTime(ITroubleShooterRepairingPolicy repairPolicy, double Tlimit, double nIntervals,
        //                          Dictionary<int, int> revealedSensors, out int nFaults, out int nFix, out int nReplace)
        //{
        //    _Tlimit = Tlimit;
        //    nFix = 0; nReplace = 0;
        //    nFaults = 0;
        //    _repairPolicy = repairPolicy;
        //    //TroubleshooterLoger._instance.setLogFileName(_folderName, Tlimit);
        //    //CreatorOverTimeLoger._instance.setLogFileName(fixPolicy.ToString() + "_" + Tlimit);
        //    double totalCost = 0;

        //    _nIntervals = nIntervals;
        //    MinHeap<IntervalFault> faultsQueue = new MinHeap<IntervalFault>();
        //    Dictionary<int, List<Interval>> compsIntervalsDis = cutsIntervals(Tlimit, nIntervals);
        //    sampleIntervalsFaults(faultsQueue, compsIntervalsDis, Tlimit, _model._testComponents);
        //    CreatorOverTimeLoger._instance.markIntervalsProbs2(compsIntervalsDis, _model);
        //    double prevTime = 0;
        //    int i = 0;
        //    while (faultsQueue.Count > 0) //note : new element can add during the loop - therefore using while
        //    {
        //        IntervalFault currFault = faultsQueue.Peek();
        //        nFaults++;
        //        if (currFault._faultTime < prevTime)
        //            throw new InvalidProgramException();
        //        injectSystemFaults(currFault._compID, currFault._faultTime, prevTime, revealedSensors);// updates components Age!!! notice to init priorProbs in surviveInModel
        //        //if (currFault._faultTime  >= Tlimit)
        //        //    Console.WriteLine();
        //        //_model.clearObservedValues();
        //        _model.initModel(); // HERE IS THE PRIOR-PROBS INITIATION

        //        removeOldIntervals(currFault._faultTime, compsIntervalsDis, currFault._compID);

        //        List<Interval> IntervalsCopy = new List<Interval>(compsIntervalsDis[currFault._compID]);
        //        //bool checkInterval = checkIntervals(IntervalsCopy);
        //        //if (!checkInterval)
        //        //    Console.WriteLine();
        //        Dictionary<int, Component> testCompCopy = _model.getTestsComponentsCopy(); //
        //        ReapirType currRepairAction;
        //        double cost = fixSystemConsiderTime(revealedSensors, Tlimit, currFault._faultTime, currFault._compID, IntervalsCopy, out currRepairAction);
        //        Component copyComp = new Component(_model._components[currFault._compID]);
        //        if (currRepairAction == ReapirType.FIX)
        //            nFix++;
        //        else
        //            nReplace++;

        //        totalCost += (cost + ExperimentRunner.OVERHEADCOST);
        //        faultsQueue.RemoveMin();

        //        _model.updateComps(testCompCopy); //
        //        _model._components[currFault._compID] = copyComp;
        //        _model.initModel();
        //        if (_worldAfterRepair is DecresingSurival)
        //            ////addNewInterval(currFault._faultTime, currFault._compID, compsIntervalsDis); //add the new one and removes the old one
        //            cutNewIntervals(currFault._faultTime, currFault._compID, compsIntervalsDis);

        //        double newFaultTime = sampleNewCompFault(compsIntervalsDis, currFault._compID, Tlimit, currFault._faultTime);
        //        if (_repairPolicy is Troubleshooting.HealthyReplacementRepairingPolicy)
        //        {

        //        }
        //        if (newFaultTime != -1)
        //        {
        //            faultsQueue.Add(new IntervalFault(currFault._compID, newFaultTime));
        //            //nFaults++;
        //        }
        //        CreatorOverTimeLoger._instance.markIntervalsProbs2(compsIntervalsDis, _model);
        //        prevTime = currFault._faultTime;

        //    }

        //    TroubleshooterLoger._instance.totalCostOfExperiments(totalCost);
        //    MIN_HOP = double.MaxValue; // need tro check that we arent running DFS with a depth longer than the default of the troubleshooter
        //    return totalCost;
        //}

        //-----------------------------------------------------------------------------------------------------------  
        //returns foreach interval(indicated with its upper range) the comps which had sampled as fault
        private Dictionary<int, List<Interval>> cutsIntervals(double Tlimit, double nIntervals)
        {
            List<Interval> dist = new List<Interval>();
            double hop = Tlimit / nIntervals;
            double currTime = 0;
            Interval interval;
            do
            {
                interval = new Interval();
                interval.Lr = currTime;
                currTime += hop;
                if (currTime + EPSILON >= Tlimit)
                    currTime = Tlimit;
                interval.Ur = currTime;
                dist.Add(interval);
            }
            while (currTime < Tlimit);
            interval = new Interval(currTime, -1);
            dist.Add(interval);
            Dictionary<int, List<Interval>> ans = new Dictionary<int, List<Interval>>();
            foreach (var testcopm in _model._testComponents)
            {
                List<Interval> listToCopy = new List<Interval>();
                foreach (var currinterval in dist)
                {
                    Interval intervalToCopy = new Interval(currinterval.Lr, currinterval.Ur);
                    listToCopy.Add(intervalToCopy);
                }
                ans.Add(testcopm, listToCopy);
            }
            return ans;
        }
        //----------------------------------------------------------------------------------------------------------------------
        public void sampleIntervalsFaults(MinHeap<IntervalFault> faultsQueue, Dictionary<int, List<Interval>> compIntervalsDis, double Tlimit,
                                              List<int> componentsToDamage)
        {
            foreach (var testComp in _model._testComponents)
            {
                List<Interval> currDis = compIntervalsDis[testComp];
                //List<Interval> timeDistribution = _worldAfterRepair.updateTimeDistributionVector(this, compIntervalsDis, testComp, Tlimit, 0);
                //compIntervalsDis[testComp] = timeDistribution;
                if (componentsToDamage.Contains(testComp))
                {
                    double faultTime = sampleNewCompFault(compIntervalsDis, testComp, Tlimit, 0);
                    //double faultTime = FixedNewCompFault(compIntervalsDis, testComp, Tlimit, 0);

                    if (faultTime != -1)
                    {
                        IntervalFault intervalFault = new IntervalFault(testComp, faultTime);
                        faultsQueue.Add(intervalFault);
                    }
                }
            }
        }



        public void sampleIntervalsFaults(Dictionary<int, double> faultsQueue, Dictionary<int, List<Interval>> compIntervalsDis, double Tlimit,
                                      List<int> componentsToDamage)
        {
            foreach (var testComp in _model._testComponents)
            {
                List<Interval> currDis = compIntervalsDis[testComp];
                //List<Interval> timeDistribution = _worldAfterRepair.updateTimeDistributionVector(this, compIntervalsDis, testComp, Tlimit, 0);
                //compIntervalsDis[testComp] = timeDistribution;
                if (componentsToDamage.Contains(testComp))
                {
                    double faultTime = sampleNewCompFault(compIntervalsDis, testComp, Tlimit, 0);
                    //double faultTime = FixedNewCompFault(compIntervalsDis, testComp, Tlimit, 0);

                    if (faultTime != -1)
                    {
                        faultsQueue[testComp] = faultTime;
                    }
                }
            }
        }


        //---------------------------------------------------------------------------------------------------------------------- 

        private double sampleNewCompFault(Dictionary<int, List<Interval>> compsIntervalsDis, int compId, double Tlimit, double currTime)
        {
            List<Interval> timeDistribution = _worldAfterRepair.updateTimeDistributionVector(this, compsIntervalsDis, compId, Tlimit, currTime);
          
                //printIntervalList(timeDistribution, currTime);
            Interval newInterval = UsefulFunctions.createSample(timeDistribution);
            double faultTime;
            if (newInterval.Ur != -1)
            {
                faultTime = UsefulFunctions.randFromSpecificRange(newInterval.Lr, newInterval.Ur);
                if (faultTime == Tlimit)
                    Console.WriteLine();  //exeption
            }
            else
                faultTime = -1;

            return faultTime;
        }

        //private double FixedNewCompFault(Dictionary<int, List<Interval>> compsIntervalsDis, int compId, double Tlimit, double currTime)
        //{
        //    List<Interval> timeDistribution = _worldAfterRepair.updateTimeDistributionVector(this, compsIntervalsDis, compId, Tlimit, currTime);
        //    Interval newInterval = null;
        //    if (timeDistribution.Count >= 3)
        //        newInterval = timeDistribution.ElementAt(2);
        //    else if (timeDistribution.Count >= 2)
        //        newInterval = timeDistribution.ElementAt(1);
        //    else
        //        newInterval = timeDistribution.ElementAt(0);

        //    double faultTime;
        //    if (newInterval.Ur != -1)
        //    {
        //        faultTime = (newInterval.Ur - newInterval.Lr) / (double) 2;

        //        //faultTime = UsefulFunctions.randFromSpecificRange(newInterval.Lr, newInterval.Ur);
        //        if (faultTime == Tlimit)
        //            Console.WriteLine();  //exeption
        //    }
        //    else
        //        faultTime = -1;

        //    return faultTime;
        //}
        //----------------------------------------------------------------------------------------------------------------------
        private void injectSystemFaults(int faultComp, double faultTime, double lastElpsTime, Dictionary<int, int> revealedSensors)
        {
            double addTime = faultTime - lastElpsTime;
            _model.sampleControlComponents();
            foreach (var comp in _model._testComponents)
            {
                _model._components[comp]._age += addTime; // updates components Age!!! notice to init priorProbs in surviveInModel
                if (comp == faultComp)
                {
                    double[] eDis = new double[_model._components[comp]._domain - 1];
                    UsefulFunctions.fillEqualDistribution(eDis);
                    int val = UsefulFunctions.createSample(eDis) + 1;
                    _model.updateObservedValue(comp, val);
                }
                else
                    _model.updateObservedValue(comp, 0); //means healthy
            }
            CreatorOverTimeLoger._instance.markFaultInInterval(faultTime, _model);
        }
        //----------------------------------------------------------------------------------------------------------------------

        private void addNewInterval(double oldFaultTime, int compID, Dictionary<int, List<Interval>> compsIntervalsDis)
        {
            List<Interval> compDis = compsIntervalsDis[compID];
            Interval intervalToRemove = null;
            foreach (var currInterval in compDis)
            {
                if (currInterval.contains(oldFaultTime))
                {
                    intervalToRemove = currInterval;
                    break;
                }
            }
            Interval newInterval = new Interval(oldFaultTime, intervalToRemove.Ur);
            compDis.Remove(intervalToRemove); //check if the hash code found this interval in every comp's list

            compDis.Insert(0, newInterval);
        }

        private void cutNewIntervals(double oldFaultTime, int compID, Dictionary<int, List<Interval>> compsIntervalsDis)
        {
            //double minHop = (_Tlimit - 0) / _nIntervals;
            //double minHop = _Tlimit / _nIntervals;
            double Ninterval = ((_Tlimit - oldFaultTime) / _Tlimit) * (_nIntervals);
            double hop = _Tlimit / (double)(Math.Ceiling(Ninterval));
            MIN_HOP = hop;
            List<Interval> dist = new List<Interval>();
            double currTime = oldFaultTime;
            Interval interval;
            do
            {
                interval = new Interval();
                interval.Lr = currTime;
                currTime += hop;
                if (currTime + EPSILON >= _Tlimit)
                    currTime = _Tlimit;
                interval.Ur = currTime;
                dist.Add(interval);
            }
            while (currTime < _Tlimit);
            interval = new Interval(currTime, -1);
            dist.Add(interval);

            List<Interval> listToCopy = new List<Interval>();
            foreach (var currinterval in dist)
            {
                Interval intervalToCopy = new Interval(currinterval.Lr, currinterval.Ur);
                listToCopy.Add(intervalToCopy);
            }
            compsIntervalsDis[compID] = listToCopy;
        }

        //----------------------------------------------------------------------------------------------------------------------

        public bool checkIntervals(List<Interval> IntervalsCopy)
        {
            if (IntervalsCopy.Count <= 1)
                return true;
            double prevUr = IntervalsCopy.First().Ur;
            for (int i = 1; i < IntervalsCopy.Count; i++ )
            {
                Interval intreval = IntervalsCopy.ElementAt(i);
                if (intreval.Lr != prevUr)
                    if (intreval.Ur != -1)
                        return false;
                prevUr = intreval.Ur;
            }
            return true;
        }



        private void removeOldIntervals(double currTime, Dictionary<int, List<Interval>> compsIntervals)
        {
            for (int i = 0; i < compsIntervals.Count; i++ )
            {
                List<Interval> compIntervals = compsIntervals.ElementAt(i).Value;
                List<Interval> intervalsToRemove = new List<Interval>();
                foreach (Interval interval in compIntervals)
                {
                    if (currTime >= interval.Ur && interval.Ur != -1)
                    {
                        intervalsToRemove.Add(interval);
                    }
                }
                foreach (var x in intervalsToRemove)
                    compIntervals.Remove(x);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------
        private void removeOldIntervals(double currTime, Dictionary<int, List<Interval>> compsIntervals, int compID)
        {

            List<Interval> compIntervals = compsIntervals[compID];
            List<Interval> intervalsToRemove = new List<Interval>();
            foreach (Interval interval in compIntervals)
            {
                if (currTime >= interval.Ur && interval.Ur != -1)
                {
                    intervalsToRemove.Add(interval);
                }
            }
            foreach (var x in intervalsToRemove)
                compIntervals.Remove(x);
            
        }
        //----------------------------------------------------------------------------------------------------------------------

        private void printIntervalList(List<Interval> intervals, double currTime)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var x in intervals)
            {
                sb.AppendLine(x.ToString());
            }
            Console.WriteLine(sb.ToString());
        }

        //----------------------------------------------------------------------------------------------------------------------
        private void rearrangeFaultQueue(Dictionary<int, double> faultQ, HashSet<int> healthReplaced, Dictionary<int, List<Interval>> compsIntervalsDis, double Tlimit, double currTime)
        {
            foreach (int x in healthReplaced)
            {
                //(1)delete current fault
                faultQ.Remove(x);
                //(2)sample new fault
                double newFaultTime = sampleNewCompFault(compsIntervalsDis, x, Tlimit, currTime);
                if (newFaultTime != -1)
                    faultQ[x] = newFaultTime;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------

        //-----------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="repairPolicy"></param>
        /// <param name="Tlimit"></param>
        /// <param name="nIntervals"></param>
        /// <param name="revealedSensors"></param>
        /// <param name="nFaults"></param>
        /// <returns></returns>
        public double troubleshootingOverTime(ITroubleShooterRepairingPolicy repairPolicy, double Tlimit, double nIntervals,
                                  Dictionary<int, int> revealedSensors, out int nFaults, out int nFix, out int nReplace)
        {
            _Tlimit = Tlimit;
            nFix = 0; nReplace = 0;
            nFaults = 0;
            _repairPolicy = repairPolicy;
            //TroubleshooterLoger._instance.setLogFileName(_folderName, Tlimit);
            //CreatorOverTimeLoger._instance.setLogFileName(fixPolicy.ToString() + "_" + Tlimit);
            double totalCost = 0;

            _nIntervals = nIntervals;
            Dictionary<int, double> faultsQueue = new Dictionary<int, double>();
            //MinHeap<IntervalFault> faultsQueue = new MinHeap<IntervalFault>();

            Dictionary<int, List<Interval>> compsIntervalsDis = cutsIntervals(Tlimit, nIntervals);
            sampleIntervalsFaults(faultsQueue, compsIntervalsDis, Tlimit, _model._testComponents);
            CreatorOverTimeLoger._instance.markIntervalsProbs2(compsIntervalsDis, _model);
            double prevTime = 0;
            int i = 0;

            nhealthyReplaced = 0;
            while (faultsQueue.Count > 0) //note : new element can add during the loop - therefore using while
            {
                TroubleshooterLoger._instance.writeText("fault Queue: " + faultQtoString(faultsQueue));

                KeyValuePair<int, double> currFault = UsefulFunctions.getMinValueFromDic(faultsQueue);
                int currFaultComp = currFault.Key; double currFaultTime = currFault.Value;
                nFaults++;
                if (currFaultTime < prevTime)
                    throw new InvalidProgramException();
                injectSystemFaults(currFaultComp, currFaultTime, prevTime, revealedSensors);// updates components Age!!! notice to init priorProbs in surviveInModel
                //if (currFault._faultTime  >= Tlimit)
                //    Console.WriteLine();
                //_model.clearObservedValues();
                _model.initModel(); // HERE IS THE PRIOR-PROBS INITIATION

                removeOldIntervals(currFaultTime, compsIntervalsDis, currFaultComp);

                List<Interval> IntervalsCopy = new List<Interval>(compsIntervalsDis[currFaultComp]);
                //bool checkInterval = checkIntervals(IntervalsCopy);
                //if (!checkInterval)
                //    Console.WriteLine();
                Dictionary<int, Component> testCompCopy = _model.getTestsComponentsCopy(); //
                ReapirType currRepairAction;
                double cost = fixSystemConsiderTime(revealedSensors, Tlimit, currFaultTime, currFaultComp, IntervalsCopy, out currRepairAction);
                Component copyComp = new Component(_model._components[currFaultComp]);
                if (currRepairAction == ReapirType.FIX)
                    nFix++;
                else
                    nReplace++;

                totalCost += (cost + ExperimentRunner.OVERHEADCOST);

                faultsQueue.Remove(currFaultComp);

                _model.updateComps(testCompCopy); //
                _model._components[currFaultComp] = copyComp;
                _model.initModel();
                if (_worldAfterRepair is DecresingSurival)
                    ////addNewInterval(currFault._faultTime, currFault._compID, compsIntervalsDis); //add the new one and removes the old one
                    cutNewIntervals(currFaultTime, currFaultComp, compsIntervalsDis);

                double newFaultTime = sampleNewCompFault(compsIntervalsDis, currFaultComp, Tlimit, currFaultTime);
                if (newFaultTime != -1)
                {
                    faultsQueue.Add(currFaultComp, newFaultTime);
                    //nFaults++;
                }

                if (_repairPolicy is Troubleshooting.HealthyReplacementRepairingPolicy)
                {
                    Troubleshooting.HealthyReplacementRepairingPolicy blabla = (Troubleshooting.HealthyReplacementRepairingPolicy)_repairPolicy;
                    foreach (var x in blabla._HealthComponentsReplaced)
                    {
                        repairComponent(x, ReapirType.REPLACE);
                        cutNewIntervals(currFaultTime, x, compsIntervalsDis);
                    }
                    rearrangeFaultQueue(faultsQueue, blabla._HealthComponentsReplaced, compsIntervalsDis, Tlimit, currFaultTime);

                    nhealthyReplaced += blabla._HealthComponentsReplaced.Count;
                }

                CreatorOverTimeLoger._instance.markIntervalsProbs2(compsIntervalsDis, _model);
                prevTime = currFaultTime;

            }

            TroubleshooterLoger._instance.totalCostOfExperiments(totalCost);
            MIN_HOP = double.MaxValue; // need tro check that we arent running DFS with a depth longer than the default of the troubleshooter
            return totalCost;
        }

        //-----------------------------------------------------------------------------------------------------------  

        private string faultQtoString(Dictionary<int, double> faultQ)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in faultQ)
            {
                sb.AppendLine("V_" + pair.Key + "- time = " + pair.Value);

            }
            return sb.ToString();
        }


    }
}
