using AnticipatoryTroubleShooting.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.Troubleshooting
{
    class DFS_HybridRepairPolicy : ITroubleShooterRepairingPolicy
    {
        TroubleShooter _troubleshooter;
        List<ReapirType> _actions;
        double _intervalLength;
        double _Tlimit;
        double _realityTime;
        double _firstInterval;
        int _compId;
        double _epsilon = 0.01;
        public int _nIntervals;
        public List<double> _probsSeen;
        StringBuilder _sb;
        private string PATH = @"../Debug/Files/Logers/";

        //public DFS_HybridRepairPolicy(TroubleShooter troubleshooter)
        //{
        //    _actions = new List<ReapirType>();
        //    _actions.Add(ReapirType.FIX); _actions.Add(ReapirType.REPLACE);
        //    _troubleshooter = troubleshooter;
        //    _sb = new StringBuilder();
          
        //}
        public DFS_HybridRepairPolicy(TroubleShooter troubleshooter, int nIntervalsLookAhead)
        {
            _actions = new List<ReapirType>();
            _actions.Add(ReapirType.FIX); _actions.Add(ReapirType.REPLACE);
            _troubleshooter = troubleshooter;
            _nIntervals = nIntervalsLookAhead;
            _sb = new StringBuilder();
            PATH += "lk_" + nIntervalsLookAhead +".txt";
        }
        //-----------------------------------------------------------------------------------------------------------
        public ReapirType RepairComponentPolicy(Model model, int compID, double timeLimit, double currTime, out string policyString)
        {
            _probsSeen = new List<double>();
            State.TROUBLESHOOTER = _troubleshooter;
            Component comp = model._components[compID];
            policyString = string.Empty;
            ReapirType repairAns = ReapirType.FIX;

            _compId = compID;
           
            _realityTime = currTime;
            _Tlimit = timeLimit;

            if (_realityTime + _epsilon >= _Tlimit)
            {
                //_troubleshooter.repairComponent(compID, ReapirType.FIX);
                return ReapirType.FIX;
            }
            List<Interval> compIntervals = createIntervals(_realityTime, _Tlimit, compID);
            double cost = double.MaxValue;

            //double v = computeCost(compID, true, _realityTime, true, out repairAns);
            State startState = new State(_realityTime, 0, true, null, _troubleshooter);

            double costAns = computeCost(startState, compIntervals);
            repairAns = startState._repairType;
            //_troubleshooter.repairComponent(compID, repairAns);
            writeText();
            return repairAns;

            //foreach (var repairType in _actions)
            //{
            //    double nextTime = _realityTime + _firstInterval;
            //    double a = Math.Abs(nextTime - _Tlimit);
            //    if (a < 0.01)
            //        nextTime = _Tlimit - _epsilon;
            //    State childState = new State(nextTime, _realityTime, false, startState);
            //    double currCost = _troubleshooter.repairComponent(compID, repairType);
            //    _troubleshooter._model.updateCompsAges(childState._currTime - startState._currTime);
            //    double v = computeCost(childState, compIntervals);
            //    if ((v + currCost) < cost)
            //    {
            //        repairAns = repairType;
            //        cost = v + currCost;
            //    }
            //    _troubleshooter._model.updateComps(startState._comps);
            //}
            ////Console.WriteLine("a");

            //_troubleshooter.repairComponent(compID, repairAns); 
            ////if (repairAns == ReapirType.FIX)
            ////    Console.WriteLine();
            //return repairAns;

        }
        //-----------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "Look-Ahead-Tree_" + _nIntervals;
        }

        //-----------------------------------------------------------------------------------------------------------
        //private double computeCost(int compID, bool decisionNode, bool n0, out ReapirType bestAction, List<Interval> intervalsProb)
        //{
        //    double cost = 0;
        //    bestAction = ReapirType.FIX;
        //    double currTime = intervalsProb.First().Lr;
        //    if (currTime > _Tlimit || currTime <= 0)
        //        Console.WriteLine("time excedded");
        //    if (decisionNode)
        //    {
        //        if (_Tlimit - 0.1 == currTime)
        //            return _troubleshooter._model._components[compID]._repairCost;
        //        cost = double.MaxValue;
        //        double nextTime = intervalsProb.ElementAt(1).Lr;
        //        if (nextTime > _Tlimit)
        //            Console.WriteLine("next time excedded");
        //        Dictionary<int, Component> testCompCopy = _troubleshooter._model.getTestsComponentsCopy();
        //        foreach (var repairType in _actions)
        //        {
        //            _troubleshooter.repairComponent(compID, repairType); //update only the specific repair comp
        //            _troubleshooter._model.updateCompsAges(nextTime - currTime);
        //            intervalsProb.RemoveAt(0);
        //            double currCost = computeCost(compID, false, false, out bestAction, intervalsProb); //
        //            if (currCost < cost)
        //            {
        //                cost = currCost;
        //                bestAction = repairType;
        //            }
        //            _troubleshooter._model.updateComps(testCompCopy);
        //        }
        //    }
        //    else
        //    {
        //        if (currTime == _Tlimit)
        //            return cost;
        //        if (currTime > _Tlimit)
        //            Console.WriteLine("next time excedded");
        //        Dictionary<int, Component> testCompCopy = _troubleshooter._model.getTestsComponentsCopy();
        //        double upInterval = intervalsProb.First().Ur;
        //        double faultProb = anticipateFault(compID, currTime, 
        //        double cost1 = computeCost(compID, true, currTime, false, out bestAction); // fault branch
        //        double nextTime;
        //        if (currTime + 0.1 >= _Tlimit)
        //            nextTime = _Tlimit;
        //        else
        //            nextTime = currTime + _intervalLength;
        //        if (nextTime > _Tlimit)
        //            Console.WriteLine("next time excedded");
        //        _troubleshooter._model.updateComps(testCompCopy);
        //        _troubleshooter._model.updateCompsAges(nextTime - currTime);
        //        double cost2 = computeCost(compID, false, nextTime, false, out bestAction);  //
        //        cost = cost1 * faultProb + cost2 * (1 - faultProb);                
        //    }
        //    return cost;
        //}
        //-----------------------------------------------------------------------------------------------------------

        //private void clearOldIntervals(List<Interval> compIntervals)
        //{
        //    List<int> toRemove = new List<int>();
        //    int c = 0;
        //    foreach (var x in compIntervals)
        //    {
        //        if (_realityTime >= x.Ur && x.Ur != -1)
        //        {
        //            toRemove.Add(c);
        //        }
        //        c++;
        //    }
        //    foreach (var x in toRemove)
        //        compIntervals.RemoveAt(x);
        //}


        //-----------------------------------------------------------------------------------------------------------
        //private double anticipateFault( int compID)
        //{
        //    _troubleshooter._model.initModel();
        //    //if (!_troubleshooter._model.verifyModel())
        //    //    Console.WriteLine();
        //    //Console.WriteLine("aaa");
        //    _troubleshooter._model.getComponentDistribution(compID);
        //    double faultProb = _troubleshooter._model.getFaultProb(compID);
        //    //Console.WriteLine("bbb");

        //    return faultProb;
        //}
        //private double anticipateFault(int compID, double currTime, double prevTime)
        //{
        //    _troubleshooter._model.initModel();
        //    SurvivalBayesModel svModel = (SurvivalBayesModel)_troubleshooter._model;
        //    Component comp = _troubleshooter._model._components[compID];
        //    double prevAge = comp._age - (currTime - prevTime);
        //    //if (prevAge != 0)
        //    //    throw new InvalidProgramException();
        //    double faultProb = 1 - svModel._survivalCurves[compID].survive(currTime, prevTime, prevAge);
        //    //Console.WriteLine("bbb");

        //    return faultProb;
        //}

            //-----+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private double anticipateFault(State state, int compID)
        {

            //_troubleshooter._model.initModel();
            //SurvivalBayesModel svModel = (SurvivalBayesModel)_troubleshooter._model;
            //Component comp = _troubleshooter._model._components[compID];
            //double lower = state._prevTime;
            //double up = state._currTime;
            //double prevAge;
            //if (state._parent._decisionNode)
            //    prevAge = 0;
            //else
            //    prevAge = state._parent._comps[compID]._age;
            //double elpsT = state._prevTime;

            //double faultProb = svModel._survivalCurves[compID].intervalFault(lower, up, prevAge, elpsT);
            //Console.WriteLine("bbb");

            _troubleshooter._model.initModel();
            SurvivalBayesModel svModel = (SurvivalBayesModel)_troubleshooter._model;
            Component comp = _troubleshooter._model._components[compID];
            double lower = state._prevTime;
            double up = state._currTime;
            double prevAge;
            if (state._parent._decisionNode)
                prevAge = 0;
            else
                prevAge = state._parent._comps[compID]._age;
            double currAge = state._comps[_compId]._age;

            double faultProb = (svModel._survivalCurves[compID].survive(prevAge) - svModel._survivalCurves[compID].survive(currAge))
                / svModel._survivalCurves[compID].survive(prevAge);

            if (! _probsSeen.Contains(faultProb))
                _probsSeen.Add(faultProb);
            return faultProb;
        }
        ////////////-----------------------------------------------------------------------------------------------------------

        //////////private double computeCost(State currState, List<Interval> intervalsProb)
        //////////{
        //////////    if (intervalsProb.Count == 0)
        //////////        return 0;
        //////////    double nextTime = intervalsProb.First().Ur;
        //////////    if (currState.isDecisionNode())
        //////////    {
        //////////        if (currState._currTime == _Tlimit)
        //////////        {
        //////////            currState._cost = _troubleshooter.repairComponent(_compId, ReapirType.FIX);
        //////////            return currState._cost;
        //////////        }
        //////////        foreach (var repairAction in _actions)
        //////////        {
        //////////            //State newState = new State(nextTime, false, currState);
        //////////            State childState = new State(nextTime, currState._currTime, false, currState);
        //////////            double repairCost = _troubleshooter.repairComponent(_compId, repairAction); //update only the specific repair comp
        //////////            _troubleshooter._model.updateCompsAges(childState._currTime - currState._currTime);
        //////////            List<Interval> intervalsCopy = new List<Interval>(intervalsProb);
        //////////            intervalsProb.RemoveAt(0);
        //////////            double childCost = computeCost(childState, intervalsProb); //
        //////////            double currCost = childCost + repairCost;
        //////////            if (currCost <= currState._cost)
        //////////            {
        //////////                currState._cost = currCost;
        //////////                currState._repairType = repairAction;
        //////////            }
        //////////            _troubleshooter._model.updateComps(currState._comps);
        //////////            intervalsProb = intervalsCopy;
        //////////        }
        //////////    }
        //////////    else
        //////////    {
        //////////        if (currState._prevTime == _Tlimit)
        //////////            return 0;
        //////////        double faultProb = anticipateFault(_compId, currState._currTime, currState._prevTime);

        //////////        State faultChildState = new State(currState._currTime, currState._prevTime, true, currState);
        //////////        double faultChildCost = computeCost(faultChildState, intervalsProb); //
        //////////        if (nextTime == _Tlimit)
        //////////            nextTime -= _epsilon;
        //////////        _troubleshooter._model.updateComps(currState._comps);
        //////////        State healthyChildState = new State(nextTime, currState._currTime, false, currState);
        //////////        _troubleshooter._model.updateCompsAges(healthyChildState._currTime - currState._currTime);
        //////////        //intervalsProb.RemoveAt(0);
        //////////        double healthyChildCost = computeCost(healthyChildState, intervalsProb); //
        //////////        currState._cost = faultChildCost * faultProb + (1 - faultProb) * healthyChildCost;
        //////////    }
        //////////    return currState._cost;

        //////////}










        //-----------------------------------------------------------------------------------------------------------

        //private double computeCost()
        //{
        //    Queue<State> states = new Queue<State>();
        //    State startState = new State(_realityTime, true, _troubleshooter);
        //    startState._nextTime = startState._currTime + _firstInterval;
        //    states.Enqueue(startState);
        //    while (states.Count > 0)
        //    {
        //        State currState = states.Dequeue();
        //        double nextStateNextTime = currState._nextTime + _intervalLength;
        //        if (currState.isDecisionNode())
        //        {

        //            foreach (var repairAction in _actions)
        //            {
        //                double repairCost = _troubleshooter.repairComponent(_compId, repairAction); //update only the specific repair comp
        //                if (nextStateNextTime == _Tlimit)
        //                    nextStateNextTime = _Tlimit - _epsilon;
        //                _troubleshooter._model.updateCompsAges(currState._nextTime - currState._currTime);
        //                State newState = new State(currState._nextTime, false, nextStateNextTime, currState, _troubleshooter);
        //                newState
        //                states.Enqueue(newState);
        //            }

        //        }
        //        else
        //        {
        //            double faultProb = anticipateFault(_compId);
        //            if (nextStateNextTime > _Tlimit)
        //            {
        //                currState._cost += faultProb * 
        //            }
        //            State faultNodeChild = new State(currState._currTime, true, )
        //        }
        //    }
        //}



        //-----------------------------------------------------------------------------------------------------------
        //private double rec_fixComponent(int compID, double repairCost, List<Interval> compIntervals, bool decisionNode,
        //    out ReapirType bestAction, double currTime)
        //{
        //    Console.Clear();

        //    double cost = -1;
        //    bestAction = 0;
        //    if (compIntervals.Count == 0)
        //    {
        //        return repairCost;
        //    }
        //    if (compIntervals.Count == 1 && decisionNode)
        //    {
        //        return repairCost + _troubleshooter._model._components[compID]._repairCost;
        //    }
        //    if (decisionNode)
        //    {
        //        double minCost = Double.MaxValue;
        //        foreach (var repairType in _actions)
        //        {
        //            //Dictionary<int, Component> componentsStates = _troubleshooter._model.getTestsComponents();
        //            //List<Interval> currIntervalsCopy = new List<Interval>(compIntervals);
        //            Dictionary<int, Component> copyComps; List<Interval> copyIntervals;
        //            double timeSlotLength = compIntervals.First().Ur - currTime;
        //            if (timeSlotLength < 0)
        //                timeSlotLength = 0;

        //            removeIntervalAndUpdateAges(compIntervals, _troubleshooter._model._components, out copyIntervals, out copyComps, currTime);
        //            double currCost = _troubleshooter.repairComponent(compID, repairType); //update only the specific repair comp
        //            //repairCost += currCost; //
        //            currTime = compIntervals.First().Lr;
        //            if (compIntervals.Count == 1)
        //            {
        //                currTime = compIntervals.First().Lr - 0.1;
        //                timeSlotLength -= 0.1;
        //                //currCost = _troubleshooter.repairComponent(compID, ReapirType.FIX);
        //                //return repairCost + currCost;
        //            }
        //            _troubleshooter._model.updateCompsAges(timeSlotLength);

        //            currCost = rec_fixComponent(compID, repairCost + currCost, compIntervals, false, out bestAction, currTime); //need to remove one interval
        //            if (currCost < minCost)
        //            {
        //                minCost = currCost;
        //                cost = minCost;
        //                bestAction = repairType;
        //            }
        //            compIntervals = copyIntervals;
        //            _troubleshooter._model.updateComps(copyComps);
        //            currTime = compIntervals.First().Lr;
        //        }
        //    }
        //    else
        //    {
        //        double faultProb = anticipateFault(compID);
        //        List<Interval> currIntervalsCopy = new List<Interval>(compIntervals);
        //        double cost1 = rec_fixComponent(compID, repairCost, compIntervals, true, out bestAction, currTime);
        //        //Dictionary<int, Component> copyComps; List<Interval> copyIntervals;
        //        //removeIntervalAndUpdateAges(compIntervals, _troubleshooter._model._components, out copyIntervals, out copyComps,  currTime);

        //        compIntervals = currIntervalsCopy;
        //        double timeSlotLength = compIntervals.First().Ur - currTime;
        //        if (compIntervals.Count > 0)
        //        {
        //            currTime = compIntervals.First().Ur;
        //            compIntervals.RemoveAt(0);
        //            if (compIntervals.Count == 1)
        //            {
        //                currTime = compIntervals.First().Lr - 0.1;
        //                timeSlotLength -= 0.1;
        //            }
        //            _troubleshooter._model.updateCompsAges(timeSlotLength); //
        //        }
        //        double cost2 = rec_fixComponent(compID, repairCost, compIntervals, false, out bestAction, currTime);  //need to remove one interval
        //        cost = cost1 * faultProb + cost2 * (1 - faultProb);
        //    }
        //    return cost;
        //}

        //-----------------------------------------------------------------------------------------------------------
        //private double removeIntervalAndUpdateAges(List<Interval> compIntervals, Dictionary<int, Component> comps,
        //        out List<Interval> copyIntervals, out Dictionary<int, Component> testCompCopy, double currTime)
        //{
        //    copyIntervals = new List<Interval>(compIntervals); // check if it this deep copy
        //    testCompCopy = _troubleshooter._model.getTestsComponentsCopy();
        //    Interval interval = compIntervals.First();
        //    double timeSlotLength = interval.getTimeSlotLength();
        //    if (interval.Lr < currTime)
        //        timeSlotLength = interval.Ur - currTime;
        //    if (compIntervals.Count > 0)
        //        compIntervals.RemoveAt(0);
        //    return timeSlotLength;
        //}
        //-----------------------------------------------------------------------------------------------------------

        private List<Interval> createIntervals(double elpsTime, double Tlimit, int compId)
        {
            List<Interval> dist = new List<Interval>();
            //double MIN_HOP = Tlimit / ExperimentRunner.N_INTERVALS;
            double hop = (Tlimit -elpsTime) / _nIntervals;
            if (hop < TroubleShooter.MIN_HOP)
            {
                //throw new InvalidProgramException();
                hop = TroubleShooter.MIN_HOP;

            }
            double currTime = elpsTime;
            Interval interval;
            do
            {
                interval = new Interval();
                interval.Lr = currTime;
                currTime += hop;
                if (currTime + _epsilon >= Tlimit)
                    currTime = Tlimit;
                interval.Ur = currTime;
                dist.Add(interval);
            }
            while (currTime < Tlimit);
            interval = new Interval(currTime, -1);
            dist.Add(interval);
            SurvivalBayesModel model = (SurvivalBayesModel)_troubleshooter._model;
            double Cage = model._components[compId]._age;

            foreach (var intervalProb in dist)
            {
                double currProb = model._survivalCurves[compId].intervalFault(intervalProb.Lr, intervalProb.Ur, Cage, elpsTime);

            }
            return dist;
        }









        //-----------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------
        //private double computeCost(State currState, List<Interval> intervalsProb)
        //{
        //    if (intervalsProb.Count == 0)
        //        return 0;
        //    double nextTime = intervalsProb.First().Ur;
        //    if (currState.isDecisionNode())
        //    {
        //        if (currState._currTime == _Tlimit)
        //        {
        //            currState._cost = _troubleshooter.repairComponent(_compId, ReapirType.FIX);
        //            return currState._cost;
        //        }
        //        foreach (var repairAction in _actions)
        //        {
        //            double repairCost = _troubleshooter.repairComponent(_compId, repairAction); //update only the specific repair comp
        //            _troubleshooter._model.updateCompsAges(nextTime - currState._currTime);

        //            State childState = new State(nextTime, currState._currTime, false, currState, _troubleshooter);

        //            //childState._cost = _troubleshooter._model._components[_compId].getRepairCost(repairAction);

        //            List<Interval> intervalsCopy = new List<Interval>(intervalsProb);
        //            intervalsProb.RemoveAt(0);

        //            double childCost = computeCost(childState, intervalsProb); //
        //            double currCost = childCost + repairCost;
        //            //double currCost = childCost;
        //            if (currCost <= currState._cost)
        //            {
        //                currState._repairType = repairAction;
        //                currState._cost = currCost;

        //            }
        //            _troubleshooter._model.updateComps(currState._comps);
        //            intervalsProb = intervalsCopy;
        //        }
        //        //currState._cost = _troubleshooter._model._components[_compId].getRepairCost(currState._repairType);

        //    }
        //    else
        //    {
        //        if (currState._prevTime == _Tlimit)
        //            return 0;
        //        double faultProb = anticipateFault(_compId, currState._currTime, currState._prevTime);

        //        State faultChildState = new State(currState._currTime, currState._prevTime, true, currState, _troubleshooter);
        //        double faultChildCost = computeCost(faultChildState, intervalsProb); //
        //        //if (nextTime == _Tlimit)
        //        //    nextTime -= _epsilon;
        //        _troubleshooter._model.updateComps(currState._comps);

        //        _troubleshooter._model.updateCompsAges(nextTime - currState._currTime);
        //        State healthyChildState = new State(nextTime, currState._currTime, false, currState, _troubleshooter);
        //        //intervalsProb.RemoveAt(0);
        //        double healthyChildCost = computeCost(healthyChildState, intervalsProb); //
        //        currState._cost = faultChildCost * faultProb + (1 - faultProb) * healthyChildCost;

        //        //_troubleshooter._model.updateComps(currState._comps);

        //    }
        //    return currState._cost;

        //}


        //-----------------------------------------------------------------------------------------------------------


        private double computeCost(State currState, List<Interval> intervalsProb)
        {
            if (intervalsProb.Count == 0)
                return 0;
            double nextTime = intervalsProb.First().Ur;
            _sb.AppendLine(currState.myToString(_compId));
            if (currState.isDecisionNode())
            {
                if (currState._currTime == _Tlimit)
                {
                    currState._cost = _troubleshooter.repairComponent(_compId, ReapirType.FIX);
                    //return currState._cost;
                }
                else
                {
                    foreach (var repairAction in _actions)
                    {
                        double repairCost = _troubleshooter.repairComponent(_compId, repairAction); //update only the specific repair comp
                        _troubleshooter._model.updateCompsAges(nextTime - currState._currTime);

                        State childState = new State(nextTime, currState._currTime, false, currState, _troubleshooter);

                        //childState._cost = _troubleshooter._model._components[_compId].getRepairCost(repairAction);

                        List<Interval> intervalsCopy = new List<Interval>(intervalsProb);
                        intervalsProb.RemoveAt(0);

                        double childCost = computeCost(childState, intervalsProb); //
                        double currCost = childCost + repairCost;
                        currState._costs.Add(currCost);
                        //double currCost = childCost;
                        if (currCost <= currState._cost)
                        {
                            currState._repairType = repairAction;
                            currState._cost = currCost;

                        }
                        _troubleshooter._model.updateComps(currState._comps);
                        intervalsProb = intervalsCopy;
                    }
                }
                //currState._cost = _troubleshooter._model._components[_compId].getRepairCost(currState._repairType);
                _sb.AppendLine("choose to- " + currState._repairType.ToString());
            }
            else
            {
                //if (currState._currTime > 13.38 && currState._currTime < 13.39)
                //    Console.WriteLine();
                //if (currState._currTime> 16.69 && currState._currTime < 16.694)
                //    Console.WriteLine();
                if (currState._prevTime == _Tlimit)
                {
                    _sb.AppendLine("return 0$");
                    return 0;
                }
                //int x;
                //if (!currState._parent._decisionNode && currState._currTime == _Tlimit)
                //    x = 7;
                double faultProb = anticipateFault(currState, _compId);
                _sb.AppendLine("faultProb: " + faultProb);
                State faultChildState = new State(currState._currTime, currState._prevTime, true, currState, _troubleshooter);

                List<Interval> intervalsCopy = new List<Interval>(intervalsProb);

                double faultChildCost = computeCost(faultChildState, intervalsProb); //
                //if (nextTime == _Tlimit)
                //    nextTime -= _epsilon;
                _troubleshooter._model.updateComps(currState._comps);


                //if (currState._currTime > 13.38 && currState._currTime < 13.39)
                //    Console.WriteLine();

                _troubleshooter._model.updateCompsAges(nextTime - currState._currTime);
                State healthyChildState = new State(nextTime, currState._currTime, false, currState, _troubleshooter);
                //intervalsProb.RemoveAt(0);
                _sb.AppendLine("HealthProb: " + (1 - faultProb));
                intervalsProb = intervalsCopy;
                intervalsProb.RemoveAt(0);

                double healthyChildCost = computeCost(healthyChildState, intervalsProb); //

                currState._cost = faultChildCost * faultProb + (1 - faultProb) * healthyChildCost;

                //_troubleshooter._model.updateComps(currState._comps);

            }
            _sb.AppendLine("return " + currState._cost.ToString() + " $");

            return currState._cost;

        }
        //-----------------------------------------------------------------------------------------------------------

        public void writeText()
        {
            _sb.AppendLine("---------------------------");
            File.AppendAllText(PATH, _sb.ToString());
            _sb = new StringBuilder();
        }
    }






}
