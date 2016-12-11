using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnticipatoryTroubleShooting.BayesianInference;

namespace AnticipatoryTroubleShooting
{
    class BayesianNetwork
    {
        public Dictionary<int, Node> _nodes;
        public Dictionary<int, List<int>> _factors;
        public Dictionary<int, List<int>> _forwardFactors;
        public Dictionary<int, Cpt> _cpts;

        private Dictionary<int, int> _mapping;

        public VariableEliminator _inferenceEngine;
        public VariableEliminator_V2 _inferenceEngine2;

        #region constructors
        public BayesianNetwork()
        {
            _nodes = new Dictionary<int, Node>();
            _factors = new Dictionary<int, List<int>>();
            _forwardFactors = new Dictionary<int, List<int>>();
            _cpts = new Dictionary<int, Cpt>();
        }

        public BayesianNetwork(BayesianNetwork bn)
        {
            _nodes = new Dictionary<int, Node>(bn._nodes);
            _factors = new Dictionary<int, List<int>>(bn._factors);
            _forwardFactors = new Dictionary<int, List<int>>(bn._forwardFactors);
            _cpts = new Dictionary<int, Cpt>(bn._cpts);
        }

        public BayesianNetwork(string pathFile)
        {
            _nodes = new Dictionary<int, Node>();
            _factors = new Dictionary<int, List<int>>();
            _cpts = new Dictionary<int, Cpt>();
            _forwardFactors = new Dictionary<int, List<int>>();
            StreamReader sr = new StreamReader(pathFile);
            Dictionary<int, int> domains = readDomains(sr);
            Dictionary<int, List<int>> factors = readFactors(sr);
            Dictionary<int, List<double>> probs = readProbs(sr);
            sr.Close();

            //List<int> topologicalSorted = topologicalSort(factors);
            //Dictionary<int, int> mapping = channgeVarsTopologicalSorterdNNumbers(topologicalSorted, domains, factors, probs);

            BuildNetwork(domains, factors, probs);
            _inferenceEngine = new VariableEliminator(this);
            _inferenceEngine2 = new VariableEliminator_V2(this);

            //writeNNewOrderToFile();
        }
        //-----------------------------------------------------------------------------------------------------------
        public void BuildNetwork(Dictionary<int, int> domains, Dictionary<int, List<int>> factors, Dictionary<int, List<double>> probs)
        {
            _factors = factors;
            foreach (var x in domains)
            {
                Node node = new Node(x.Key, x.Value);
                _nodes.Add(x.Key, node);
            }
            _factors = factors;

            foreach (var node in probs)
            {
                if (node.Key == 5)
                    Console.WriteLine();
                List<Node> currFactors = new List<Node>();
                foreach (var factor in _factors[node.Key])
                {
                    currFactors.Add(_nodes[factor]);
                }
                currFactors.Add(_nodes[node.Key]); //the depnedent var is always at the last row of the structure
                Cpt cpt = new Cpt(currFactors);
                cpt.fillTable(probs[node.Key]);
                _cpts.Add(node.Key, cpt);
            }
            buildForwardFactors();

        }


        #endregion
        //-----------------------------------------------------------------------------------------------------------

        public void changeCptProbs(int nodeID, Dictionary<int,double> newProbs)
        {
            _cpts[nodeID].changeProbs(newProbs);
        }
        public Dictionary<int,double> getCptProbs(int nodeID)
        {
            return _cpts[nodeID]._rowsProbs;
        }

        //-----------------------------------------------------------------------------------------------------------
        public List<double> inference(int nodeID)
        {
            return _inferenceEngine2.doInference(nodeID);
        }
        public List<double> inference(int nodeID, Object o)
        {
            return _inferenceEngine.doInference(nodeID, null);
        }
        public void insertObservedValue(int varNum, int value)
        {
            _nodes[varNum].setObservedValue(value);
            _inferenceEngine._originalBn._nodes[varNum].setObservedValue(value);
        }
        //-----------------------------------------------------------------------------------------------------------
        public List<int> getFactors(int var)
        {
            List<int> ans = new List<int>();
            ans.Add(var);
            foreach (var x in _factors[var])
                ans.Add(x);
            return ans;
        }
        //-----------------------------------------------------------------------------------------------------------

        public double[] getPriors(int varID)
        {
            Dictionary<int, double> rows = _cpts[varID]._rowsProbs;
            double[] probs = rows.Values.ToArray();
            return probs;
        }





        //-----------------------------------------------------------------------------------------------------------
        public void clearObservedValues()
        {
            foreach (var node in _nodes)
            {
                node.Value.clearObservedValue();
            }
        }

        public void clearObservedValue(int compNum)
        {
            _nodes[compNum].clearObservedValue();
        }
        //-----------------------------------------------------------------------------------------------------------

        public void addFactor(int nodeId, int factor)
        {

        }

        //-----------------------------------------------------------------------------------------------------------
        #region initiation

        private Dictionary<int, int> readDomains(StreamReader sr)
        {
            Dictionary<int, int> ans = new Dictionary<int, int>();
            string line = sr.ReadLine();
            string[] domainsS = line.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < domainsS.Length; i++)
            {
                ans.Add(i, int.Parse(domainsS[i]));
            }
            return ans;
        }

        private Dictionary<int, List<int>> readFactors(StreamReader sr)
        {
            Dictionary<int, List<int>> ans = new Dictionary<int, List<int>>();
            //string all= sr.ReadToEnd();
            string line = sr.ReadLine();
            line = sr.ReadLine();
            while (line != "")
            {
                string[] splited = line.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                List<int> currFactors = new List<int>();
                if (splited.Length > 1)
                {
                    string[] factorsS = splited[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var f in factorsS)
                    {
                        if (f == " ")
                            continue;
                        currFactors.Add(int.Parse(f));
                    }
                }
                ans.Add(int.Parse(splited[0]), currFactors);
                line = sr.ReadLine();
            }
            return ans;

        }

        private Dictionary<int, List<double>> readProbs(StreamReader sr)
        {
            Dictionary<int, List<double>> ans = new Dictionary<int, List<double>>();
            string line = sr.ReadLine();
            while (line != null)
            {
                string[] splited = line.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                List<double> currFactors = new List<double>();
                string[] factorsS = splited[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var f in factorsS)
                {
                    currFactors.Add(double.Parse(f));
                }
                ans.Add(int.Parse(splited[0]), currFactors);
                line = sr.ReadLine();
            }
            return ans;

        }
        #endregion




        //----------------------------------------------------------------------------------------------------------------------

        public List<int> topologicalSort(Dictionary<int, List<int>> factorsGiven)
        {
            //create copy of the graph
            Dictionary<int, List<int>> factors = new Dictionary<int, List<int>>();
            for (int i = 0; i < factorsGiven.Count; i++)
            {
                List<int> currInList = new List<int>();
                for (int j = 0; j < factorsGiven[i].Count; j++)
                {
                    currInList.Add(factorsGiven[i][j]);
                }
                factors.Add(i, currInList);
            }

            //sort
            int k = 0;
            List<int> topologicalSorted = new List<int>();
            while (!(topologicalSorted.Count == factors.Count)) //if there is no topological sort the loop can be infinite
            {
                int node = getZeroInDegree(factors, topologicalSorted);
                //remove it node and it's all outgoing edges- remove is acctually done by the function- it not returns a node which akready on the topoligical list
                topologicalSorted.Add(node);
                for (int i = 0; i < factors.Count; i++)
                {
                    for (int j = 0; j < factors[i].Count; j++)
                    {
                        if (factors[i][j] == node)
                        {
                            factors[i].Remove(factors[i][j]);
                        }
                    }
                }
            }
            return topologicalSorted;
        }



        private int getZeroInDegree(Dictionary<int, List<int>> factors, List<int> topologicalSorted)
        {
            //bool found = false;
            foreach (var node in factors)
            {
                if (node.Value.Count == 0 && !topologicalSorted.Contains(node.Key))
                {
                    return node.Key;
                }

            }
            return -1;

        }


        //----------------------------------------------------------------------------------------------------------------------
        public void addNode(List<int> factors) //can only add node that others pointing on it 
        {
            Node newNode = new Node(_nodes.Count, 3);
            _nodes.Add(newNode._nodeID, newNode);
            _factors.Add(newNode._nodeID, factors);
            _forwardFactors.Add(newNode._nodeID, new List<int>());
            foreach (var x in factors)
                _forwardFactors[x].Add(newNode._nodeID);

            CptRow fictiveRow = new CptRow();
            foreach (var x in factors)
            {
                Node node = new Node(_nodes[x]._nodeID, _nodes[x]._domain);
                fictiveRow._vars.Add(x, node);
            }

            List<string> permutations = VariableEliminator.buildAllPermutationInDomain(fictiveRow);
            //insert the newNode now to the row after builing permutations without it
            fictiveRow.addVarToRow(newNode);
            Cpt cpt = new Cpt(fictiveRow);
            int rowNum = 0;
            foreach (string perm in permutations)
            {
                int value = checkForValue(perm);
                insertRows(value, rowNum, cpt);
                rowNum += 3;
            }
            _cpts.Add(newNode._nodeID, cpt);

        }

        private int checkForValue(string row)
        {
            int sawFault = 0;

            for (int i = 0; i < row.Length; i++ )
            {
                int num = int.Parse(row[i].ToString());
                if (num > 0)
                {
                    sawFault++;
                    if (sawFault > 1)
                        return 2;
                }

            }
            return sawFault;
        }

        private void insertRows(int value, int rowNum, Cpt cpt)
        {
            if (value == 0)
            {
                cpt._rowsProbs.Add(rowNum, 1);
                cpt._rowsProbs.Add(rowNum + 1, 0);
                cpt._rowsProbs.Add(rowNum + 2, 0);
            }
            if (value == 1)
            {
                cpt._rowsProbs.Add(rowNum, 0);
                cpt._rowsProbs.Add(rowNum + 1, 1);
                cpt._rowsProbs.Add(rowNum + 2, 0);
            }
            if (value == 2)
            {
                cpt._rowsProbs.Add(rowNum, 0);
                cpt._rowsProbs.Add(rowNum + 1, 0);
                cpt._rowsProbs.Add(rowNum + 2, 1);
            }
        }
        //----------------------------------------------------------------------------------------------------------------------
        private Dictionary<int, int> channgeVarsTopologicalSorterdNNumbers(List<int> topologicalSorted, Dictionary<int, int> domains, Dictionary<int, List<int>> factors, Dictionary<int, List<double>> probs)
        {
            Dictionary<int, int> domainsCopy = new Dictionary<int, int>();
            Dictionary<int, List<int>> factorsCopy = new Dictionary<int,List<int>>();
            Dictionary<int, List<double>> probsCopy = new Dictionary<int, List<double>>();

            Dictionary<int, int> mapping = new Dictionary<int,int>();
            int c=0;
            foreach (var x in topologicalSorted)
            {
                mapping.Add(x, c);
                c++;
            }

            for (int i=0; i< factors.Count; i++)
            {
                domainsCopy.Add(domains.Keys.ElementAt(i), domains.Values.ElementAt(i));
                factorsCopy.Add(factors.Keys.ElementAt(i), factors.Values.ElementAt(i));
                probsCopy.Add(probs.Keys.ElementAt(i), probs.Values.ElementAt(i));
            }

            for (int i=0; i< factors.Count; i++)
            {
                domains[i] = domainsCopy[topologicalSorted[i]];
                factors[i] = factorsCopy[topologicalSorted[i]];
                probs[i] = probsCopy[topologicalSorted[i]];
                for (int j=0; j< factors[i].Count; j++)
                {
                    factors[i][j] = mapping[factors[i][j]];
                }
            }
            _mapping = new Dictionary<int, int>();
            _mapping = mapping;
            return mapping;
        }

        public int getMapping(int oldNum)
        {
            if (_mapping == null)
                return oldNum;
            return _mapping[oldNum];
        }


        private void writeNNewOrderToFile()
        {
            List<string> lines = new List<string>();
            string line = string.Empty;
            List<string> ss = new List<string>();
            foreach (var x in _nodes)
            {
                ss.Add(x.Value._domain.ToString());
            }
            line = string.Join(", ", ss);
            lines.Add(line);
            
            foreach (var x in _nodes)
            {
                line = x.Key + ": ";
                ss = new List<string>();
                line += string.Join(", ", _factors[x.Key]);
                lines.Add(line);
            }

            foreach (var x in _cpts)
            {
                line = x.Key + ": " + x.Value.ToString();
                lines.Add(line);
            }

            File.WriteAllLines("aaaa.txt", lines);

        }



        private void buildForwardFactors()
        {
            foreach (var node in _nodes)
            {
                List<int> currForwardFactors = new List<int>();
                _forwardFactors.Add(node.Key, currForwardFactors);
                foreach (var pointedNode in _factors)
                {
                    if (pointedNode.Key == node.Key)
                        continue;
                    foreach (var node2 in _factors[pointedNode.Key])
                    {
                        if (node.Key == node2)
                            currForwardFactors.Add(pointedNode.Key);
                    }
                }
            }
        }


        public bool verifyCpts()
        {
            foreach (var cpt in _cpts)
            {
                if (!cpt.Value.verifyCpt())
                    return false;
            }
            return true;
        }


        public void updateNodesValues(Dictionary<int, int> values)
        {
            foreach (var x in values)
            {
                Node node = _nodes[x.Key];
                node.setObservedValue(x.Value);
            }
        }


    }
}
