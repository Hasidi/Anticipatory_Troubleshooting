using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.MDP
{
    class MDPmodel
    {
        int _nStates;
        public List<State> _states;
        Dictionary<State, List<double>> _transitions;

        double _dicountFactor;

        ValueIteration _solver;

        public MDPmodel()
        {

        }




    }
}
