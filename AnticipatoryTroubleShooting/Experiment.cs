using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class Experiment
    {
        public Dictionary<int, Component> _components { get; set; }
        public double _ageDiff { get; set; }
        //public List<int> _sensorsToReveal { get; set; }

        //public Experiment(Dictionary<int, Component> components, double ageDiff, List<int> sensorsToReveal)
        //{
        //    _components = components;
        //    _ageDiff = ageDiff;
        //    _sensorsToReveal = sensorsToReveal;
        //}

        public Experiment(Dictionary<int, Component> components, double ageDiff)
        {
            _components = components;
            _ageDiff = ageDiff;
        }
        public Experiment(Dictionary<int, Component> components)
        {
            _components = components;
        }

    }
}
