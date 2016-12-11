using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.BayesianInference;
using System.IO;

namespace AnticipatoryTroubleShooting
{
    class BayesianNetworkModel : Model
    {
        protected BayesianNetwork _bsn;
        private string NETWORK_FILE_PATH = @"../Debug/Files/NetworkFiles/";

        public BayesianNetworkModel(string networkFileName, string componentTypeFile)
        {
            onStart();
            _bsn = new BayesianNetwork(NETWORK_FILE_PATH + networkFileName);
            readComponentFileType(NETWORK_FILE_PATH + componentTypeFile);
            addFaultIndicatorNode();
            initBayessianModel();
        }
        //-----------------------------------------------------------------------------------------------------------
        #region onStart
        private void readComponentFileType(string filePath)
        {
            StreamReader sr = new StreamReader(filePath);
            string line = sr.ReadLine();
            string[] comps = line.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string comp in comps)
            {
                if (comp != " ")
                    _controlComponents.Add(_bsn.getMapping(int.Parse(comp)));
            }
            line = sr.ReadLine();
            comps = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string comp in comps)
            {
                _testComponents.Add(_bsn.getMapping(int.Parse(comp)));
            }
            line = sr.ReadLine();
            comps = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string comp in comps)
            {
                _sensorComponents.Add(_bsn.getMapping(int.Parse(comp)));
            }
            sr.Close();
        }
        //-----------------------------------------------------------------------------------------------------------
        public void addFaultIndicatorNode()
        {
            _bsn.addNode(_testComponents);
            _controlComponents.Add(_bsn._nodes.Count - 1);
        }
        //-----------------------------------------------------------------------------------------------------------
        protected void initBayessianModel()
        {
            foreach (var node in _bsn._nodes)
            {
                Component comp = null;
                if (_testComponents.Contains(node.Key))
                {
                    comp = new Component(node.Key, node.Value._domain, ComponentType.TEST);
                }
                else if (_controlComponents.Contains(node.Key))
                {
                    comp = new Component(node.Key, node.Value._domain, ComponentType.COMMAND);
                }
                else
                {
                    comp = new Component(node.Key, node.Value._domain, ComponentType.SENSOR);
                }
                _components[node.Key] = comp;
            }
            _nComponents = _components.Count;
        }
        #endregion
        //-----------------------------------------------------------------------------------------------------------
        public override double[] getComponentDistribution(int componentNum)
        {
            List<double> distributation = _bsn.inference(componentNum);
            //List<double> distributation = new List<double>() { 0.01, 0.99 };

            Component comp = _components[componentNum];
            comp._distribution = distributation.ToArray();
            return comp._distribution;
        }
        //-----------------------------------------------------------------------------------------------------------
        //dont need an inference operation
        public override double getFaultProb(int componentNum)
        {
            if (_components[componentNum]._distribution == null)
                throw new InvalidOperationException();
            double[] distributation = _components[componentNum]._distribution;
            double sum = 0;
            for (int i = 1; i < distributation.Length; i++)
            {
                sum += distributation[i];
            }
            Component comp = _components[componentNum];
            comp._bsProb = sum;
            comp._faultProb = sum;
            return sum;
        }
        //-----------------------------------------------------------------------------------------------------------
        public override void updateObservedValue(int compID, int observedVal)
        {
            base.updateObservedValue(compID, observedVal);
            _bsn.insertObservedValue(compID, observedVal);
        }
        //-----------------------------------------------------------------------------------------------------------
        public override void clearObservedValues()
        {
            base.clearObservedValues();
            _bsn.clearObservedValues();
        }
        //-----------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "BayesianModel";
        }
        //-----------------------------------------------------------------------------------------------------------

        public override bool verifyModel()
        {
            return _bsn.verifyCpts();
        }
        //-----------------------------------------------------------------------------------------------------------

        public override void updateComps(Dictionary<int, Component> components)
        {
            foreach (var comp in components)
            {
                _components[comp.Key] = comp.Value;
                _bsn.insertObservedValue(comp.Key, comp.Value._observedValue);

            }
        }


        public override void initModel()
        {
            base.initModel();
            _bsn.clearObservedValues();

        }

    }
}
