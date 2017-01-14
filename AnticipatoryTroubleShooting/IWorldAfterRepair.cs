using AnticipatoryTroubleShooting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    interface IWorldAfterRepair
    {
        List<Interval> updateTimeDistributionVector(TroubleShooter troubleshooter, Dictionary<int, List<Interval>> compsIntervalsDis, int compId, double Tlimit, double currTime);


    }


    class DecresingSurival : IWorldAfterRepair
    {
        public List<Interval> updateTimeDistributionVector(TroubleShooter troubleshooter, Dictionary<int, List<Interval>> compsIntervalsDis, int compId, double Tlimit, double currTime)
        {
            List<Interval> intervals = compsIntervalsDis[compId];
            List<Interval> elementToRemove = new List<Interval>();
            foreach (var interval in intervals)
            {
                if (currTime >= interval.Ur && interval.Ur != -1)
                {
                    elementToRemove.Add(interval);
                }
            }
            foreach (var x in elementToRemove)
            {
                intervals.Remove(x);
            }
            if (intervals.Count <=1)
                Console.WriteLine();
            SurvivalBayesModel model = (SurvivalBayesModel)troubleshooter._model;
            List<Interval> timeDistribution = new List<Interval>();
            //List<Interval> intervals = compsIntervalsDis[compId];

            double elpsTime = intervals.First().Lr;
            double Cage = model._components[compId]._age;
            double totalProbForChecking = 0;
            foreach (var currInterval in intervals)
            {
                if (currInterval.Ur == -1)
                    continue;

                //double currProb = model._survivalCurves[compId].intervalFault(currInterval.Lr, currInterval.Ur, Cage, elpsTime);
                //double currProb = model._survivalCurves[compId].FaultBetween(currInterval.Ur, currInterval.Lr, elpsTime);
                double currProb = model._survivalCurves[compId].faultProb(currInterval.Ur, currInterval.Lr, elpsTime);

                currInterval.faultProb = currProb;
                timeDistribution.Add(currInterval);
                totalProbForChecking += currProb;
            }
            //double surviveWholeTime = model._survivalCurves[compId].survive(Tlimit, elpsTime, Cage);

            double surviveWholeTime = 1 - totalProbForChecking;

            totalProbForChecking += surviveWholeTime;
            if (Math.Abs(1 - totalProbForChecking) > 0.001) 
            {
                throw new InvalidProgramException();
            }
            Interval lastInterval = intervals.Last();
            lastInterval.faultProb = surviveWholeTime;
            timeDistribution.Add(lastInterval);
            //if (totalProbForChecking < 1)
            //    UsefulFunctions.normalize(timeDistribution);


            return timeDistribution;
        }


    }
    //-----------------------------------------------------------------------------------------------------------
    //class ConstSurvial : IWorldAfterRepair
    //{
    //    public List<Interval> updateTimeDistributionVector(TroubleShooter troubleshooter, Dictionary<int, List<Interval>> compsIntervalsDis, int compId, double Tlimit, double currTime)
    //    {
    //        //SurvivalBayesModel model = (SurvivalBayesModel)troubleshooter._model;
    //        //List<Interval> intervals = compsIntervalsDis[compId];

    //        //double elpsTime = intervals.First().Lr;
    //        ////double Cage = model._components[compId]._age;
    //        //double totalProbForChecking = 0;

            
    //        //for (int i = 1; i < intervals.Count; i++ )
    //        //{
    //        //    Interval currInterval = intervals[i];
    //        //    double currProb = model._survivalCurves[compId].intervalFault(currInterval.Lr, currInterval.Ur, 0, elpsTime);
    //        //    currInterval.faultProb = currProb;
    //        //    totalProbForChecking += currInterval.faultProb;
    //        //}
    //        //double newFirstProb = 1 - totalProbForChecking;

    //        //intervals[0].faultProb = newFirstProb;

    //        //return intervals;
    //        List<Interval> intervals = compsIntervalsDis[compId];
    //        List<Interval> elementToRemove = new List<Interval>();
    //        foreach (var interval in intervals)
    //        {
    //            if (currTime > interval.Lr && interval.Ur != -1)
    //            {
    //                elementToRemove.Add(interval);
    //            }
    //        }
    //        foreach (var x in elementToRemove)
    //        {
    //            intervals.Remove(x);
    //        }
    //        if (intervals.Count == 1)
    //        {
    //            //means no fault anymore
    //            intervals[0].faultProb = 1; //its not a mistake (logic it need to be equals to 0 but after that with the smpaling method will know to sample this interval with probabilty 1
    //            return intervals;
    //        }
    //        SurvivalBayesModel model = (SurvivalBayesModel)troubleshooter._model;
    //        List<Interval> timeDistribution = new List<Interval>();
    //        //List<Interval> intervals = compsIntervalsDis[compId];

    //        double elpsTime = intervals.First().Lr;
    //        double Cage = model._components[compId]._age;
    //        double totalProbForChecking = 0;
    //        foreach (var currInterval in intervals)
    //        {
    //            if (currInterval.Ur == -1)
    //                continue;
    //            if (Cage > currInterval.Lr)  //there was repair, need to break
    //                break;
    //            double currProb = model._survivalCurves[compId].intervalFault(currInterval.Lr, currInterval.Ur, Cage, elpsTime);
    //            currInterval.faultProb = currProb;
    //            timeDistribution.Add(currInterval);
    //            totalProbForChecking += currProb;
    //        }
    //        double surviveWholeTime = model._survivalCurves[compId].survive(Tlimit, elpsTime, Cage);
    //        totalProbForChecking += surviveWholeTime;
    //        Interval lastInterval;
    //        if ((1 - totalProbForChecking) > 0.001)
    //        {
    //            totalProbForChecking = 0;
    //            //timeDistribution = new List<Interval>();
    //            for (int i = 1; i < intervals.Count - 1; i++)
    //            {
    //                Interval currInterval = intervals[i];
    //                double currProb = model._survivalCurves[compId].intervalFault(currInterval.Lr, currInterval.Ur, 0, 0);
    //                currInterval.faultProb = currProb;
    //                //timeDistribution.Add(currInterval);
    //                totalProbForChecking += currProb;
    //            }
    //            lastInterval = intervals.Last();
    //            lastInterval.faultProb = model._survivalCurves[compId].survive(Tlimit);
    //            totalProbForChecking += lastInterval.faultProb;
    //            Interval firstInterval = intervals[0];
    //            firstInterval.faultProb = 1 - totalProbForChecking;
    //            return intervals;

    //        }
    //        lastInterval = intervals.Last();
    //        lastInterval.faultProb = surviveWholeTime;
    //        timeDistribution.Add(lastInterval);
    //        //if (totalProbForChecking < 1)
    //        //    UsefulFunctions.normalize(timeDistribution);


    //        return timeDistribution;
    //    }
    //}







}
