using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.Models
{
    class RandomModel : Model
    {
        public RandomModel(Model model)
        {
            _components = model._components;
            _testComponents = model._testComponents;
            _sensorComponents = model._sensorComponents;
            _controlComponents = model._controlComponents;
        }

        public override double[] getComponentDistribution(int componentNum)
        {
            return new double[] { 0.5, 0.5 };
        }
        public override double getFaultProb(int componentNum)
        {
            Component comp = _components[componentNum];
            return comp._distribution[1]; 
        }

        public override string ToString()
        {
            return "RandomModel";
        }

        public override bool verifyModel()
        {
            throw new NotImplementedException();
        }

    }
}
