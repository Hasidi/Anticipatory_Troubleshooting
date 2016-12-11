using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting.BayesianInference
{
    class VariableEliminator_V2
    {

        public BayesianNetwork _originalBn;
        BayesianNetwork _currBn;
        List<int> _outedNodes;
        private static int MAX_ROWS_ON_Memory = 1;
        private static string CPTS_PATH = @"../bin/debug/files/cpts";
        Dictionary<int, List<Cpt>> _currFactors;
        Dictionary<Cpt, List<int>> _currFactorsToNodes;
        CptRow _queryObservations;
        public static int CPT_ELIMINATION_NUMBER = -100;

        private double _constants = 1;


        public VariableEliminator_V2(BayesianNetwork bn)
        {
            _originalBn = bn;
            _currBn = new BayesianNetwork(bn);
            _outedNodes = new List<int>();

        }

        public List<double> doInference(int nodeID)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            _outedNodes = new List<int>();
            initFactors();
            List<Node> hiddenNodes = new List<Node>();
            foreach (var node in _originalBn._nodes)
            {
                if (!node.Value.isObserved() && 
                    node.Key != nodeID)
                //if (!node.Value.isObserved())
                   hiddenNodes.Add(node.Value);
            }
            List<int> eliminationOrder = chooseEliminationOrder(hiddenNodes);
            Cpt joinedCpt = null;
            //TimeSpan timeElapsed;
            while (eliminationOrder.Count > 0)
            {

                int currElimination = eliminationOrder.First();
                joinedCpt = summingOutNode(currElimination);
                _outedNodes.Add(currElimination);
                eliminationOrder.Remove(currElimination);
                //timeElapsed = stopwatch.Elapsed;
                //if (timeElapsed.Seconds > 7)
                //    Console.WriteLine();
            }
            if (_currFactors.Count != 1)
            {
                //throw new InvalidProgramException();
                Console.WriteLine();
            }
            

            double[] distribution = joinRemainingFactors(nodeID);
            return distribution.ToList();
            
            return null;


        }


        private double[] joinRemainingFactors(int nodeId)
        {
            //CptRow fictiveRow = new CptRow();
            //fictiveRow._vars.Add(nodeId, new Node(_originalBn._nodes[nodeId]));
            //Cpt ansCpt = new Cpt(fictiveRow);
            double[] ans = new double[_originalBn._nodes[nodeId]._domain];
            for (int i = 0; i < ans.Length; i++)
            {
                double currProb = 1;
                foreach (var cpt in _currFactors[nodeId])
                {
                    CptRow row = new CptRow();
                    row.addVarToRow(new Node(_originalBn._nodes[nodeId]));
                    row._vars[nodeId].setObservedValue(i);
                    int rowNum = cpt.getRowNumber(row, _queryObservations);
                    currProb *= cpt._rowsProbs[rowNum];
                }
                ans[i] = currProb;
            }
            normalize(ans);
            return ans;

        }

        private void normalize(double[] distribution)
        {
            double denominator = 0;
            for (int i=0; i<distribution.Length; i++)
            {
                denominator += distribution[i];
            }
            for (int i = 0; i < distribution.Length; i++)
            {
                distribution[i] = distribution[i] / denominator;
            }
        }
        /*
        private List<int> findFactorsOnJoin(int joinedVar, ref List<int> distinctFactors)
        {
            List<int> ans = new List<int>();
            foreach (var factor in _currBn._factors[joinedVar])
            {
                ans.Add(factor);
            }
            foreach (var forwardFactor in _currBn._forwardFactors[joinedVar])
            {
                if (!distinctFactors.Contains(forwardFactor))
                    distinctFactors.Add(forwardFactor);
                foreach (var factor in _currBn._forwardFactors[forwardFactor])
                {
                    if (!ans.Contains(factor))
                        ans.Add(factor);
                }
            }
            return ans;
        }

        //--------------------------------------------------------------------------------------------------------------------
        public bool checkAvailablePerm(CptRow row)
        {
            foreach (var node in row._vars)
            {
                Node currNode = _currBn._nodes[node.Key];
                if (currNode.isObserved())
                {
                    if (node.Value.getObservedValue() != currNode.getObservedValue())
                        return false;
                }
            }
            return true;
        }
        */
        //--------------------------------------------------------------------------------------------------------------------

        public List<int> chooseEliminationOrder(List<Node> hiddenNodes)
        {
            //List<int> sorted = sort().ToList();
            //return sorted;
            List<int> ans = new List<int>();
            foreach (var x in hiddenNodes)
                ans.Add(x._nodeID);
            return ans;
        }
        //----------------------------------------------------------------------------------------------------------------------
        private void initFactors()
        {
            _currFactors = new Dictionary<int, List<Cpt>>();
            foreach (var node in _originalBn._factors.Keys)
            {
                List<Cpt> currCptList = new List<Cpt>();
                _currFactors.Add(node, currCptList);
                currCptList.Add(_originalBn._cpts[node]);
                foreach (var pointedNode in _originalBn._forwardFactors[node])
                {
                    currCptList.Add(_originalBn._cpts[pointedNode]);
                }

            }
            _currFactorsToNodes = new Dictionary<Cpt, List<int>>();

            _queryObservations = new CptRow();
            foreach (var node in _originalBn._nodes)
            {
                Node observedNode = null;
                if (node.Value.isObserved())
                    observedNode = new Node(_originalBn._nodes[node.Key]);
                if (observedNode != null)
                    _queryObservations._vars.Add(observedNode._nodeID, observedNode);
            }
        }




        //--------------------------------------------------------------------------------------------------------------------


        private Cpt summingOutNode(int outedNode)
        {
            Cpt joinedCpt;
            List<int> newCptsFactors = findFactors(outedNode);
            if (newCptsFactors.Count == 0)
            {
                _constants *= joinFactors(outedNode, new CptRow());
                eraseJoinedFactors(outedNode);
                _currFactors.Remove(outedNode);
                return null;
            }
            CptRow fictiveRow = new CptRow();
            foreach (var x in newCptsFactors)
            {
                Node node = new Node(_originalBn._nodes[x]);
                fictiveRow._vars.Add(x, node);
            }
            //calculate for each row the multiplication of each factor
            List<string> permutations = buildAllPermutationInDomain(fictiveRow);
            //Console.WriteLine(permutations.Count);
            joinedCpt = new Cpt(fictiveRow._vars.Values.ToList());
            if (permutations.Count > 0)
            {
                foreach (var perm in permutations)
                {
                    CptRow row = new CptRow(fictiveRow);
                    row.insertValues(perm);
                    double prob = joinFactors(outedNode, row);
                    row._prob = prob;
                    joinedCpt.addRow(row);
                    
                }
            }

            //
            foreach (var node in newCptsFactors)
            {
                if (!_outedNodes.Contains(node))
                    _currFactors[node].Add(joinedCpt);  //true check here: an exception will acuur if outedNode will be still be in _currFactors
            }
            
            //erase all joinedFactors from _currFactors
            eraseJoinedFactors(outedNode);
            _currFactors.Remove(outedNode);

            //_outedNodes.Add(outedNode);
            return joinedCpt;
        }
        //--------------------------------------------------------------------------------------------------------------------
        /*
        private double joinConstantFactors(int outedNode)
        {
            foreach ()
        }
        */
        //--------------------------------------------------------------------------------------------------------------------

        private void eraseJoinedFactors(int outedNode)
        {
            for (int i = 0; i < _currFactors[outedNode].Count; i++)
            {
                Cpt currCpt = _currFactors[outedNode].ElementAt(i);
                for (int j = 0; j < _currFactors.Count; j++)
                {
                    if (outedNode == _currFactors.ElementAt(j).Key)
                        continue;
                    for (int k = 0; k < _currFactors.ElementAt(j).Value.Count; k++)
                    {
                        Cpt verifyCpt = _currFactors.ElementAt(j).Value.ElementAt(k);
                        if (currCpt == verifyCpt)
                        {
                            _currFactors.ElementAt(j).Value.Remove(verifyCpt);
                            k--;
                        }
                        //if (_currFactors.ElementAt(j).Value.Count == 0)
                        //{
                        //    _currFactors.Remove(_currFactors.ElementAt(j).Key);
                        //    j--;
                        //}
                    }
                }
                _currFactors[outedNode].Remove(currCpt);
                i--;
            }
            for (int i = 0; i < _currFactors.Count; i++)
            {
                if (_currFactors.ElementAt(i).Value.Count == 0)
                {
                    _currFactors.Remove(_currFactors.ElementAt(i).Key);
                    i--;
                }
            }
            //_currFactors.Remove(outedNode);
        }


        //--------------------------------------------------------------------------------------------------------------------

        private List<int> findFactors(int outedNode)
        {
            List<int> ans = new List<int>();
            foreach (var cpt in _currFactors[outedNode])
            {
                foreach (var nodeId in cpt._rowStructure.Keys)
                {
                    if (!ans.Contains(nodeId) && !_originalBn._nodes[nodeId].isObserved() && nodeId != outedNode)
                    {                  
                        ans.Add(nodeId);
                    }

                }
            }
            return ans;
        }


        private double joinFactors(int outVar, CptRow rowObservation)
        {
            double sum = 0;
            for (int i = 0; i < _originalBn._nodes[outVar]._domain; i++)
            {
                CptRow currRow = new CptRow(rowObservation);
                currRow.addVarToRow(new Node(_originalBn._nodes[outVar]));
                currRow._vars[outVar].setObservedValue(i);
                double prob = 1;
                foreach (var cpt in _currFactors[outVar])
                {
                    double currProb = cpt.getRowProb(currRow, _queryObservations);
                    prob *= currProb;
                }
                sum += prob;
            }
            return sum;
        }

        //-----------------------------------------------------------------------------------------------------------------
       
        public static List<string> buildAllPermutationInDomain(CptRow row)
        {
            List<string> ans = new List<string>();
            List<int> domains = new List<int>();
            foreach (var x in row._vars)
            {
                domains.Add(x.Value._domain);
            }
            bool writenToDisk = false;
            recPermutationsDomain("", domains, 0, ans, ref writenToDisk);
            if (writenToDisk)
                return new List<string>() { };
            return ans;
        }

        private static void recPermutationsDomain(string oldPerm, List<int> domains, int place, List<string> allPerms, ref bool writenToDisk)
        {
            if (place >= domains.Count)
            {
                /*
                if (allPerms.Count > MAX_ROWS_ON_Memory)
                    writePermsToDisk(ref allPerms, ref writenToDisk);*/
                allPerms.Add(oldPerm);
                return;
            }
            for (int i = 0; i < domains[place]; i++)
            {
                oldPerm += i;
                recPermutationsDomain(oldPerm, domains, place + 1, allPerms, ref writenToDisk);
                oldPerm = oldPerm.Substring(0, oldPerm.Length - 1);

            }
        }

        //-------------------------------------------------------------------------------------------------------------------

        private int[] sort()
        {
            Dictionary<int, int> toSort = new Dictionary<int, int>();
            foreach (var x in _currFactors)
            {
                toSort.Add(x.Key, x.Value.Count);
            }
            int[] keys = toSort.Keys.ToArray();

            bool swapped = true;
            int place = 1;
            while (swapped)
            {
                swapped = false;
                for (int i = 0; i < toSort.Count - place; i++)
                {
                    int biggestKey = -1;
                    if (toSort.ElementAt(i).Value> toSort.ElementAt(i + 1).Value)
                    {
                        swapped = true;
                        int temp = toSort.ElementAt(i+1).Value;
                        int nextKey = toSort.ElementAt(i + 1).Key;
                        int prevKey = toSort.ElementAt(i).Key;
                        toSort[nextKey] = toSort[prevKey];
                        toSort[prevKey] = temp;
                        temp = keys[i + 1];
                        keys[i + 1] = keys[i];
                        keys[i] = temp;
                    }
                }
                place++;
                //sorted.Add(toSort.ElementAt(toSort.Count - place).Key); //add at the end of the list
            }
            return keys;
        }

        //-------------------------------------------------------------------------------------------------------------------
    }
}
