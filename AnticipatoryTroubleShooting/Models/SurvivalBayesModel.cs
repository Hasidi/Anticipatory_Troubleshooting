using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.SurvivalFunctions;

namespace AnticipatoryTroubleShooting.Models
{
    class SurvivalBayesModel : BayesianNetworkModel
    {
        public Dictionary<int, SurvivalFunction> _survivalCurves;


        public SurvivalBayesModel(string networkPath, string componentTypeFile)
            : base(networkPath, componentTypeFile)
        {
            _survivalCurves = new Dictionary<int, SurvivalFunction>();
            foreach (var compNum in _testComponents)
            {
                Component testComponent = _components[compNum];
                //_survivalCurves.Add(compNum, new ExponentialDecayCurve(testComponent._survivalFactor));
                _survivalCurves.Add(compNum, new WeibullCurve(testComponent._survivalFactor));

            }

        }
        //----------------------------------------------------------------------------------------------------------------------
        //public SurvivalBayesModel(string networkPath, string componentTypeFile, SurvivalFunction survivalFunc)
        //    : base(networkPath, componentTypeFile)
        //{
        //    _survivalCurves = new Dictionary<int, SurvivalFunction>();
        //    foreach (var compNum in _testComponents)
        //    {
        //        Component testComponent = _components[compNum];
        //        _survivalCurves.Add(compNum, survivalFunc);

        //    }
        //}
        //----------------------------------------------------------------------------------------------------------------------
        public override double getFaultProb(int componentNum)
        {
            double oldnessRate = getFaultSurviveProb(componentNum);
            double bsProb = base.getFaultProb(componentNum);
            Component comp = _components[componentNum];
            //comp._bsProb = bsProb; // not needed- done in father's fucntion
            comp._svProb = oldnessRate;
            comp._faultProb = calcHybridProb(bsProb, oldnessRate);
            return comp._faultProb;

        }
        //----------------------------------------------------------------------------------------------------------------------

        public override void initModel()
        {
            base.initModel();
            foreach (var testCompId in _testComponents)
            {
                //_components[testCompId]._survivalFactor = components[testCompId]._survivalFactor;
                _survivalCurves[testCompId].setParameter(_components[testCompId]._survivalFactor);
            }
        }



        public override void initModel(Dictionary<int,Component> components)
        {
            base.initModel(components);
            foreach (var testCompId in _testComponents)
            {
                //_components[testCompId]._survivalFactor = components[testCompId]._survivalFactor;
                _survivalCurves[testCompId].setParameter(components[testCompId]._survivalFactor);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------
        protected double getFaultSurviveProb(int compId)
        {
            SurvivalFunction currCurve = _survivalCurves[compId];
            double oldnessRate = currCurve.getOldnessRate(_components[compId]._age);
            return oldnessRate;
        }
        //----------------------------------------------------------------------------------------------------------------------
        protected double calcHybridProb(double bsProb, double oldnessRate)
        {
            double ans = bsProb + oldnessRate - bsProb * oldnessRate;
            return ans;
        }
        //----------------------------------------------------------------------------------------------------------------------

        public void updateSurvivalCurve(int compID, double newCurveFactor)
        {
            _survivalCurves[compID].setParameter(newCurveFactor);
            _components[compID]._survivalFactor = newCurveFactor;
        }

        //----------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "SurvivalBayesModel";
        }
        //------------------------------------------------------------------------------------------------------------------

        public override void updateComps(Dictionary<int, Component> components)
        {
            foreach (var comp in components)
            {
                _components[comp.Key] = new Component(comp.Value);
                _bsn.insertObservedValue(comp.Key, comp.Value._observedValue);
                updateSurvivalCurve(comp.Key, comp.Value._survivalFactor);

            }
        }

        //------------------------------------------------------------------------------------------------------------------



        //------------------------------------------------------------------------------------------------------------------

    }
}
