using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.SurvivalFunctions
{
    class WeibullCurve : SurvivalFunction
    {
        double _beta;
        double _lamda;

        public WeibullCurve(double beta, double lamda)
        {
            _beta = beta;
            _lamda = lamda;
        }
        public WeibullCurve(double lamda)
        {
            _beta = 2;
            _lamda = lamda;
        }
        public override double survive(double age)
        {

            double ans = Math.Exp(-(Math.Pow(_lamda * age, _beta)));
            return ans;
        }

        //public override void setParameter(double lamda, double beta)
        //{
        //    _lamda = lamda;
        //    _beta = beta;
        //}
        public override void setParameter(double lamda)
        {
            _lamda = lamda;
        }

        public override string ToString()
        {
            return "{" + _beta + ", " + _lamda + "}";
        }
    }
}
