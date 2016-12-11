using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class State
    {

        public static TroubleShooter TROUBLESHOOTER;
        public double _currTime;
        public ReapirType _repairType;
        public Dictionary<int, Component> _comps;
        public bool _decisionNode;
        //public double _nextTime;
        public double _prevTime;
        public double _cost;
        public State _parent;
        double _upFaultProbBranch; // only to decision node, it penetrate it to the chance node

        //public State(double time, bool decision)
        //{
        //    _currTime = time;
        //    _decisionNode = decision;
        //    _comps = TROUBLESHOOTER._model.getTestsComponentsCopy();
        //    _cost = double.MaxValue;

        //}
        //public State(double time, bool decision, State parent)
        //{
        //    _currTime = time;
        //    _decisionNode = decision;
        //    _parent = parent;
        //    parent._comps = TROUBLESHOOTER._model.getTestsComponentsCopy();
        //    _cost = double.MaxValue;
        //}
        public State(double currTime, double prevTime, bool decision, State parent, TroubleShooter troubleshooter)
        {
            _currTime = currTime;
            _decisionNode = decision;
            _parent = parent;
            _comps = troubleshooter._model.getTestsComponentsCopy();
            _comps = new Dictionary<int, Component>();
            foreach (var compNum in troubleshooter._model._testComponents)
            {
                Component testComponent = troubleshooter._model._components[compNum];
                Component newComp = new Component(testComponent);
                _comps.Add(newComp._number, newComp);

            }

            _cost = double.MaxValue;
            _prevTime = prevTime;
        }


        public bool isDecisionNode()
        {
            return _decisionNode;
        }

        public override string ToString()
        {
            string type = "";
            if (_decisionNode)
                type = "Decision";
            else
                type = "Chance";
            return _currTime + "_" + type;
        }

        public string myToString(int compId)
        {
            string type = "";
            if (_decisionNode)
                type = "Decision";
            else
                type = "Chance";
            Component comp = _comps[compId];
            string details = "Time: " + _currTime + ", curv: " + comp._survivalFactor + ", age: " + comp._age;
            return details + "  " + type;
        }

    }
}
