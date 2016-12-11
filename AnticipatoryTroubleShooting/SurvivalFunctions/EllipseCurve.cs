using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.SurvivalFunctions
{
    class EllipseCurve : SurvivalFunction
    {
        double _a;

        public EllipseCurve(double aFactor)
        {
            _a = aFactor;
        }


        public override double survive(double age)
        {
            if (age > _a)
                throw new InvalidOperationException();
            double ans = Math.Sqrt((_a + age) * (_a - age)) / _a;
            return ans;
        }

        public override void setParameter(double param)
        {
            _a = param;
        }

        public override string ToString()
        {
            return "{ a: " + _a + "}";
        }


    }
}
