using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.MDP
{
    class State
    {

        public Action[] _actionsAvailable;
        public double[] _rewards;

        public double _utility;
        public Action _bestAction;


    }
}
