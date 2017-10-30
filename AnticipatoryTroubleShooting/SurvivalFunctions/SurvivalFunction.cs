using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.SurvivalFunctions
{
    public abstract class SurvivalFunction
    {
        //-----------------------------------------------------------------------------------------------------------
        public abstract double survive(double age);
        //-----------------------------------------------------------------------------------------------------------
        public abstract void setParameter(double param);

        //-----------------------------------------------------------------------------------------------------------
        public double getOldnessRate(double age)
        {
            return (1 - survive(age));
        }
        //-----------------------------------------------------------------------------------------------------------
        //public double survive(double Ur, double elspTime, double currAge)
        //{
        //    //if (currAge > elspTime)
        //    //    throw new InvalidOperationException();
        //    double ans = survive(Ur - elspTime + currAge);
        //    return ans;
        //}
        //-----------------------------------------------------------------------------------------------------------
        //public double intervalFault(double lower, double upper, double age, double elpsTime)
        //{
        //    if (age > lower)
        //    {
        //        if (Math.Abs(age - lower) > 0.001)
        //            throw new InvalidOperationException();
        //    }
        //    double d1 = survive(lower, elpsTime, age);
        //    double d2 = survive(upper, elpsTime, age);
        //    return (d1-d2);
        //}
        //-----------------------------------------------------------------------------------------------------------
        public double FaultBetween(double Ur, double Lr, double currTime)
        {
            //if (currAge > elspTime)
            //    throw new InvalidOperationException();
            double ans = (survive(Lr) - survive(Ur)) / (survive(currTime));
            double delta = Ur - Lr;
            //double ans = (survive(Lr + delta)) / (survive(currTime));

            return ans;
        }
        //-----------------------------------------------------------------------------------------------------------
        public double faultProb(double Ur, double Lr, double currTime)
        {
            double ans = survive(Lr - currTime) - survive(Ur - currTime);
            //double ans = (survive(Lr) - survive(Ur)) / (survive(currTime));
            return ans; 
        }

    }
}
