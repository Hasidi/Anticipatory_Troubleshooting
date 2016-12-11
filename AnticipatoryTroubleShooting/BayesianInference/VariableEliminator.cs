using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class VariableEliminator
    {
        public BayesianNetwork _originalBn;
        BayesianNetwork _currBn;
        List<int> _outedNodes;
        private static int MAX_ROWS_ON_Memory = 1;
        private static string CPTS_PATH = @"../bin/debug/files/cpts";

        public static int CPT_ELIMINATION_NUMBER = -100;


        public VariableEliminator(BayesianNetwork bn)
        {
            _originalBn = bn;
            _currBn = new BayesianNetwork(bn);
            _outedNodes = new List<int>();
        }
        //----------------------------------------------------------------------------------------------------------------------
        public List<double> doInference(int nodeID, Dictionary<int, int> observations)
        {
            _currBn = new BayesianNetwork(_originalBn);
            _outedNodes = new List<int>();
            if (_currBn._nodes[nodeID].isObserved())
            {
                List<double> ans = new List<double>();
                for (int i = 0; i < _currBn._nodes[nodeID]._domain; i++)
                    if (i == _currBn._nodes[nodeID].getObservedValue())
                        ans.Add(1);
                    else
                        ans.Add(0);
                return ans;
            }
            GC.Collect();
            List<Node> hiddenNodes = new List<Node>();
            foreach (var node in _originalBn._nodes)
            {
                if (//!node.Value.isObserved() && 
                    node.Key != nodeID)
                    hiddenNodes.Add(node.Value);
            }
            List<Node> eliminationOrder = chooseEliminationOrder(hiddenNodes);
            while (eliminationOrder.Count > 0)
            {

                Node currElimination = eliminationOrder.First();
                //Node currElimination = eliminationOrder.Last();

                if (currElimination._nodeID == 23)
                    Console.WriteLine();
                Cpt joinedCpt = computeJoinedCpt(currElimination._nodeID, observations);
                if (joinedCpt != null)
                {
                    Cpt summingOutCpt = summingOutNode(currElimination._nodeID, joinedCpt, observations);
                    if (summingOutCpt == null)
                        Console.WriteLine();
                    _currBn._cpts.Remove(currElimination._nodeID);
                    //addCPT(currElimination._nodeID, summingOutCpt);
                    _currBn._cpts.Add(currElimination._nodeID, summingOutCpt);
                    _outedNodes.Add(currElimination._nodeID);

                }
                eliminationOrder.RemoveAt(0);

                //eliminationOrder.RemoveAt(eliminationOrder.Count - 1);

                //Console.WriteLine("node num. " + currElimination._nodeID + " has been removed");
            }

            //no hidden vars --> join all remaining factors
            Cpt finalCpt = joinAllRemainingFactors(observations);
            if (finalCpt == null)
            {
                if (verifyAllFathersObserved(nodeID, observations))
                    finalCpt = _currBn._cpts[nodeID];
            }
            //SumOut all remaining unwanted vars
            while (finalCpt._rowsProbs.Count > _currBn._nodes[nodeID]._domain)
            {
                int outVar = pickOutVar(finalCpt, nodeID);
                Cpt summingOut = summingOutNode(outVar, finalCpt, observations);
                finalCpt = summingOut;
            }
            List<double> distribution = normalize(finalCpt);
            return distribution;
        }
        //----------------------------------------------------------------------------------------------------------------------
        public List<Node> chooseEliminationOrder(List<Node> hiddenNodes)
        {
            return hiddenNodes;
        }
        //----------------------------------------------------------------------------------------------------------------------
        public Cpt computeJoinedCpt(int joinedVar, Dictionary<int, int> observations)
        {
            List<int> distinctFactors = new List<int>();
            List<int> factors = findFactorsOnJoin(joinedVar, ref distinctFactors);
            Cpt joinedCpt;
            if (factors.Count == 1 && !_outedNodes.Contains(factors[0]))
                joinedCpt = _currBn._cpts[joinedVar];  //its a table that already exists (source cpt)--> there is nothing to join with
            /*
            if (factors.Count == 1 && !_outedNodes.Contains(factors[0]))
                return null;*/
            else
            {
                CptRow fictiveRow = new CptRow();
                foreach (var x in distinctFactors)
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
                        bool b = checkAvailablePerm(row, observations);
                        if (b)
                        {

                            double prob = calculateJoinedProb(factors, row);
                            row._prob = prob;
                            joinedCpt.addRow(row);
                        }

                        else
                        {
                            joinedCpt._currAddedPointer++;
                        }
                    }
                }
                else
                {
                    //the cpt is on the disk

                }
            }
            //remove factors who were joined
            foreach (int factor in factors)
            {
                _currBn._factors.Remove(factor);
                _currBn._cpts.Remove(factor);
            }
            _currBn._factors.Remove(joinedVar);
            //add the function that replace the factors who were gone
            distinctFactors.Remove(joinedVar);
            _currBn._factors.Add(joinedVar, distinctFactors);



            return joinedCpt;
        }

        //--------------------------------------------------------------------------------------------------------------------

        public Cpt summingOutNode(int varOut, Cpt joinedCpt, Dictionary<int, int> observations)
        {

            int nodeOutPlace = 0;
            Node nodeOut = null;
            if (!joinedCpt._rowStructure.ContainsKey(varOut))
                return null;
                //throw new InvalidOperationException("cant out var that is not in the table");
            else
                nodeOut = joinedCpt._rowStructure[varOut];
            /*
            if (nodeOut._nodeID != joinedCpt._rowStructure.ElementAt(0).Value._nodeID)
                return null;*/
                //throw new InvalidOperationException("outed node is must be in the first coulmn from left");
            List<Node> rowStructure = joinedCpt.getRowStructure();
            CptRow fictiveRowStructureToSearch = joinedCpt.buildFictiveRowWithNewVar(new CptRow(rowStructure));

            rowStructure.Remove(nodeOut);
            //Cpt ans = new Cpt(rowStructure);
            CptRow newTableRowStructure = new CptRow(rowStructure);
            List<string> permutations = buildAllPermutationInDomain(newTableRowStructure);
            Cpt newCpt = new Cpt(rowStructure);

            foreach (var perm in permutations)
            {
                CptRow row = new CptRow(newTableRowStructure);
                row.insertValues(perm);
                bool b = checkAvailablePerm(row, observations);
                if (b)
                {
                    joinedCpt.sumReleventRowsFit(row, ref fictiveRowStructureToSearch, nodeOut);
                    newCpt.addRow(row);
                }
                else
                {
                    newCpt._currAddedPointer++;
                }
            }


            return newCpt;
        }

        //--------------------------------------------------------------------------------------------------------------------
        private double calculateJoinedProb(List<int> factors, CptRow rowObservation)
        {
            ///////
            double prob = 1;
            foreach (var factor in factors)
            {
                Cpt currTable = _currBn._cpts[factor];
                double currProb = currTable.getRowProb(rowObservation);
                prob *= currProb;
            }
            return prob;

        }
        //--------------------------------------------------------------------------------------------------------------------
        private List<int> findFactorsOnJoin(int joinedVar, ref List<int> distinctFactors)
        {
            List<int> ans = new List<int>();
            distinctFactors.Add(joinedVar);

            if (_currBn._factors.ContainsKey(joinedVar))
            {
                ans.Add(joinedVar);
                foreach (var x in _currBn._factors[joinedVar])
                {
                    if (distinctFactors.Contains(x))
                        throw new InvalidOperationException();
                    distinctFactors.Add(x);
                }
            }
            foreach (var x in _currBn._factors)
            {
                if (x.Value.Contains(joinedVar))
                {
                    ans.Add(x.Key);
                    //if (!_outedNodes.Contains(x.Key) && !distinctFactors.Contains(x.Key))
                    if (!_outedNodes.Contains(x.Key))
                        distinctFactors.Add(x.Key);
                    foreach (var y in x.Value)
                    {
                        if (!distinctFactors.Contains(y))
                            distinctFactors.Add(y);
                    }
                }
            }
            HashSet<int> set = new HashSet<int>(distinctFactors);
            distinctFactors = set.ToList();

            //_outedNodes.Add(joinedVar);
            return ans;
        }


        //--------------------------------------------------------------------------------------------------------------------

        private List<double> normalize(Cpt cpt)
        {
            double denominator = 0;
            foreach (var rowNumber in cpt._rowsProbs)
            {
                denominator += rowNumber.Value;
            }
            List<double> disAns = new List<double>();
            foreach (var rowNumber in cpt._rowsProbs)
            {
                disAns.Add(rowNumber.Value / denominator);
            }
            return disAns;
        }

        //--------------------------------------------------------------------------------------------------------------------
        private int pickOutVar(Cpt cpt, int stayVar)
        {
            List<Node> rowStructure = cpt.getRowStructure();
            foreach (var node in rowStructure)
            {
                if (node._nodeID != stayVar)
                    return node._nodeID;
            }
            return -1;
        }

        //--------------------------------------------------------------------------------------------------------------------


        private void addCPT(int varNum, ref Cpt cpt)
        {
            /*
            if (cpt._rowsProbs.Length <= MAX_ROWS_ON_DISK)
            {
                _currBn._cpts.Add(varNum, cpt);
            }
            else
            {
                //big table - write it to the disk
                File.Create(CPTS_PATH + @"/" + varNum + ".txt").Close();
            }*/
        }









        //--------------------------------------------------------------------------------------------------------------------
        public bool checkAvailablePerm(CptRow row, Dictionary<int, int> observations)
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

        //--------------------------------------------------------------------------------------------------------------------

        public Cpt joinAllRemainingFactors(Dictionary<int, int> observations)
        {
            List<int> distinctFactors = new List<int>();
            List<int> factors = new List<int>();
            foreach (var factorPair in _currBn._factors)
            {
                factors.Add(factorPair.Key);
                foreach (Node node in _currBn._cpts[factorPair.Key].getRowStructure())
                {
                    if (!distinctFactors.Contains(node._nodeID))
                        distinctFactors.Add(node._nodeID);
                }
            }

            CptRow fictiveRow = new CptRow();
            foreach (var x in distinctFactors)
            {
                Node node = new Node(_originalBn._nodes[x]);
                fictiveRow._vars.Add(x, node);
            }
            //calculate for each row the multiplication of each factor
            List<string> permutations = buildAllPermutationInDomain(fictiveRow);
            //Console.WriteLine(permutations.Count);
            Cpt joinedCpt = new Cpt(fictiveRow._vars.Values.ToList());

            if (permutations.Count > 0)
            {
                foreach (var perm in permutations)
                {
                    CptRow row = new CptRow(fictiveRow);
                    row.insertValues(perm);
                    bool b = checkAvailablePerm(row, observations);
                    if (b)
                    {

                        double prob = calculateJoinedProb(factors, row);
                        row._prob = prob;
                        joinedCpt.addRow(row);
                    }

                    else
                    {
                        joinedCpt._currAddedPointer++;
                    }
                }
            }
            else
            {
                //the cpt is on the disk

            }
            
            //remove factors who were joined
            foreach (int factor in factors)
            {
                _currBn._factors.Remove(factor);
                _currBn._cpts.Remove(factor);
            }


            return joinedCpt;
        }




        //--------------------------------------------------------------------------------------------------------------------
        private bool verifyAllFathersObserved(int nodeID, Dictionary<int, int> observations)
        {
            List<int> fathers = _currBn._factors[nodeID];
            foreach (var factor in fathers)
            {
                if (!_currBn._nodes[factor].isObserved())
                    return false;
            }
            return true;
        }
        //--------------------------------------------------------------------------------------------------------------------
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

        //-----------------------------------------------------------------------------------------------------------------------------

        private static void writePermsToDisk(ref List<string> perms, ref bool writenToDisk)
        {
            File.Create(CPTS_PATH + @"perms.txt").Close();
            StreamWriter sw = new StreamWriter(CPTS_PATH + @"perms.txt");
            foreach (var perm in perms)
            {
                sw.WriteLine(perm);
            }
            sw.Close();
            perms = new List<string>();
            writenToDisk = true;
            GC.Collect();
        }

        //-----------------------------------------------------------------------------------------------------------------------------









        //-----------------------------------------------------------------------------------------------------------------------------
    }
}
