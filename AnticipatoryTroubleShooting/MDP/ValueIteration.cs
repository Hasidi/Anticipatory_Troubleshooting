using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.MDP
{
    class ValueIteration
    {
        MDPmodel _mdpModel;
        int _maxIterations;
        double _maxDelta; //targer value of differnce between 2 iterations

        public void solve()
        {
            initUtilities();
            int iteration = 0;
            double delta = double.MaxValue; 
            while (iteration < _maxIterations || delta > _maxDelta)
            {
                
            }

        }

        public void initUtilities()
        {
            
        }

        //private double calculateNextStatesValues()
        //{
        //    foreach (State state in _mdpModel._states)
        //    {
        //        double nextUtility = 
        //    }
        //}

        //private double utilityGivenAction(State s, Action a)
        //{
        //    double[] rewards = _mdpModel._states.Find(s)._rewards;

        //}
    }
}
