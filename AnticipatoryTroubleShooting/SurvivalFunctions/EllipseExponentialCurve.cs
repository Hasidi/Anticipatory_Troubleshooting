using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.SurvivalFunctions
{
    class EllipseExponentialCurve : SurvivalFunction
    {
        EllipseCurve _ellipseCurve;
        ExponentialDecayCurve _expCurve;
        double _criticAge;


        public EllipseExponentialCurve(double aFactor, double lamda, double criticAge)
        {
            _ellipseCurve = new EllipseCurve(aFactor);
            _expCurve = new ExponentialDecayCurve(lamda);
            _criticAge = criticAge;
        }
        public override double survive(double age)
        {
            if (age < _criticAge)
                return _ellipseCurve.survive(age);
            return _expCurve.survive(age);

        }

        public override void setParameter(double param)
        {
        }


    }
}
