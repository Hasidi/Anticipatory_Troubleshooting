using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    abstract class Model
    {

#region vars
        public int _nComponents;

        public Dictionary<int, Component> _components;
        public List<int> _testComponents;
        public List<int> _controlComponents;
        public List<int> _sensorComponents;


        public List<int> _faultComponents;
        public int _nFaultComponents;
#endregion

        //-----------------------------------------------------------------------------------------------------------
        public abstract double[] getComponentDistribution(int componentNum);

        //-----------------------------------------------------------------------------------------------------------
        public abstract double getFaultProb(int componentNum);
        //-----------------------------------------------------------------------------------------------------------     
        //public double getFaultProb(int componentNum, double[] distribution)
        //{
        //    getComponentDistribution(componentNum); //the distribution is update in the function call
        //    Component comp = _components[componentNum];
        //    distribution = comp._distribution;
        //    return getFaultProb(componentNum);
        //}
        //-----------------------------------------------------------------------------------------------------------
        public virtual void updateObservedValue(int compID, int observedVal)
        {
            _components[compID].updateObserveValue(observedVal);
        }
        //-----------------------------------------------------------------------------------------------------------
        public virtual void clearObservedValues()
        {
            foreach (var comp in _components)
            {
                comp.Value.clearObservedValue();
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public virtual void clearObservedValue(int compNum)
        {
            _components[compNum].clearObservedValue();
        }
        //-----------------------------------------------------------------------------------------------------------
        public virtual void initModel(Dictionary<int, Component> components)
        {
            clearObservedValues();

            foreach (var testComp in _testComponents)
            {
                Component currComponent = _components[testComp];
                Component currComp = components[testComp];
                currComponent._preTestObservation = currComp._preTestObservation;
                currComponent._repairCost = currComp._repairCost;
                currComponent._survivalFactor = currComp._survivalFactor;
                currComponent._age = currComp._age;
            }
            foreach (var controlComp in _controlComponents)
            {
                Component currComponent = components[controlComp];
                _components[controlComp]._preTestObservation = currComponent._preTestObservation;
            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public virtual void initModel() 
        {
            clearObservedValues();
        }
        //-----------------------------------------------------------------------------------------------------------
        //public virtual void insertTestControlModelValues(Dictionary<int, Component> components)
        //{
        //    foreach (var testComp in _testComponents)
        //    {
        //        Component currComponent = _components[testComp];
        //        Component currComp = components[testComp];
        //        currComponent._preTestObservation = currComp._preTestObservation;
        //        //currComponent._repairCost = currComp._repairCost;
        //        currComponent._survivalFactor = currComp._survivalFactor;
        //        currComponent._age = currComp._age;
        //    }

        //    foreach (var controlComp in _controlComponents)
        //    {
        //        Component currComponent = components[controlComp];
        //        _components[controlComp]._preTestObservation = currComponent._preTestObservation;
        //    }

        //}
        //-----------------------------------------------------------------------------------------------------------
        public virtual Dictionary<int, Component> getCurrentComponentsSituation(List<int> components)
        {
            Dictionary<int, Component> comps = new Dictionary<int, Component>();
            foreach (var compN in components)
            {
                double[] currDis = getComponentDistribution(compN);
                Component currComp = _components[compN];
                double faultProb = getFaultProb(compN);
                currComp._faultProb = faultProb;
                comps.Add(compN, currComp);
            }
            return comps;
        }
        //-----------------------------------------------------------------------------------------------------------
        public void sampleControlComponents()
        {
            int val = UsefulFunctions.randFromGivenSet(new List<int>() { 0, 1 });
            foreach (var comp in _controlComponents)
            {
                if (comp == _components.Count-1)
                    continue;
                //double[] distribution = getComponentDistribution(comp);
                //int value = UsefulFunctions.createSample(distribution);
                _components[comp].updateObserveValue(val);
                val = UsefulFunctions.randFromGivenSet(new List<int>() { 0, 1 });
            }
            _components[_components.Count - 1].updateObserveValue(1);
        }
        //-----------------------------------------------------------------------------------------------------------
        public void sampleSensorComponents(Dictionary<int, int> revealedSensors)
        {

        }
        //-----------------------------------------------------------------------------------------------------------
        protected void onStart()
        {
            _components = new Dictionary<int, Component>();
            _testComponents = new List<int>();
            _controlComponents = new List<int>();
            _sensorComponents = new List<int>();
            _faultComponents = new List<int>();
        }


        //-----------------------------------------------------------------------------------------------------------
        //returns copy of model's tests components
        public Dictionary<int, Component> getTestsComponentsCopy()
        {
            Dictionary<int, Component> ans = new Dictionary<int, Component>();
            foreach (var compID in _testComponents)
            {
                Component currComp = _components[compID];
                Component copyComp = new Component(currComp);
                ans.Add(compID, copyComp);
            }
            return ans;
        }

        //-----------------------------------------------------------------------------------------------------------

        public virtual void updateComps(Dictionary<int, Component> components)
        {
            //Models.SurvivalBayesModel survModel = this as Models.SurvivalBayesModel;
            foreach (var comp in components)
            {
                _components[comp.Key] = new Component(comp.Value);
                //not sure this is the place to update curve
                //if (survModel != null)
                //    survModel.updateSurvivalCurve(comp.Key, comp.Value._survivalFactor);

            }
        }

        public virtual void updateComps(ref Dictionary<int, Component> components)
        {
            //Models.SurvivalBayesModel survModel = this as Models.SurvivalBayesModel;
            foreach (var comp in components)
            {
                _components[comp.Key] = new Component(comp.Value);
                //not sure this is the place to update curve
                //if (survModel != null)
                //    survModel.updateSurvivalCurve(comp.Key, comp.Value._survivalFactor);

            }
        }
        //-----------------------------------------------------------------------------------------------------------
        public void updateCompsAges(double timeToAdd)
        {
            foreach (var compID in _testComponents)
            {
                Component currComp = _components[compID];
                currComp._age += timeToAdd;
            }
        }


        //-----------------------------------------------------------------------------------------------------------
        public abstract bool verifyModel();
    }



}
