using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.SurvivalFunctions;

namespace AnticipatoryTroubleShooting.Models
{
    class Survival_IN_BayesModel :SurvivalBayesModel
    {
        public Survival_IN_BayesModel(string networkPath, string componentTypeFile)
            : base(networkPath, componentTypeFile)
        {
            
        }
        //----------------------------------------------------------------------------------------------------------------------
        public override double[] getComponentDistribution(int componentNum)
        {
            double[] dis = base.getComponentDistribution(componentNum); // calling gradfather function      
            return dis;
        }

        //----------------------------------------------------------------------------------------------------------------------
        public override double getFaultProb(int componentNum)
        {
            Component comp = _components[componentNum];
            SurvivalFunction currCurve = _survivalCurves[componentNum];
            double oldnessRate = currCurve.getOldnessRate(_components[componentNum]._age);
            comp._svProb = oldnessRate;
            base.getFaultProb(componentNum); // update the bs fault prob according earlier inference
            comp._faultProb = comp._bsProb;

            return comp._faultProb;
        }
        //----------------------------------------------------------------------------------------------------------------------
        public override void initModel()
        {
            base.initModel();
            foreach (var testCompNum in _testComponents)
            {
                Component currTestComp = _components[testCompNum];
                currTestComp._svProb = getFaultSurviveProb(testCompNum);
                _bsn._cpts[testCompNum].changePriorProb(1 - currTestComp._svProb);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------
        public override void initModel(Dictionary<int, Component> components)
        {
            base.initModel(components);
            foreach (var testCompNum in _testComponents)
            {
                Component currTestComp = _components[testCompNum];
                currTestComp._svProb = getFaultSurviveProb(testCompNum);
                _bsn._cpts[testCompNum].changePriorProb(1 - currTestComp._svProb);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------
        public void initPriorProbs()
        {
            foreach (var testCompNum in _testComponents)
            {
                Component currTestComp = _components[testCompNum];
                currTestComp._svProb = getFaultSurviveProb(testCompNum);
                _bsn._cpts[testCompNum].changePriorProb(1 - currTestComp._svProb);
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "Survival_IN_BayesModel";
        }
        //------------------------------------------------------------------------------------------------------------------

    }
}
