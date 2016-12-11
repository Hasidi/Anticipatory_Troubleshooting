using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.Troubleshooting
{
    class TroubleshooterFaultInjector
    {
        ExperimentRunner _experimentRunner;
        TroubleShooter _troubleshooter;






        //----------------------------------------------------------------------------------------------------------------------
        //public double troubleshootingOverTime(ITroubleShooterFixingPolicy repairPolicy, double Tlimit, int nIntervals,
        //                  Dictionary<int, int> revealedSensors, out int nFaults)
        //{
        //    _nFaultsDuringTime = 0;
        //    _troubleshooter._repairPolicy = repairPolicy;
        //    //TroubleshooterLoger._instance.setLogFileName(_folderName, Tlimit);
        //    //CreatorOverTimeLoger._instance.setLogFileName(fixPolicy.ToString() + "_" + Tlimit);
        //    double totalCost = 0;
        //    MinHeap<IntervalFault> faultsQueue = new MinHeap<IntervalFault>();
        //    Dictionary<int, List<Interval>> compsIntervalsDis = cutsIntervals(Tlimit, nIntervals);
        //    sampleIntervalsFaults(faultsQueue, compsIntervalsDis, Tlimit, _troubleshooter._model._testComponents);
        //    CreatorOverTimeLoger._instance.markIntervalsProbs2(compsIntervalsDis, _troubleshooter._model);
        //    double prevTime = 0;
        //    while (faultsQueue.Count > 0) //note : new element can add during the loop - therefore using while
        //    {
        //        IntervalFault currFault = faultsQueue.Peek();
        //        if (currFault._faultTime < prevTime)
        //            throw new InvalidProgramException();
        //        injectSystemFaults(currFault._compID, currFault._faultTime, prevTime, revealedSensors);// updates components Age!!! notice to init priorProbs in surviveInModel
        //        _troubleshooter._model.clearObservedValues();
        //        _troubleshooter._model.initModel(); // HERE IS THE PRIOR-PROBS INITIATION
        //        double cost = _troubleshooter.fixSystemConsiderTime(revealedSensors, Tlimit, currFault._faultTime, currFault._compID);
        //        totalCost += cost;
        //        faultsQueue.RemoveMin();
        //        if (_worldAfterRepair is DecresingSurival)
        //            addNewInterval(currFault._faultTime, currFault._compID, compsIntervalsDis); //add the new one and removes the old one

        //        double newFaultTime = sampleNewCompFault(compsIntervalsDis, currFault._compID, Tlimit, currFault._faultTime);
        //        if (newFaultTime != -1)
        //            faultsQueue.Add(new IntervalFault(currFault._compID, newFaultTime));
        //        CreatorOverTimeLoger._instance.markIntervalsProbs2(compsIntervalsDis, _troubleshooter._model);
        //        //_nFaultsDuringTime++;
        //        prevTime = currFault._faultTime;
        //    }

        //    TroubleshooterLoger._instance.totalCostOfExperiments(totalCost);
        //    AddCSVLine(_troubleshooter._repairPolicy, FIX_RATIO, Tlimit, _nFaultsDuringTime, totalCost);
        //    nFaults = _nFaultsDuringTime;
        //    return totalCost;
        //}
        //----------------------------------------------------------------------------------------------------------------------







    }
}
