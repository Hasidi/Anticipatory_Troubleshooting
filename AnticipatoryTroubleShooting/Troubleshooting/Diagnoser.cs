using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class Diagnoser
    {
        Model _model;
        IObserveComponentSelection _componentSelector;
        public TroubleShooter _troubleShooter;
        public int _nCurrBrokenComponents;



        public Diagnoser(Model model, TroubleShooter troubleShooter, IObserveComponentSelection componentSelector)
        {
            _model = model;
            _componentSelector = componentSelector;
            _troubleShooter = troubleShooter;
            _nCurrBrokenComponents = 0;
        }
        public Dictionary<int, Component> diagnose()
        {
            Dictionary<int, Component> testComponentsObservations = _model.getCurrentComponentsSituation(_troubleShooter._currSuspectedComponents);
            return testComponentsObservations;
        }
        //-----------------------------------------------------------------------------------------------------------
        public int selectComponentToObserve(Dictionary<int, Component> componentsDistributions)
        {
            int selectedComp = _componentSelector.chooseComponent(componentsDistributions);
            return selectedComp;
        }
        //-----------------------------------------------------------------------------------------------------------
        public int observeComponent(int compNum)
        {
            Component comp = _model._components[compNum];
            comp.updateObserveValue(comp._preTestObservation);
            return comp._preTestObservation;
        }
        //-----------------------------------------------------------------------------------------------------------
        public bool systemHelathy()
        {
            return _nCurrBrokenComponents == 0;
        }
        //-----------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            string ans = _model.ToString() + "_Diagnoser";
            return ans;
        }
        //-----------------------------------------------------------------------------------------------------------


    }
}
