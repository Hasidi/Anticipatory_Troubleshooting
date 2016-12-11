using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class IntervalFault : IComparable<IntervalFault>
    {
        public int _compID;
        public double _faultTime;

        public IntervalFault()
        {

        }
        public IntervalFault(int compID, double faultTime)
        {
            _compID = compID;
            _faultTime = faultTime;
        }
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            IntervalFault otherIntervalFault = obj as IntervalFault;
            if (otherIntervalFault != null)
                return this._faultTime.CompareTo(otherIntervalFault._faultTime);
            else
                throw new ArgumentException("Object is not a Temperature");
        }
        public int CompareTo(IntervalFault obj)
        {
            IntervalFault otherIntervalFault = obj as IntervalFault;
            if (otherIntervalFault != null)
                return this._faultTime.CompareTo(otherIntervalFault._faultTime);
            else
                throw new ArgumentException("Object is not a Temperature");
        }

        public override string ToString()
        {
            return "[comp." + _compID + ", " + _faultTime + "]";
        }

    }
}
