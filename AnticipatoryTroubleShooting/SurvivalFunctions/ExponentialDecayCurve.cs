using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.SurvivalFunctions
{
    class ExponentialDecayCurve : SurvivalFunction
    {
        double _N0;
        double _lamda;

        public ExponentialDecayCurve(double n0, double lamda)
        {
            _N0 = n0;
            _lamda = lamda;
        }
        public ExponentialDecayCurve(double lamda)
        {
            _N0 = 1;
            _lamda = lamda;
        }
        public override double survive(double age)
        {
            //if (_lamda > ExperimentRunner.SURVIVAL_FACTOR_NEW)
            //    throw new UnauthorizedAccessException();
            double ans = _N0 * Math.Exp(-_lamda * age);
            return ans;
        }

        public override void setParameter(double param)
        {
            _lamda = param;
        }

        public override string ToString()
        {
            return "{"+_N0+", "+_lamda+"}";
        }
    }
}
