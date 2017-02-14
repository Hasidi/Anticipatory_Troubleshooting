using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class Component
    {
        public int _number;
        public int _domain;
        public double _age;
        public double _survivalFactor;
        public double _repairCost;
        public double _replaceCost;
        public int _observedValue;
        public int _preTestObservation;
        public ComponentType _componentType;
        public double[] _distribution;
        public double _faultProb;
        public double _bsProb;
        public double _svProb;
        public double _decisionRatio;
        public double _normailzePorb;

        public Component(int numID)
        {
            _domain = -1; _age = -1; _survivalFactor = -1; _repairCost = -1; _observedValue = -1; _preTestObservation = -1;
            _faultProb = -1; _decisionRatio = -1; _bsProb = -1; _svProb = 0; _normailzePorb = -1; _replaceCost = -1;
        }

        public Component(int number, int domain, ComponentType compType)
        {
            _number = number; _domain = domain; _componentType = compType;
            _age = -1; _survivalFactor = -1; _repairCost = -1; _observedValue = -1; _preTestObservation = -1;
            _decisionRatio = -1; _normailzePorb = -1; _svProb = 0;
            _replaceCost = -1;

        }


        public Component(Component comp)
        {
            _number = comp._number; 
            _domain = comp._domain;
            _age = comp._age;
            _survivalFactor = comp._survivalFactor;
            _repairCost = comp._repairCost;
            _replaceCost = comp._replaceCost;
            _componentType = comp._componentType;
            _observedValue = comp._observedValue;
            _preTestObservation = comp._preTestObservation;
        }

        public void clearObservedValue()
        {
            _observedValue = -1;
            _distribution = new double[_domain];
            _faultProb = -1;
            _svProb = 0;
            _bsProb = -1;
            _decisionRatio = -1;
            _normailzePorb = -1;
        }

        public double getRepairCost(ReapirType fixType)
        {
            if (fixType == ReapirType.FIX)
            {
                return _repairCost;
            }
            if (fixType == ReapirType.REPLACE)
                return _replaceCost;
            if (fixType == ReapirType.NoAction)
                return 0;
            else
                throw new InvalidProgramException();

        }
        public double sumFaultDistribution()
        {
            if (_distribution == null)
                throw new InvalidProgramException("array distribution is empty");
            double ans = 0;
            for (int i = 1; i < _distribution.Length; i++)
                ans += _distribution[i];
            return ans;

        }
        public bool isObserved()
        {
            return (_observedValue >=0);
        }

       
        public void updateObserveValue(int observedVal)
        {
            _preTestObservation = observedVal;
            _observedValue = observedVal;
        }




    }



    public enum ComponentType {TEST, COMMAND, SENSOR};
    public enum ComponentStae {HEALTHY, FAULTY};
    
}
