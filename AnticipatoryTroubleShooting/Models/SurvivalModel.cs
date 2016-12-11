using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.SurvivalFunctions;

namespace AnticipatoryTroubleShooting.Models
{
    class SurvivalModel : Model
    {
        Dictionary<int, SurvivalFunction> _surviveFunctions;


        public SurvivalModel(Model model)
        {
            _surviveFunctions = new Dictionary<int, SurvivalFunction>();
            _components = model._components;
            _testComponents = model._testComponents;
            _sensorComponents = model._sensorComponents;
            _controlComponents = model._controlComponents;
            //initModel(_components);
        }
        //-----------------------------------------------------------------------------------------------------------
        public override double[] getComponentDistribution(int componentNum)
        {
            double[] ans = new double[2];
            Component comp = _components[componentNum];
            ans[0] = _surviveFunctions[componentNum].survive(comp._age);
            ans[1] = 1- ans[0];
            comp._distribution = ans;
            return ans;
        }
        //-----------------------------------------------------------------------------------------------------------
        public override double getFaultProb(int componentNum)
        {
            Component comp = _components[componentNum];
            return comp._distribution[1];
        }
        //-----------------------------------------------------------------------------------------------------------
        public override void initModel(Dictionary<int, Component> components)
        {
            base.initModel(components);
            _surviveFunctions = new Dictionary<int, SurvivalFunction>();
            foreach (var testCompId in _testComponents)
            {
                double currSurvive = components[testCompId]._survivalFactor;
                _components[testCompId]._survivalFactor = currSurvive;
                _surviveFunctions.Add(testCompId, new ExponentialDecayCurve(currSurvive));
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "SurvivalModel";
        }
        //------------------------------------------------------------------------------------------------------------------
        public override bool verifyModel()
        {
            throw new NotImplementedException();
        }
    }
}
