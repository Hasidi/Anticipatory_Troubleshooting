using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class Interval
    {
        public double Ur;
        public double Lr;
        public double faultProb;

        public Interval() { }
        public Interval(Interval interval)
        {
            Lr = interval.Lr;
            Ur = interval.Ur;
        }
        public Interval(double l, double u)
        {
            Lr = l;
            Ur = u;
        }

        public bool contains(double point)
        {
            return ((point >= Lr && point < Ur) );
        }

        public override string ToString()
        {
            return ("[" + Lr + "," + Ur + "], " + faultProb.ToString("N4"));
        }

        public override int GetHashCode()
        {
            return string.Format("{0}_{1}", Ur, Lr).GetHashCode();
        }

        public double getTimeSlotLength()
        {
            double ans = Ur-Lr;
            if (ans < 0)
                Console.WriteLine();
            return ans;
        }

    }
}
